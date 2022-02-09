using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Tracing;
using HeuristicLab.TruffleConnector.Messages;
using HeuristicLab.TruffleConnector.Operators;
using GroupSymbol = HeuristicLab.TruffleConnector.Messages.GroupSymbol;
using Symbol = HeuristicLab.TruffleConnector.Messages.Symbol;

namespace HeuristicLab.TruffleConnector {
  /// <summary>
  ///   Represents a Truffle problem.
  /// </summary>
  [Item("Truffle Problem", "Represents a Truffle problem.")]
  [Creatable(CreatableAttribute.Categories.Problems, Priority = 100)]
  [StorableType("3AE0F9F3-162E-4046-8950-303EA34500A1")]
  public sealed class TruffleProblem : SymbolicExpressionTreeProblem {
    [Storable]
    private readonly IConstrainedValueParameter<TruffleGrammar>
      grammarParam;

    [Storable]
    private readonly IFixedValueParameter<StringValue> runIdParam;

    [Storable]
    private readonly IFixedValueParameter<TruffleTestCase> testCaseParam;

    /// <summary>
    ///   Grammar for the language of the source code.
    /// </summary>
    public TruffleGrammar Grammar => grammarParam.Value;

    public override bool Maximization { get; }

    /// <summary>
    ///   ID of the current run, used by operators to send associated messages.
    /// </summary>
    public string RunId {
      get => runIdParam.Value.Value;
      set => runIdParam.Value.Value = value;
    }

    /// <summary>
    ///   Test case that contains source code, input and output data.
    /// </summary>
    public TruffleTestCase TestCase => testCaseParam.Value;

    private TruffleProblem(TruffleProblem original, Cloner cloner) : base(original, cloner) {
      testCaseParam = cloner.Clone(original.testCaseParam);
      grammarParam = cloner.Clone(original.grammarParam);
      runIdParam = cloner.Clone(original.runIdParam);
    }

    public TruffleProblem() {
      Parameters.Add(testCaseParam = new FixedValueParameter<TruffleTestCase>(nameof(TestCase),
        "Test case that contains source code, input and output data.",
        new TruffleTestCase()));
      Parameters.Add(grammarParam = new ConstrainedValueParameter<TruffleGrammar>(nameof(Grammar),
        "Grammar for the language of the source code."));
      Parameters.Add(
        runIdParam = new FixedValueParameter<StringValue>(nameof(RunId), new StringValue()) {Hidden = true});

      Parameters["Evaluator"].Hidden = true;
      Parameters["Encoding"].Hidden = true;
      Parameters["SolutionCreator"].Hidden = true;

      grammarParam.ValueChanged +=
        (s, e) => ((SymbolicExpressionTreeEncoding) Parameters["Encoding"].ActualValue).Grammar = Grammar;

      LoadConfiguration();
    }

    [StorableConstructor]
    private TruffleProblem(StorableConstructorFlag _) : base(_) { }

    public override IDeepCloneable Clone(Cloner cloner) => new TruffleProblem(this, cloner);

    /// <summary>
    ///   Sends a request to the Truffle backend to evaluate a solution.
    /// </summary>
    public override double Evaluate(ISymbolicExpressionTree tree, IRandom random) {
      var request = new EvaluatorRequest {
        SolutionId = ((TruffleTree) tree).Id
      };

      var response = request.Send<EvaluatorResponse>(RunId);
      if (response.SolutionId != request.SolutionId)
        throw new InvalidDataException("Received invalid EvaluatorResponse.");

      return response.Quality;
    }

    private void LoadConfiguration() {
      var request = new ConfigurationRequest();
      var response = request.Send<ConfigurationResponse>();

      LoadLanguages(response.Languages);
      ConfigureOperators(response.OptionGroup);
    }

    private void ConfigureOperators(OptionGroup optionGroup) {
      var operatorParameters = new Dictionary<string, ConstrainedValueParameter<ITruffleOption>>();
      var groupTypes = new Dictionary<string, Type> {
        {"SolutionCreator", typeof(TruffleSolutionCreator)},
        {"Crossover", typeof(TruffleCrossover)},
        {"Mutator", typeof(TruffleManipulator)},
        {"Evaluator", typeof(TruffleOption)}
      };

      foreach (var constrainedOption in optionGroup.ConstrainedOptions) {
        // prefix with "Truffle" as parameters with the same names already exist
        Parameters.Add(operatorParameters[constrainedOption.Name] = new ConstrainedValueParameter<ITruffleOption>(
          $"Truffle{constrainedOption.Name}", constrainedOption.Description));

        foreach (var validValue in constrainedOption.ValidValues) {
          if (!groupTypes.TryGetValue(constrainedOption.Name, out var optionType))
            optionType = typeof(TruffleOption);

          var option = (ITruffleOption) Activator.CreateInstance(optionType,
            validValue.Name, validValue.Description);
          operatorParameters[constrainedOption.Name].ValidValues.Add(option);
          AddParameters(option.GetParameters(), validValue.Options);
          AddConstrainedParameters(option.GetParameters(), validValue.ConstrainedOptions);
          AddMultiParameters(option.GetParameters(), validValue.MultiOptions);

          if (option is TruffleCrossover || option is TruffleManipulator) {
            Operators.Add(option);
          }
        }

        if (constrainedOption.Name == "SolutionCreator") {
          operatorParameters[constrainedOption.Name].ValueChanged += (s, e) =>
            SolutionCreator = (ISolutionCreator) operatorParameters[constrainedOption.Name].ActualValue;
          // update SolutionCreator in case the first element is the default
          SolutionCreator = (ISolutionCreator) operatorParameters[constrainedOption.Name].ActualValue;
        }

        operatorParameters[constrainedOption.Name].ActualValue = operatorParameters[constrainedOption.Name].ValidValues
          .ElementAt(constrainedOption.Default);
      }

      AddParameters(Parameters, optionGroup.Options);
      AddMultiParameters(Parameters, optionGroup.MultiOptions);
    }

    private static void AddConstrainedParameters(ParameterCollection parameterCollection,
      IEnumerable<ConstrainedOption> optionGroups) {
      var parameters = new Dictionary<string, ConstrainedValueParameter<ITruffleOption>>();

      foreach (var constrainedOption in optionGroups) {
        parameterCollection.Add(parameters[constrainedOption.Name] =
          new ConstrainedValueParameter<ITruffleOption>(constrainedOption.Name, constrainedOption.Description));

        foreach (var validValue in constrainedOption.ValidValues) {
          var truffleOption = new TruffleOption(validValue.Name, validValue.Description);
          parameters[constrainedOption.Name].ValidValues.Add(truffleOption);

          AddParameters(truffleOption.GetParameters(), validValue.Options);
          AddConstrainedParameters(truffleOption.GetParameters(), validValue.ConstrainedOptions);
          AddMultiParameters(truffleOption.GetParameters(), validValue.MultiOptions);
        }

        parameters[constrainedOption.Name].ActualValue =
          parameters[constrainedOption.Name].ValidValues.ElementAt(constrainedOption.Default);
      }
    }

    private static void AddMultiParameters(ParameterCollection parameterCollection,
      IEnumerable<MultiOption> optionGroups) {
      var parameters = new Dictionary<string, FixedValueParameter<CheckedItemList<ITruffleOption>>>();

      foreach (var constrainedOption in optionGroups) {
        parameterCollection.Add(parameters[constrainedOption.Name] =
          new FixedValueParameter<CheckedItemList<ITruffleOption>>(constrainedOption.Name, constrainedOption.Description));

        var i = 0;
        foreach (var item in constrainedOption.Items) {
          var truffleOption = new TruffleOption(item.Name, item.Description);
          parameters[constrainedOption.Name].Value.Add(truffleOption, constrainedOption.Defaults.Contains(i));

          AddParameters(truffleOption.GetParameters(), item.Options);
          AddConstrainedParameters(truffleOption.GetParameters(), item.ConstrainedOptions);
          AddMultiParameters(truffleOption.GetParameters(), item.MultiOptions);
          i++;
        }
      }
    }

    private static void AddParameters(ParameterCollection parameterCollection, IEnumerable<Option> options) {
      foreach (var option in options) {
        var types = new Dictionary<OptionType, (Type, Func<string, IStringConvertibleValue>)> {
          {OptionType.Int, (typeof(IntValue), v => new IntValue(int.Parse(v)))},
          {OptionType.String, (typeof(StringValue), v => new StringValue(v))}, {
            OptionType.Double,
            (typeof(DoubleValue), v => new DoubleValue(double.Parse(v, CultureInfo.InvariantCulture)))
          },
          {OptionType.Bool, (typeof(BoolValue), v => new BoolValue(bool.Parse(v)))}, {
            OptionType.Percent,
            (typeof(PercentValue), v => new PercentValue(double.Parse(v, CultureInfo.InvariantCulture)))
          },
          {OptionType.DateTime, (typeof(DateTimeValue), v => new DateTimeValue(DateTime.Parse(v)))},
          {OptionType.TimeSpan, (typeof(TimeSpanValue), v => new TimeSpanValue(XmlConvert.ToTimeSpan(v)))}
        };

        if (!types.TryGetValue(option.Type, out var typeParse))
          throw new ArgumentOutOfRangeException(nameof(OptionType), option.Type,
            $"{nameof(OptionType)} '{option.Type}' is unknown.");
        var (type, parse) = typeParse;

        var parameter = (IFixedValueParameter) Activator.CreateInstance(
          typeof(FixedValueParameter<>).MakeGenericType(type),
          option.Name, option.Description, parse(option.Default));

        parameterCollection.Add(parameter);
      }
    }

    /// <summary>
    ///   Collects the values of all options/parameters of the problem recursively.
    /// </summary>
    /// <param name="values">The collected values.</param>
    public void CollectOptionValues(IDictionary<string, string> values) {
      CollectOptionValues(GetParameters(), values, "");
    }

    private static void CollectOptionValues(ParameterCollection parameters, IDictionary<string, string> values,
      string prefix) {
      foreach (var parameter in parameters) {
        var normalizedName = parameter.Name.StartsWith("Truffle")
          ? parameter.Name.Remove(0, "Truffle".Length)
          : parameter.Name;
        var prefixedName = string.IsNullOrEmpty(prefix)
          ? normalizedName
          : $"{prefix}.{normalizedName}";

        try {
          switch (parameter.ActualValue) {
            case IStringConvertibleValue value: // option
              string paramValue;

              switch (value) {
                case TimeSpanValue timeSpanValue:
                  paramValue = XmlConvert.ToString(timeSpanValue.Value);
                  break;
                case DoubleValue doubleValue: // also covers PercentValue
                  paramValue = doubleValue.Value.ToString(CultureInfo.InvariantCulture);
                  break;
                default:
                  paramValue = value.ToString();
                  break;
              }

              values.Add(prefixedName, paramValue);
              break;

            case ITruffleOption truffleOption: // constrained option
              var cvp = (IConstrainedValueParameter<ITruffleOption>) parameter;
              values.Add(prefixedName, cvp.ValidValues.ToList().IndexOf(truffleOption).ToString());
              CollectOptionValues(truffleOption.GetParameters(), values, prefixedName);
              break;

            case ICheckedItemList<ITruffleOption> checkedItemList: // multi-option
              var fvp = (IFixedValueParameter<CheckedItemList<ITruffleOption>>) parameter;
              values.Add(prefixedName, string.Join(", ", fvp.Value.CheckedItems.Select(i => i.Index)));
              foreach (var item in fvp.Value.CheckedItems) {
                CollectOptionValues(item.Value.GetParameters(), values, $"{prefixedName}.{item.Value.Name}");
              }
              break;
          }
        } catch (NullReferenceException ex) {
          // accessing parameter.ActualValue sometimes causes a NullReferenceException,
          // these values can be ignored as they were not created by the Truffle backend
          Logger.Warn(typeof(TruffleAlgorithm), $"Could not collect value of parameter {prefixedName}.", ex);
        } catch (InvalidCastException ex) {
          // casting fails for parameters that were not created by Truffle backend,
          // these values can be ignored
          Logger.Warn(typeof(TruffleAlgorithm), $"Could not collect value of parameter {prefixedName}.", ex);
        }
      }
    }

    private void LoadLanguages(IEnumerable<Language> languages) {
      // symbol ID to symbol (both ITruffleSymbol and GroupSymbol)
      var symbolsCache = new Dictionary<long, ISymbol>();
      // parent to (childId, argumentIndices)
      var allowedChildSymbolsCache = new Dictionary<ISymbol, Dictionary<long, List<int>>>();

      foreach (var language in languages) {
        var grammar = new TruffleGrammar(language.Id, $"{language.Name} Grammar", language.Description);
        symbolsCache.Clear();
        allowedChildSymbolsCache.Clear();

        // add symbols to grammar
        AddSymbols(language.Symbols, language.Groups, s => {
          if (s is TruffleSymbol ts) {
            grammar.AddSymbol(ts);
          } else {
            grammar.AddSymbol(s);
          }
        }, (s, id, allowedChildSymbols) => {
          symbolsCache[id] = s;
          allowedChildSymbolsCache[s] = allowedChildSymbols.ToDictionary(
            config => config.ChildId,
            config => config.ArgumentIndices.ToList());
        });

        // add allowed child symbols to grammar
        foreach (var parentEntry in allowedChildSymbolsCache) {
          var parent = parentEntry.Key;
          var allowedChildSymbols = parentEntry.Value;

          foreach (var childEntry in allowedChildSymbols) {
            var child = symbolsCache[childEntry.Key];
            var argumentIndices = childEntry.Value;

            if (childEntry.Value.Any()) {
              foreach (var index in argumentIndices) {
                grammar.AddAllowedChildSymbol(parent, child, index);
              }
            } else {
              grammar.AddAllowedChildSymbol(parent, child);
            }
          }
        }

        grammar.ReadOnly = true;
        grammarParam.ValidValues.Add(grammar);
      }

      Encoding.Grammar = grammarParam.ValidValues.First();
    }

    private static void AddSymbols(IEnumerable<Symbol> symbols, IEnumerable<GroupSymbol> groups,
      Action<ISymbol> add, Action<ISymbol, long, IEnumerable<ChildSymbolConfiguration>> cache) {
      foreach (var s in symbols) {
        var symbol = new TruffleSymbol(s.Id, s.Name, s.Description, s.MinimumArity, s.MaximumArity) {
          InitialFrequency = s.InitialFrequency,
          Enabled = s.Enabled
        };

        add(symbol);
        cache(symbol, s.Id, s.AllowedChildSymbols);
      }

      foreach (var g in groups) {
        var group = new Encodings.SymbolicExpressionTreeEncoding.GroupSymbol {
          Name = g.Name
          // Description cannot be changed for GroupSymbols (sealed)
        };

        add(group);
        cache(group, g.Id, g.AllowedChildSymbols);
        
        AddSymbols(g.Symbols, g.Groups, s => group.SymbolsCollection.Add(s), cache);
        group.Enabled = group.SymbolsCollection.Any(s => s.Enabled);
      }
    }

    public ParameterCollection GetParameters() => Parameters;
  }
}

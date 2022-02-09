using System;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using HEAL.Attic;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.TruffleConnector.Messages;
using ExecutionState = HeuristicLab.Core.ExecutionState;
using StringValue = HeuristicLab.Data.StringValue;

namespace HeuristicLab.TruffleConnector {
  /// <summary>
  ///   An algorithm that accesses a Truffle backend.
  /// </summary>
  [Item("Truffle Algorithm", "An algorithm that accesses a Truffle backend.")]
  [Creatable(CreatableAttribute.Categories.Algorithms)]
  [StorableType("F1EC1375-77B0-4CC5-9BC5-BA31E732A910")]
  public sealed class TruffleAlgorithm : BasicAlgorithm {
    [Storable]
    private readonly IValueParameter<IAlgorithm> algorithmParam;

    /// <summary>
    ///   The underlying algorithm.
    /// </summary>
    public IAlgorithm Algorithm {
      get => algorithmParam.Value;
      set => algorithmParam.Value = value;
    }

    public override bool SupportsPause { get; } = true;

    public TruffleAlgorithm() {
      Parameters.Add(algorithmParam = new ValueParameter<IAlgorithm>(nameof(Algorithm),
        "The underlying algorithm."));

      algorithmParam.ValueChanged += (sender, args) => {
        Algorithm.ExecutionStateChanged -= OnAlgorithmOnExecutionStateChanged;
        Algorithm.ExecutionStateChanged += OnAlgorithmOnExecutionStateChanged;
      };

      Problem = new TruffleProblem();

      Algorithm = new GeneticAlgorithm {
        Problem = (ISingleObjectiveHeuristicOptimizationProblem) Problem
      };

      void OnAlgorithmOnExecutionStateChanged(object sender, EventArgs args) {
        if (!new[] {ExecutionState.Paused, ExecutionState.Stopped}.Contains(Algorithm.ExecutionState))
          return;

        var problem = (TruffleProblem) Problem;
        new StopAlgorithmRequest {
          ExecutionState = (Messages.ExecutionState) Algorithm.ExecutionState
        }.Send<Empty>(problem.RunId);
      }
    }

    [StorableConstructor]
    private TruffleAlgorithm(StorableConstructorFlag _) : base(_) { }

    private TruffleAlgorithm(TruffleAlgorithm original, Cloner cloner)
      : base(original, cloner) {
      algorithmParam = cloner.Clone(original.algorithmParam);
    }

    public override IDeepCloneable Clone(Cloner cloner) => new TruffleAlgorithm(this, cloner);

    /// <summary>
    ///   Sends a Meta message to the Truffle backend and starts the underlying algorithm.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that is forwarded to the underlying algorithm.</param>
    protected override void Run(CancellationToken cancellationToken) {
      if (Algorithm == null)
        throw new InvalidOperationException($"Invalid value for parameter {nameof(Algorithm)}.");

      var problem = (TruffleProblem) Problem;
      problem.RunId = Guid.NewGuid().ToString();
      var request = BuildStartAlgorithmRequest(problem);

      var response = request.Send<StartAlgorithmResponse>(problem.RunId, new WorkerConfig {
        LanguageId = request.ProblemDefinition.LanguageId
      });

      if (!response.Success)
        throw new Exception(response.ErrorMessage);

      Algorithm.Problem = problem;

      if (Algorithm is GeneticAlgorithm ga) {
        ga.Crossover = (ICrossover) Problem.Parameters["TruffleCrossover"].ActualValue;
        ga.Mutator = (IManipulator) Problem.Parameters["TruffleMutator"].ActualValue;
      }

      Algorithm.Prepare();
      Algorithm.Start(cancellationToken);
      Results.AddRange(Algorithm.Results);
    }

    [StorableHook(HookType.BeforeSerialization)]
    private void BeforeSerialization() {
      // GeneticAlgorithm uses the following code in its AfterDeserialization method to maintain compatibility:
      //   if (optionalMutatorParameter.Value == null) MutationProbability.Value = 0; // to guarantee that the old configuration results in the same behavior
      //   else Mutator = optionalMutatorParameter.Value;
      // When the GeneticAlgorithm is used as parameter, it is cloned to the Parameters section of a Run.
      // The ActualValue of the Mutator parameter is lost when it is cloned but it is required for deserialization.
      // Thus, the following code restores the ActualValue of the Mutator parameter before serialization, if GeneticAlgorithm is used:

      foreach (var run in Runs) {
        var algorithmParameter = run.Parameters[nameof(Algorithm)];
        if (!(algorithmParameter is GeneticAlgorithm ga))
          continue;

        var mutatorParam = ga.MutatorParameter;
        var mutatorName = ((StringValue) run.Parameters[$"{nameof(Algorithm)}.Mutator"]).Value;
        mutatorParam.ActualValue = mutatorParam.ValidValues.First(m => m.Name == mutatorName);
      }
    }

    public override void Pause() {
      base.Pause();
      Algorithm?.Pause();
    }

    public override void Stop() {
      base.Stop();
      Algorithm?.Stop();
    }

    public override void Prepare() {
      base.Prepare();
      Algorithm?.Prepare();
    }

    private static StartAlgorithmRequest BuildStartAlgorithmRequest(TruffleProblem problem) {
      var request = new StartAlgorithmRequest {
        ProblemDefinition = new ProblemDefinition {
          LanguageId = problem.Grammar.Id,
          SourceCode = File.ReadAllText(problem.TestCase.SourceCode),
          Input = !string.IsNullOrEmpty(problem.TestCase.Input)
            ? File.ReadAllText(problem.TestCase.Input)
            : string.Empty,
          Output = !string.IsNullOrEmpty(problem.TestCase.Output)
            ? File.ReadAllText(problem.TestCase.Output)
            : string.Empty,
          FunctionName = problem.TestCase.FunctionName
        },
        Started = Timestamp.FromDateTime(DateTime.UtcNow)
      };

      problem.CollectOptionValues(request.OptionConfiguration);

      request.SymbolConfiguration.AddRange(problem.Grammar.Symbols
        .OfType<TruffleSymbol>()
        .Select(s => new SymbolConfiguration {
          SymbolId = s.Id,
          Enabled = s.Enabled,
          InitialFrequency = s.InitialFrequency
        })
      );

      return request;
    }
  }
}

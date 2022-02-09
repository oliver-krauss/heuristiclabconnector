using System;
using System.IO;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;
using HeuristicLab.TruffleConnector.Messages;
using HeuristicLab.TruffleConnector.Operators;

namespace HeuristicLab.TruffleConnector {
  /// <summary>
  ///   Crosses two solutions using the Truffle backend.
  /// </summary>
  [Item(nameof(TruffleCrossover), "Crosses two solutions using the Truffle backend.")]
  [StorableType("14B748D0-96A9-4C4C-A832-5F667C0C059D")]
  public sealed class TruffleCrossover : SymbolicExpressionTreeCrossover, ITruffleOperator {
    [Storable]
    private readonly ILookupParameter<TruffleGrammar> grammarParam;

    [Storable]
    private readonly IFixedValueParameter<BoolValue> localSelfCrossingParam;

    [Storable]
    private readonly ILookupParameter<StringValue> runIdParam;

    public TruffleGrammar Grammar {
      get => grammarParam.ActualValue;
      set => grammarParam.ActualValue = value;
    }

    /// <summary>
    ///   If two solutions are identical, return the first parent (instead of using the Truffle backend).
    /// </summary>
    public bool LocalSelfCrossing {
      get => localSelfCrossingParam.Value.Value;
      set => localSelfCrossingParam.Value.Value = value;
    }

    public string RunId {
      get => runIdParam.ActualValue.Value;
      set => runIdParam.ActualValue.Value = value;
    }

    public TruffleCrossover() {
      Parameters.Add(localSelfCrossingParam =
        new FixedValueParameter<BoolValue>(nameof(LocalSelfCrossing),
          "If two solutions are identical, return the first parent (instead of using the Truffle backend).",
          new BoolValue()) {Hidden = true});
      Parameters.Add(grammarParam = new LookupParameter<TruffleGrammar>(nameof(Grammar)));
      Parameters.Add(runIdParam = new LookupParameter<StringValue>(nameof(RunId)));
    }

    public TruffleCrossover(string name, string description) : this() {
      this.name = name;
      this.description = description;
    }

    [StorableConstructor]
    private TruffleCrossover(StorableConstructorFlag _) : base(_) { }

    private TruffleCrossover(TruffleCrossover original, Cloner cloner) {
      name = original.Name;
      description = original.description;
      localSelfCrossingParam = cloner.Clone(original.localSelfCrossingParam);
      grammarParam = cloner.Clone(original.grammarParam);
      runIdParam = cloner.Clone(original.runIdParam);
    }

    public override IDeepCloneable Clone(Cloner cloner) => new TruffleCrossover(this, cloner);

    public ParameterCollection GetParameters() => Parameters;

    /// <summary>
    ///   Sends a request to the Truffle backend to cross two parent solutions.
    /// </summary>
    /// <param name="random"></param>
    /// <param name="parent0">The first parent solution.</param>
    /// <param name="parent1">The second parent solution.</param>
    /// <returns>The child solution.</returns>
    public override ISymbolicExpressionTree Crossover(IRandom random, ISymbolicExpressionTree parent0,
      ISymbolicExpressionTree parent1) {
      if (!(parent0 is TruffleTree truffleParent1))
        throw new ArgumentException($"{nameof(parent0)} must be a {nameof(TruffleTree)}.");
      if (!(parent1 is TruffleTree truffleParent2))
        throw new ArgumentException($"{nameof(parent1)} must be a {nameof(TruffleTree)}.");

      if (LocalSelfCrossing && truffleParent1.Equals(truffleParent2))
        return truffleParent1;

      var request = new CrossoverRequest {
        ParentSolutionId1 = truffleParent1.Id,
        ParentSolutionId2 = truffleParent2.Id
      };

      var response = request.Send<CrossoverResponse>(RunId);
      if (response.ParentSolutionId1 != request.ParentSolutionId1 ||
          response.ParentSolutionId2 != request.ParentSolutionId2)
        throw new InvalidDataException($"Received invalid {nameof(CrossoverResponse)}.");

      var childTree = response.Tree.ToSymbolicExpressionTree(response.ChildSolutionId, Grammar);
      return childTree;
    }
  }
}

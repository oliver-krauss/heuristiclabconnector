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
  ///   Manipulates a solution using the Truffle backend.
  /// </summary>
  [Item(nameof(TruffleManipulator), "Manipulates a solution using the Truffle backend.")]
  [StorableType("4E27CED8-0857-4C04-B22C-4E323901CD7E")]
  public sealed class TruffleManipulator : SymbolicExpressionTreeManipulator, ITruffleOperator {
    [Storable]
    private readonly ILookupParameter<TruffleGrammar> grammarParam;

    [Storable]
    private readonly ILookupParameter<StringValue> runIdParam;

    public TruffleGrammar Grammar {
      get => grammarParam.ActualValue;
      set => grammarParam.ActualValue = value;
    }

    public string RunId {
      get => runIdParam.ActualValue.Value;
      set => runIdParam.ActualValue.Value = value;
    }

    public TruffleManipulator() {
      Parameters.Add(grammarParam = new LookupParameter<TruffleGrammar>(nameof(Grammar)));
      Parameters.Add(runIdParam = new LookupParameter<StringValue>(nameof(RunId)));
    }

    public TruffleManipulator(string name, string description) : this() {
      this.name = name;
      this.description = description;
    }

    [StorableConstructor]
    private TruffleManipulator(StorableConstructorFlag _) : base(_) { }

    private TruffleManipulator(TruffleManipulator original, Cloner cloner) {
      name = original.name;
      description = original.description;
      grammarParam = cloner.Clone(original.grammarParam);
      runIdParam = cloner.Clone(original.runIdParam);
    }

    public override IDeepCloneable Clone(Cloner cloner) => new TruffleManipulator(this, cloner);

    public ParameterCollection GetParameters() => Parameters;

    /// <summary>
    ///   Sends a request to the Truffle backend to manipulate a solution.
    /// </summary>
    /// <param name="random"></param>
    /// <param name="symbolicExpressionTree">The solution to manipulate.</param>
    protected override void Manipulate(IRandom random, ISymbolicExpressionTree symbolicExpressionTree) {
      if (!(symbolicExpressionTree is TruffleTree truffleTree))
        throw new ArgumentException($"{nameof(symbolicExpressionTree)} must be a {nameof(TruffleTree)}.");

      var request = new ManipulatorRequest {
        SolutionId = truffleTree.Id
      };

      var response = request.Send<ManipulatorResponse>(RunId);
      if (response.SolutionId != request.SolutionId)
        throw new InvalidDataException($"Received invalid {nameof(ManipulatorResponse)}.");

      var manipulatedTree = response.Tree.ToSymbolicExpressionTree(response.ManipulatedSolutionId, Grammar);
      truffleTree.Id = manipulatedTree.Id;
      symbolicExpressionTree.Root = manipulatedTree.Root;
    }
  }
}

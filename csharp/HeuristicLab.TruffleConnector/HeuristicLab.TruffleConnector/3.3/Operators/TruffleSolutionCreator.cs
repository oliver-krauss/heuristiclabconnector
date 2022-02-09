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
  ///   Requests a new solution from the Truffle backend.
  /// </summary>
  [Item(nameof(TruffleSolutionCreator), "Requests a new solution from the Truffle backend.")]
  [StorableType("92A97F74-3156-469E-9741-A194C5C2A022")]
  public class TruffleSolutionCreator : SymbolicExpressionTreeCreator, ITruffleOperator {
    [Storable]
    private readonly ILookupParameter<StringValue> runIdParam;

    public string RunId {
      get => runIdParam.ActualValue.Value;
      set => runIdParam.ActualValue.Value = value;
    }

    public TruffleSolutionCreator() {
      Parameters.Add(runIdParam = new LookupParameter<StringValue>(nameof(RunId)));
    }

    public TruffleSolutionCreator(string name, string description) : this() {
      this.name = name;
      this.description = description;
    }

    public TruffleSolutionCreator(TruffleSolutionCreator original, Cloner cloner) : base(original, cloner) {
      runIdParam = cloner.Clone(original.runIdParam);
    }

    [StorableConstructor]
    private TruffleSolutionCreator(StorableConstructorFlag _) : base(_) { }

    public override IDeepCloneable Clone(Cloner cloner) => new TruffleSolutionCreator(this, cloner);

    public ParameterCollection GetParameters() => Parameters;

    protected override ISymbolicExpressionTree Create(IRandom random) =>
      Create((TruffleGrammar) ClonedSymbolicExpressionTreeGrammarParameter.ActualValue, RunId);

    public override ISymbolicExpressionTree CreateTree(IRandom random,
      ISymbolicExpressionGrammar grammar, int maxTreeLength, int maxTreeDepth) =>
      Create((TruffleGrammar) grammar, RunId);

    /// <summary>
    ///   Sends a request to the Truffle backend to create a new solution and parses the result.
    /// </summary>
    /// <param name="grammar">The grammar to generate trees. Currently not in use.</param>
    /// <returns>The symbolic expression tree.</returns>
    private static TruffleTree Create(TruffleGrammar grammar, string runId) {
      var request = new SolutionCreatorRequest();
      var response = request.Send<SolutionCreatorResponse>(runId);
      var tree = response.Tree.ToSymbolicExpressionTree(response.SolutionId, grammar);
      return tree;
    }
  }
}

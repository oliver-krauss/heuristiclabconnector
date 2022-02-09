using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.TruffleConnector {
  /// <summary>
  ///   Represents a Truffle solution.
  /// </summary>
  [Item(nameof(TruffleTree), "Represents a Truffle solution.")]
  [NonDiscoverableType]
  [StorableType("FA261738-D2B0-4316-97FA-EEC1BB4F5121")]
  public sealed class TruffleTree : SymbolicExpressionTree {
    [field: Storable]
    public long Id { get; internal set; }

    [StorableConstructor]
    private TruffleTree(StorableConstructorFlag _) : base(_) { }

    private TruffleTree(TruffleTree original, Cloner cloner)
      : base(original, cloner) {
      Id = original.Id;
    }

    public TruffleTree(long id, ISymbolicExpressionTreeNode root) : base(root) {
      Id = id;
    }

    public override bool Equals(object obj) => (obj as TruffleTree)?.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();

    public override IDeepCloneable Clone(Cloner cloner) => new TruffleTree(this, cloner);
  }
}

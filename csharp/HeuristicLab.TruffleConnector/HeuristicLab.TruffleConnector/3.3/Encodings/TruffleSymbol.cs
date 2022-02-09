using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.TruffleConnector {
  /// <summary>
  ///   Represents a Truffle symbol.
  /// </summary>
  [Item(nameof(TruffleSymbol), "Represents a Truffle symbol.")]
  [NonDiscoverableType]
  [StorableType("BB1F0A9E-67C1-4470-B639-0A1AF17F4344")]
  public sealed class TruffleSymbol : Symbol {
    public override bool CanChangeDescription => false;

    public override bool CanChangeName => false;

    [Storable]
    private long id;

    [Storable]
    private int minimumArity;

    [Storable]
    private long maximumArity = byte.MaxValue;

    public long Id => id;

    public override int MaximumArity => minimumArity;

    public override int MinimumArity => minimumArity;

    [StorableConstructor]
    private TruffleSymbol(StorableConstructorFlag _) : base(_) { }

    private TruffleSymbol(TruffleSymbol original, Cloner cloner)
      : base(original, cloner) {
      id = original.Id;
      minimumArity = original.MinimumArity;
      maximumArity = original.MaximumArity;
    }

    public TruffleSymbol(long id, string name, int arity = int.MaxValue)
      : this(id, name, string.Empty, arity, arity) { }

    public TruffleSymbol(long id, string name, string description, int minimumArity, int maximumArity)
      : base(name, description) {
      this.id = id;
      this.minimumArity = minimumArity;
      this.maximumArity = maximumArity;
    }

    public override IDeepCloneable Clone(Cloner cloner) => new TruffleSymbol(this, cloner);
  }
}

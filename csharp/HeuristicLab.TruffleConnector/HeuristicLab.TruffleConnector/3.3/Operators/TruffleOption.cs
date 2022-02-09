using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;

namespace HeuristicLab.TruffleConnector.Operators {
  /// <summary>
  ///   Represents a non-first-tier Truffle option.
  /// </summary>
  [Item(nameof(TruffleOption), "")]
  [StorableType("A2225C6E-66E0-4909-91CF-537A5EF20F43")]
  public sealed class TruffleOption : ParameterizedNamedItem, ITruffleOption {
    public TruffleOption(string name, string description) : base(name, description) { }

    [StorableConstructor]
    private TruffleOption(StorableConstructorFlag _) : base(_) { }

    private TruffleOption(TruffleOption original, Cloner cloner)
      : base(original, cloner) { }

    public ParameterCollection GetParameters() => Parameters;

    public override IDeepCloneable Clone(Cloner cloner) => new TruffleOption(this, cloner);
  }
}

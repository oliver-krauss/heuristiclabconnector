using System.Linq;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;

namespace HeuristicLab.TruffleConnector {
  /// <summary>
  ///   Represents a Truffle language.
  /// </summary>
  [Item(nameof(TruffleGrammar), "Represents a Truffle language.")]
  [StorableType("36B9DB42-89EB-48ED-86A6-B10A7691356F")]
  public sealed class TruffleGrammar : SymbolicExpressionGrammar {
    public long Id { get; set; }

    [StorableConstructor]
    private TruffleGrammar(StorableConstructorFlag _) : base(_) { }

    private TruffleGrammar(TruffleGrammar original, Cloner cloner) : base(original, cloner) { }

    public TruffleGrammar(long id, string name, string description)
      : base(name, description) {
      Id = id;
    }

    public override IDeepCloneable Clone(Cloner cloner) => new TruffleGrammar(this, cloner);

    /// <summary>
    ///   Adds a symbol to the grammar.
    /// </summary>
    /// <param name="symbol">The symbol to add.</param>
    public void AddSymbol(TruffleSymbol symbol) {
      base.AddSymbol(symbol);
      SetSubtreeCount(symbol, symbol.MinimumArity, symbol.MaximumArity);

      foreach (var s in Symbols) {
        if (s == ProgramRootSymbol)
          continue;
        if (s.MaximumArity > 0) {
          AddAllowedChildSymbol(s, symbol);
        }

        if (s == DefunSymbol)
          continue;
        if (s == StartSymbol)
          continue;
        if (symbol.MaximumArity > 0) {
          AddAllowedChildSymbol(symbol, s);
        }
      }
    }

    /// <summary>
    ///   Looks up a Truffle symbol of the language by its ID.
    /// </summary>
    /// <param name="id">The ID of the symbol.</param>
    /// <returns>The Truffle symbol or null, if the symbol does not exist.</returns>
    public TruffleSymbol GetTruffleSymbol(long id) {
      var sy = symbols.Values.SingleOrDefault(s => s is TruffleSymbol ts && ts.Id == id);
      return (TruffleSymbol) sy;
    }
  }
}

using HEAL.Attic;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;

namespace HeuristicLab.TruffleConnector.Operators {
  /// <summary>
  ///   Represents a first-tier Truffle option and thus a Truffle operator.
  /// </summary>
  [StorableType("414E83C6-634A-408E-8A67-56E30E593DEF")]
  public interface ITruffleOperator : ISymbolicExpressionTreeOperator, ITruffleOption { }
}

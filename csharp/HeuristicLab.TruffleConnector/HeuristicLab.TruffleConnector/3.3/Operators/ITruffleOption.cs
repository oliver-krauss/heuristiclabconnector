using HEAL.Attic;
using HeuristicLab.Core;

namespace HeuristicLab.TruffleConnector.Operators {
  /// <summary>
  ///   Represents a Truffle option.
  /// </summary>
  [StorableType("DFF6E392-FBFA-475B-90A8-AC1341A05B4B")]
  public interface ITruffleOption : IParameterizedNamedItem {
    ParameterCollection GetParameters();
  }
}

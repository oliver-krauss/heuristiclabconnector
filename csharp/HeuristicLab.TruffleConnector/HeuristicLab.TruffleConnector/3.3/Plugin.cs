using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.TruffleConnector {
  /// <summary>
  ///   Plugin class for HeuristicLab.TruffleConnector.
  /// </summary>
  [Plugin("HeuristicLab.TruffleConnector", "3.3.15.0")]
  [PluginFile("HeuristicLab.TruffleConnector-3.3.dll", PluginFileType.Assembly)]
  [PluginDependency("HeuristicLab.Algorithms.GeneticAlgorithm", "3.3")]
  [PluginDependency("HeuristicLab.Collections", "3.3")]
  [PluginDependency("HeuristicLab.Common", "3.3")]
  [PluginDependency("HeuristicLab.Core", "3.3")]
  [PluginDependency("HeuristicLab.Data", "3.3")]
  [PluginDependency("HeuristicLab.Encodings.SymbolicExpressionTreeEncoding", "3.4")]
  [PluginDependency("HeuristicLab.Operators", "3.3")]
  [PluginDependency("HeuristicLab.Optimization", "3.3")]
  [PluginDependency("HeuristicLab.Parameters", "3.3")]
  [PluginDependency("HeuristicLab.Persistence", "3.3")]
  [PluginDependency("HeuristicLab.Protobuf", "3.6.1")]
  [PluginDependency("HeuristicLab.Tracing", "3.3")]
  [PluginDependency("HeuristicLab.NetMQ", "4.0.0")]
  public class HeuristicLabTruffleConnectorPlugin : PluginBase {
  }
}

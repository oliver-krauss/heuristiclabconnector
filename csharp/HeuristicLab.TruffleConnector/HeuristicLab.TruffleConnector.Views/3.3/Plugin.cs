using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.TruffleConnector {
  /// <summary>
  ///   Plugin class for HeuristicLab.TruffleConnector.Views.
  /// </summary>
  [Plugin("HeuristicLab.TruffleConnector.Views", "3.3.15.0")]
  [PluginFile("HeuristicLab.TruffleConnector.Views-3.3.dll", PluginFileType.Assembly)]
  [PluginDependency("HeuristicLab.Core", "3.3")]
  [PluginDependency("HeuristicLab.Core.Views", "3.3")]
  [PluginDependency("HeuristicLab.Common", "3.3")]
  [PluginDependency("HeuristicLab.Data", "3.3")]
  [PluginDependency("HeuristicLab.TruffleConnector", "3.3")]
  [PluginDependency("HeuristicLab.MainForm", "3.3")]
  [PluginDependency("HeuristicLab.MainForm.WindowsForms", "3.3")]
  public class HeuristicLabTruffleConnectorPlugin : PluginBase {
  }
}

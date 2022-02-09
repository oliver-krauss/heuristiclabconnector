using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.NetMQ {
  /// <summary>
  ///   Plugin class for HeuristicLab.NetMQ.
  /// </summary>
  [Plugin("HeuristicLab.NetMQ", "4.0.0")]
  [PluginFile("HeuristicLab.NetMQ-4.0.0.dll", PluginFileType.Assembly)]
  [PluginFile("NetMQ.dll", PluginFileType.Assembly)]
  [PluginFile("AsyncIO.dll", PluginFileType.Assembly)]
  [PluginFile("NetMQ-license.txt", PluginFileType.License)]
  [PluginFile("AsyncIO-license.txt", PluginFileType.License)]
  public class HeuristicLabNetMQPlugin : PluginBase { }
}

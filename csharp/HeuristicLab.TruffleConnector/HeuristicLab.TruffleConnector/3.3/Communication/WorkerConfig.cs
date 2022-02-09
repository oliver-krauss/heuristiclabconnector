namespace HeuristicLab.TruffleConnector {
  /// <summary>
  ///   Specifies a configuration a worker must support.
  /// </summary>
  public class WorkerConfig {
    /// <summary>
    ///   The ID of the programming language that must be supported (each worker only supports one at the moment).
    /// </summary>
    public long LanguageId { get; set; }
  }
}

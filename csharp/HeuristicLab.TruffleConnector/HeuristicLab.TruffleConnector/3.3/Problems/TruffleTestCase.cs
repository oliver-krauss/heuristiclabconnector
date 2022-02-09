using System;
using System.IO;
using System.Reflection;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Parameters;

namespace HeuristicLab.TruffleConnector {
  /// <summary>
  ///   Test case consisting of source code and input/output data.
  /// </summary>
  [Item(nameof(TruffleTestCase), "Test case consisting of source code and input/output data.")]
  [StorableType("A8789AF4-BF49-4DC9-9D5E-650C13A9B57C")]
  public sealed class TruffleTestCase : ParameterizedNamedItem {
    /// <summary>
    ///   Name of the function that should be optimized.
    /// </summary>
    public string FunctionName {
      get => FunctionNameParameter.Value.Value;
      set => FunctionNameParameter.Value.Value = value;
    }

    public IFixedValueParameter<StringValue> FunctionNameParameter =>
      (IFixedValueParameter<StringValue>) Parameters[nameof(FunctionName)];

    /// <summary>
    ///   Input data for the function to be tested (arguments).
    /// </summary>
    public string Input {
      get => InputParameter.Value.Value;
      set => InputParameter.Value.Value = value;
    }

    public IFixedValueParameter<TextFileValue> InputParameter =>
      (IFixedValueParameter<TextFileValue>) Parameters[nameof(Input)];

    /// <summary>
    ///   Output data for the function to be tested (results matching input).
    /// </summary>
    public string Output {
      get => OutputParameter.Value.Value;
      set => OutputParameter.Value.Value = value;
    }

    public IFixedValueParameter<TextFileValue> OutputParameter =>
      (IFixedValueParameter<TextFileValue>) Parameters[nameof(Output)];

    /// <summary>
    ///   Source code containing the function that should be optimized.
    /// </summary>
    public string SourceCode {
      get => SourceCodeParameter.Value.Value;
      set => SourceCodeParameter.Value.Value = value;
    }

    public IFixedValueParameter<TextFileValue> SourceCodeParameter =>
      (IFixedValueParameter<TextFileValue>) Parameters[nameof(SourceCode)];

    public TruffleTestCase() {
      Parameters.Add(new FixedValueParameter<TextFileValue>(nameof(SourceCode), new TextFileValue {
        FileDialogFilter = "All source code files (*.c;*.js)|*.c;*.js|All files|*.*"
      }));
      Parameters.Add(new FixedValueParameter<TextFileValue>(nameof(Input), new TextFileValue {
        FileDialogFilter = "Input files (*.input)|*.input|All files (*.*)|*.*"
      }));
      Parameters.Add(new FixedValueParameter<TextFileValue>(nameof(Output), new TextFileValue {
        FileDialogFilter = "Output files (*.output)|*.output|All files (*.*)|*.*"
      }));
      Parameters.Add(new FixedValueParameter<StringValue>(nameof(FunctionName), new StringValue()));

      RegisterEventHandlers();

      // specify default test case
      var executableDirectory =
        Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath) ?? "";
      var relativeTestCasePath = "../../../resources/minic/Fibonacci.c";
      var absoluteTestCasePath = Path.Combine(executableDirectory, relativeTestCasePath);

      SourceCode = Path.GetFullPath(new Uri(absoluteTestCasePath).LocalPath);
      FunctionName = "fibonacci";
    }

    [StorableConstructor]
    private TruffleTestCase(StorableConstructorFlag _) : base(_) { }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      RegisterEventHandlers();
    }

    private TruffleTestCase(TruffleTestCase original, Cloner cloner)
      : base(original, cloner) {
      RegisterEventHandlers();
    }

    public override IDeepCloneable Clone(Cloner cloner) => new TruffleTestCase(this, cloner);

    private void RegisterEventHandlers() {
      SourceCodeParameter.Value.PathChanged += (o, e) => {
        var sourceCodePath = SourceCodeParameter.Value.Value;
        var inputPath = Path.ChangeExtension(sourceCodePath, ".input");
        var outputPath = Path.ChangeExtension(sourceCodePath, ".output");

        if (!string.IsNullOrEmpty(sourceCodePath)) {
          FunctionName = Path.ChangeExtension(new FileInfo(sourceCodePath).Name, null);
        }

        if (File.Exists(inputPath)) {
          Input = inputPath;
        }

        if (File.Exists(outputPath)) {
          Output = outputPath;
        }
      };
    }
  }
}

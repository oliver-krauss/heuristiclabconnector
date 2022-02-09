using HeuristicLab.Core.Views;
using HeuristicLab.MainForm;

namespace HeuristicLab.TruffleConnector.Views {
  [View(nameof(TruffleTestCase) + " View")]
  [Content(typeof(TruffleTestCase), true)]
  public partial class TruffleTestCaseView : ItemView {
    public new TruffleTestCase Content {
      get => (TruffleTestCase)base.Content;
      set => base.Content = value;
    }

    public TruffleTestCaseView() {
      InitializeComponent();
    }

    protected override void OnContentChanged() {
      base.OnContentChanged();
      if (Content == null) {
        functionNameViewHost.Content = null;
        sourceCodeViewHost.Content = null;
        inputViewHost.Content = null;
        outputViewHost.Content = null;
        Caption = string.Empty;
      } else {
        functionNameViewHost.Content = Content.FunctionNameParameter.Value;
        sourceCodeViewHost.Content = Content.SourceCodeParameter.Value;
        inputViewHost.Content = Content.InputParameter.Value;
        outputViewHost.Content = Content.OutputParameter.Value;
        Caption = Content.Name;
      }
    }
  }
}

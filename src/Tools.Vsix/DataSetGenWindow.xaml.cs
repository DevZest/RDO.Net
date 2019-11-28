using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for DataSetGenWindow.xaml
    /// </summary>
    public partial class DataSetGenWindow : DbInitWindowBase
    {
        public DataSetGenWindow()
        {
            InitializeComponent();
        }

        public static void Show(CodeContext codeContext, EnvDTE.DTE dte)
        {
            new DataSetGenWindow().ShowDialog(codeContext, dte);
        }

        private void ShowDialog(CodeContext codeContext, EnvDTE.DTE dte)
        {
            _presenter = new Presenter(this, codeContext, dte);
            ShowDialog();
        }

        private Presenter _presenter;
        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        protected override bool Run()
        {
            return _presenter.Execute();
        }
    }
}

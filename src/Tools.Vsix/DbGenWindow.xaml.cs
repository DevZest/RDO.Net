using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for DbInitWindow.xaml
    /// </summary>
    public sealed partial class DbGenWindow : DbInitWindowBase
    {
        public static void Show(Project project, INamedTypeSymbol dbSessionProviderType, DataSet<DbInitInput> input, EnvDTE.DTE dte)
        {
            new DbGenWindow().ShowDialog(project, dbSessionProviderType, input, dte);
        }

        private DbGenWindow()
            : base()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private void ShowDialog(Project project, INamedTypeSymbol dbSessionProviderType, DataSet<DbInitInput> input, EnvDTE.DTE dte)
        {
            _presenter = new Presenter(this, project, dbSessionProviderType, input, dte);
            ShowDialog();
        }

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

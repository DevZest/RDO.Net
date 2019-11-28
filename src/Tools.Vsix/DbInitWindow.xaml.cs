using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for DbInitWindow.xaml
    /// </summary>
    public sealed partial class DbInitWindow : DbInitWindowBase
    {
        public static void Show(Project project, INamedTypeSymbol dbInitializerType, EnvDTE.DTE dte)
        {
            new DbInitWindow().ShowDialog(project, dbInitializerType, dte);
        }

        private DbInitWindow()
            : base()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private void ShowDialog(Project project, INamedTypeSymbol dbInitializerType, EnvDTE.DTE dte)
        {
            _presenter = new Presenter(this, project, dbInitializerType, dte);
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

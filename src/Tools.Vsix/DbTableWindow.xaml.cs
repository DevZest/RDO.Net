using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for DbTableWindow.xaml
    /// </summary>
    public sealed partial class DbTableWindow : CommonDialogWindow
    {
        public static void Show(DbMapper dbMapper, AddDbTableDelegate addDbTable)
        {
            new DbTableWindow().ShowDialog(dbMapper, addDbTable);
        }

        private DbTableWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private AddDbTableDelegate _addDbTable;
        private void ShowDialog(DbMapper dbMapper, AddDbTableDelegate addDbTable)
        {
            _presenter = new Presenter(dbMapper, this);
            _addDbTable = addDbTable;
            ShowDialog();
        }

        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        protected override void ExecApply()
        {
            _presenter.Execute(_addDbTable);
        }
    }
}

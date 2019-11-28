using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for RelationshipWindow.xaml
    /// </summary>
    public sealed partial class RelationshipWindow : CommonDialogWindow
    {
        public static void Show(DbMapper dbMapper, IPropertySymbol dbTable, AddRelationshipDelegate addRelationship)
        {
            new RelationshipWindow().ShowDialog(dbMapper, dbTable, addRelationship);
        }

        private RelationshipWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private AddRelationshipDelegate _addRelationship;
        private void ShowDialog(DbMapper dbMapper, IPropertySymbol dbTable, AddRelationshipDelegate addRelationship)
        {
            _presenter = new Presenter(dbMapper, dbTable, this);
            _addRelationship = addRelationship;
            ShowDialog();
        }

        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        protected override void ExecApply()
        {
            _presenter.Execute(_addRelationship);
        }
    }
}

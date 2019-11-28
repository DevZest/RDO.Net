using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for ForeignKeyWindow.xaml
    /// </summary>
    public partial class ForeignKeyWindow : CommonDialogWindow
    {
        public ForeignKeyWindow()
        {
            InitializeComponent();
        }

        public static void Show(ModelMapper modelMapper, AddForeignKeyDelegate addForeignKey)
        {
            new ForeignKeyWindow().ShowDialog(modelMapper, addForeignKey);
        }

        private Presenter _presenter;
        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        private AddForeignKeyDelegate _addForeignKey;
        private void ShowDialog(ModelMapper modelMapper, AddForeignKeyDelegate addForeignKey)
        {
            _presenter = new Presenter(modelMapper, this);
            _addForeignKey = addForeignKey;
            ShowDialog();
        }

        protected override void ExecApply()
        {
            _presenter.Execute(_addForeignKey);
        }
    }
}

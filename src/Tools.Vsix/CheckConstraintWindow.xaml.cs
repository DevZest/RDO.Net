using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for CheckConstraintWindow.xaml
    /// </summary>
    public sealed partial class CheckConstraintWindow : CommonDialogWindow
    {
        public static void Show(ModelMapper modelMapper, AddCheckConstraintDelegate addCheckConstraint)
        {
            new CheckConstraintWindow().ShowDialog(modelMapper, addCheckConstraint);
        }

        private CheckConstraintWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private AddCheckConstraintDelegate _addCheckConstraint;
        private void ShowDialog(ModelMapper modelMapper, AddCheckConstraintDelegate addCheckConstraint)
        {
            _presenter = new Presenter(modelMapper, this);
            _addCheckConstraint = addCheckConstraint;
            ShowDialog();
        }

        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        protected override void ExecApply()
        {
            _presenter.Execute(_addCheckConstraint);
        }
    }
}

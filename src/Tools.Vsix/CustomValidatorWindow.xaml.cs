using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for CustomValidatorWindow.xaml
    /// </summary>
    public sealed partial class CustomValidatorWindow : CommonDialogWindow
    {
        public static void Show(ModelMapper modelMapper, AddCustomValidatorDelegate addValidator)
        {
            new CustomValidatorWindow().ShowDialog(modelMapper, addValidator);
        }

        private CustomValidatorWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private AddCustomValidatorDelegate _addValidator;
        private void ShowDialog(ModelMapper modelMapper, AddCustomValidatorDelegate addValidator)
        {
            _presenter = new Presenter(modelMapper, this);
            _addValidator = addValidator;
            ShowDialog();
        }

        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        protected override void ExecApply()
        {
            _presenter.Execute(_addValidator);
        }
    }
}

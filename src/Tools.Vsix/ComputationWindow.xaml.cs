using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for ComputationWindow.xaml
    /// </summary>
    public sealed partial class ComputationWindow : CommonDialogWindow
    {
        public static void Show(ModelMapper modelMapper, AddComputationDelegate addComputation)
        {
            new ComputationWindow().ShowDialog(modelMapper, addComputation);
        }

        private ComputationWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private AddComputationDelegate _addComputation;
        private void ShowDialog(ModelMapper modelMapper, AddComputationDelegate addComputation)
        {
            _presenter = new Presenter(modelMapper, this);
            _addComputation = addComputation;
            ShowDialog();
        }

        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        protected override void ExecApply()
        {
            _presenter.Execute(_addComputation);
        }
    }
}

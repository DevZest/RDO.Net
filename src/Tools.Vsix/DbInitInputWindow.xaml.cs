using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.VisualStudio.PlatformUI;

namespace DevZest.Data.Tools
{
    /// <summary>
    /// Interaction logic for DbInitInputWindow.xaml
    /// </summary>
    public sealed partial class DbInitInputWindow : CommonDialogWindow
    {
        public static bool? Show(DataSet<DbInitInput> input)
        {
            return new DbInitInputWindow().ShowDialog(input);
        }

        private DbInitInputWindow()
            : base()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private bool? ShowDialog(DataSet<DbInitInput> input)
        {
            _presenter = new Presenter(this, input);
            return ShowDialog();
        }

        protected override BasePresenter GetPresenter()
        {
            return _presenter;
        }

        protected override void ExecApply()
        {
            DialogResult = true;
        }
    }
}

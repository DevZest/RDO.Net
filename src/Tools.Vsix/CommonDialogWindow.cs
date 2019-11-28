using DevZest.Data.Presenters;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows.Input;

namespace DevZest.Data.Tools
{
    public abstract class CommonDialogWindow : DialogWindow
    {
        public static RoutedUICommand Apply { get; private set; } = new RoutedUICommand();

        protected CommonDialogWindow()
            : this(true)
        {
        }

        protected CommonDialogWindow(bool autoClose)
        {
            CommandBindings.Add(new CommandBinding(Apply, ExecApply, CanExecApply));
            AutoClose = autoClose;
        }

        protected abstract BasePresenter GetPresenter();

        private void CanExecApply(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanApply;
        }

        protected virtual bool CanApply
        {
            get
            {
                var presenter = GetPresenter();
                if (presenter == null || !presenter.CanSubmitInput)
                    return false;
                return (presenter is DataPresenter dataPresenter) ? dataPresenter.DataSet.Count > 0 : true;
            }
        }

        protected bool AutoClose { get; }

        private void ExecApply(object sender, ExecutedRoutedEventArgs e)
        {
            bool isValid = Validate();
            if (!isValid)
                return;

            ExecApply();
            if (AutoClose)
                Close();
        }

        protected abstract void ExecApply();

        protected virtual bool Validate()
        {
            return GetPresenter().SubmitInput();
        }
    }
}

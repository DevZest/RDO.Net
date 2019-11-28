using DevZest.Data.Presenters;
using System.Windows.Input;

namespace DevZest.Data.Tools
{
    public abstract class RowArrangerDialogWindow : CommonDialogWindow
    {
        public static RoutedUICommand MoveUp { get { return ComponentCommands.MoveUp; } }
        public static RoutedUICommand MoveDown { get { return ComponentCommands.MoveDown; } }
        public static RoutedUICommand Delete { get { return ApplicationCommands.Delete; } }

        protected RowArrangerDialogWindow()
        {
            CommandBindings.Add(new CommandBinding(MoveUp, ExecMoveUp, CanExecMoveUp));
            CommandBindings.Add(new CommandBinding(MoveDown, ExecMoveDown, CanExecMoveDown));
            CommandBindings.Add(new CommandBinding(Delete, ExecDelete, CanExecDelete));
        }

        protected virtual void OnRowArranged()
        {
        }

        private void CanExecMoveUp(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanMoveUp;
        }

        private DataPresenter DataPresenter
        {
            get { return (DataPresenter)GetPresenter(); }
        }

        private bool CanMoveUp
        {
            get { return DataPresenter != null && DataPresenter.CurrentRow != null && !DataPresenter.CurrentRow.IsVirtual && DataPresenter.CurrentRow.Index > 0; }
        }

        private void ExecMoveUp(object sender, ExecutedRoutedEventArgs e)
        {
            DataPresenter.CurrentRow.DataRow.Move(-1);
            OnRowArranged();
        }

        private void CanExecMoveDown(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanMoveDown;
        }

        private bool CanMoveDown
        {
            get { return DataPresenter != null && DataPresenter.CurrentRow != null && !DataPresenter.CurrentRow.IsVirtual && DataPresenter.CurrentRow.Index < DataPresenter.DataSet.Count - 1; }
        }

        private void ExecMoveDown(object sender, ExecutedRoutedEventArgs e)
        {
            DataPresenter.CurrentRow.DataRow.Move(1);
            OnRowArranged();
        }

        private void CanExecDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanDelete;
        }

        private bool CanDelete
        {
            get { return DataPresenter != null && DataPresenter.CurrentRow != null && !DataPresenter.CurrentRow.IsVirtual; }
        }

        private void ExecDelete(object sender, ExecutedRoutedEventArgs e)
        {
            DataPresenter.CurrentRow.Delete();
            OnRowArranged();
        }
    }
}

using DevZest.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Windows.Data
{
    partial class DataPresenter
    {
        protected internal virtual IEnumerable<CommandBinding> DataViewCommandBindings
        {
            get
            {
                if (Scrollable != null)
                {
                    if (Template.Orientation == Orientation.Vertical)
                    {
                        yield return new CommandBinding(DataView.MoveFocusUpCommand, MoveToPreviousRow, CanMoveToPreviousRow);
                        yield return new CommandBinding(DataView.MoveFocusDownCommand, MoveToNextRow, CanMoveToNextRow);
                    }
                }
            }
        }

        protected internal virtual IEnumerable<InputBinding> DataViewInputBindings
        {
            get
            {
                if (Scrollable != null)
                {
                    if (Template.Orientation == Orientation.Vertical)
                    {
                        yield return new InputBinding(DataView.MoveFocusUpCommand, new KeyGesture(Key.A, ModifierKeys.Alt));
                        yield return new InputBinding(DataView.MoveFocusDownCommand, new KeyGesture(Key.Z, ModifierKeys.Alt));
                    }
                }
            }
        }

        private RowPresenter PreviousRow
        {
            get { return CurrentRow != null && CurrentRow.Index > 0 ? Rows[CurrentRow.Index - 1] : null; }
        }

        private RowPresenter NextRow
        {
            get { return CurrentRow != null && CurrentRow.Index < Rows.Count - 1 ? Rows[CurrentRow.Index + 1] : null; }
        }

        private void CanMoveToPreviousRow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Scrollable != null && PreviousRow != null;
        }

        private void MoveToPreviousRow(object sender, ExecutedRoutedEventArgs e)
        {
            Select(PreviousRow, SelectionMode.Single);
        }

        private void CanMoveToNextRow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Scrollable != null && NextRow != null;
        }

        private void MoveToNextRow(object sender, ExecutedRoutedEventArgs e)
        {
            Select(NextRow, SelectionMode.Single);
        }
    }
}

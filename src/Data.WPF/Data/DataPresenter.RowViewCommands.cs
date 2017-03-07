using DevZest.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Windows.Data
{
    partial class DataPresenter
    {
        protected internal virtual IEnumerable<CommandEntry> RowViewCommandEntries
        {
            get
            {
                if (Scrollable != null)
                {
                    if (Template.Orientation == Orientation.Vertical)
                    {
                        yield return RowView.MoveFocusUpCommand.InputBinding(MoveToPreviousRow, CanMoveToPreviousRow, new KeyGesture(Key.Up));
                        yield return RowView.MoveFocusDownCommand.InputBinding(MoveToNextRow, CanMoveToNextRow, new KeyGesture(Key.Down));
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

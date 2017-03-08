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
                    if (LayoutOrientation.HasValue)
                    {
                        yield return RowView.ScrollUpCommand.CommandBinding(ScrollUp);
                        yield return RowView.ScrollDownCommand.CommandBinding(ScrollDown);
                        yield return RowView.ScrollLeftCommand.CommandBinding(ScrollLeft);
                        yield return RowView.ScrollRightCommand.CommandBinding(ScrollRight);
                        yield return RowView.ScrollPageUpCommand.CommandBinding(ScrollPageUp);
                        yield return RowView.ScrollPageDownCommand.CommandBinding(ScrollPageDown);
                        yield return RowView.SelectUpCommand.InputBinding(SelectUp, CanSelectUp, new KeyGesture(Key.Up));
                        yield return RowView.SelectDownCommand.InputBinding(SelectDown, CanSelectDown, new KeyGesture(Key.Down));
                        yield return RowView.SelectLeftCommand.InputBinding(SelectLeft, CanSelectLeft, new KeyGesture(Key.Left));
                        yield return RowView.SelectRightCommand.InputBinding(SelectRight, CanSelectRight, new KeyGesture(Key.Right));
                        yield return RowView.SelectHomeCommand.InputBinding(SelectHome, CanSelect, new KeyGesture(Key.Home));
                        yield return RowView.SelectEndCommand.InputBinding(SelectEnd, CanSelect, new KeyGesture(Key.End));
                        //yield return RowView.SelectPageUpCommand.InputBinding(SelectPageUp, CanSelectHomeOrEnd, new KeyGesture(Key.PageUp));
                        //yield return RowView.SelectPageDownCommand.InputBinding(SelectPageDown, CanSelectHomeOrEnd, new KeyGesture(Key.PageDown));
                        yield return RowView.ExtendSelectionUpCommand.InputBinding(ExtendSelectionUp, CanSelectUp, new KeyGesture(Key.Up, ModifierKeys.Shift));
                        yield return RowView.ExtendSelectionDownCommand.InputBinding(ExtendSelectionDown, CanSelectDown, new KeyGesture(Key.Down, ModifierKeys.Shift));
                        yield return RowView.ExtendSelectionLeftCommand.InputBinding(ExtendSelectionLeft, CanSelectLeft, new KeyGesture(Key.Left, ModifierKeys.Shift));
                        yield return RowView.ExtendSelectionRightCommand.InputBinding(ExtendSelectionRight, CanSelectRight, new KeyGesture(Key.Right));
                        yield return RowView.ExtendSelectionHomeCommand.InputBinding(ExtendSelectionHome, CanSelect, new KeyGesture(Key.Home, ModifierKeys.Shift));
                        yield return RowView.ExtendSelectionEndCommand.InputBinding(ExtendSelectionEnd, CanSelect, new KeyGesture(Key.End, ModifierKeys.Shift));
                        //yield return RowView.ExtendSelectionPageUpCommand.InputBinding(ExtendSelectionPageUp, CanSelectHomeOrEnd, new KeyGesture(Key.PageUp, ModifierKeys.Shift));
                        //yield return RowView.ExtendSelectionPageDownCommand.InputBinding(ExtendSelectionPageDown, CanSelectHomeOrEnd, new KeyGesture(Key.PageDown, ModifierKeys.Shift));
                    }
                }
            }
        }

        private void ScrollUp(object sender, ExecutedRoutedEventArgs e)
        {
            Scrollable.ScrollBy(0, -View.ScrollLineHeight);
        }

        private void ScrollDown(object sender, ExecutedRoutedEventArgs e)
        {
            Scrollable.ScrollBy(0, View.ScrollLineHeight);
        }

        private void ScrollLeft(object sender, ExecutedRoutedEventArgs e)
        {
            Scrollable.ScrollBy(-View.ScrollLineWidth, 0);
        }

        private void ScrollRight(object sender, ExecutedRoutedEventArgs e)
        {
            Scrollable.ScrollBy(View.ScrollLineWidth, 0);
        }

        private void ScrollPageUp(object sender, ExecutedRoutedEventArgs e)
        {
            if (LayoutOrientation == Orientation.Vertical)
                Scrollable.ScrollBy(0, -Scrollable.ScrollableHeight);
            else
                Scrollable.ScrollBy(0, -Scrollable.ScrollableWidth);
        }

        private void ScrollPageDown(object sender, ExecutedRoutedEventArgs e)
        {
            if (LayoutOrientation == Orientation.Vertical)
                Scrollable.ScrollBy(0, Scrollable.ScrollableHeight);
            else
                Scrollable.ScrollBy(0, Scrollable.ScrollableWidth);
        }

        private bool CanSelect(Orientation orientation)
        {
            if (CurrentRow == null || !LayoutOrientation.HasValue)
                return false;

            if (LayoutOrientation != orientation && FlowCount == 1)
                return false;

            return true;
        }

        private RowPresenter GetRowBackward(Orientation orientation)
        {
            if (!CanSelect(orientation))
                return null;
            var index = CurrentRow.Index - (LayoutOrientation == orientation ? FlowCount : 1);
            return index >= 0 ? Rows[index] : null;
        }

        private RowPresenter GetRowForward(Orientation orientation)
        {
            if (!CanSelect(orientation))
                return null;
            var index = CurrentRow.Index + (LayoutOrientation == orientation ? FlowCount : 1);
            return index < Rows.Count - 1 ? Rows[index] : null;
        }

        private RowPresenter SelectUpRow
        {
            get { return GetRowBackward(Orientation.Vertical); }
        }

        private RowPresenter SelectDownRow
        {
            get { return GetRowForward(Orientation.Vertical); }
        }

        private RowPresenter SelectLeftRow
        {
            get { return GetRowBackward(Orientation.Horizontal); }
        }

        private RowPresenter SelectRightRow
        {
            get { return GetRowForward(Orientation.Horizontal); }
        }

        private void CanSelectUp(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Scrollable != null && SelectUpRow != null;
        }

        private void SelectUp(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectUpRow, SelectionMode.Single);
        }

        private void CanSelectDown(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Scrollable != null && SelectDownRow != null;
        }

        private void SelectDown(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectDownRow, SelectionMode.Single);
        }

        private void CanSelectLeft(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Scrollable != null && SelectLeftRow != null;
        }

        private void SelectLeft(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectLeftRow, SelectionMode.Single);
        }

        private void CanSelectRight(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Scrollable != null && SelectRightRow != null;
        }

        private void SelectRight(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectRightRow, SelectionMode.Single);
        }

        private void ExtendSelectionUp(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectUpRow, SelectionMode.Extended);
        }

        private void ExtendSelectionDown(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectDownRow, SelectionMode.Extended);
        }

        private void ExtendSelectionLeft(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectLeftRow, SelectionMode.Extended);
        }

        private void ExtendSelectionRight(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectRightRow, SelectionMode.Extended);
        }

        private void CanSelect(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Rows.Count > 0;
        }

        private void SelectHome(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Rows[0], SelectionMode.Single);
        }

        private void SelectEnd(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Rows[Rows.Count - 1], SelectionMode.Single);
        }

        private void ExtendSelectionHome(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Rows[0], SelectionMode.Extended);
        }

        private void ExtendSelectionEnd(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Rows[Rows.Count - 1], SelectionMode.Extended);
        }
    }
}

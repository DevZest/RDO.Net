using DevZest.Windows.Controls;
using System;
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
                        yield return RowView.SelectUpCommand.InputBinding(MoveFocusUp, CanMoveFocusUp, new KeyGesture(Key.Up));
                        yield return RowView.SelectDownCommand.InputBinding(MoveFocusDown, CanMoveFocusDown, new KeyGesture(Key.Down));
                        yield return RowView.SelectLeftCommand.InputBinding(MoveFocusLeft, CanMoveFocusLeft, new KeyGesture(Key.Left));
                        yield return RowView.SelectRightCommand.InputBinding(MoveFocusRight, CanMoveFocusRight, new KeyGesture(Key.Right));
                        yield return RowView.SelectHomeCommand.InputBindings(MoveToHome, CanMoveToHomeOrEnd, new KeyGesture(Key.PageUp, ModifierKeys.Control), new KeyGesture(Key.Home, ModifierKeys.Control));
                        yield return RowView.SelectEndCommand.InputBindings(MoveToEnd, CanMoveToHomeOrEnd, new KeyGesture(Key.PageDown, ModifierKeys.Control), new KeyGesture(Key.End, ModifierKeys.Control));
                        yield return RowView.ExtendSelectionUpCommand.InputBinding(ExtendSelectionUp, CanMoveFocusUp, new KeyGesture(Key.Up, ModifierKeys.Shift));
                        yield return RowView.ExtendSelectionDownCommand.InputBinding(ExtendSelectionDown, CanMoveFocusDown, new KeyGesture(Key.Down, ModifierKeys.Shift));
                        yield return RowView.ExtendSelectionLeftCommand.InputBinding(ExtendSelectionLeft, CanMoveFocusLeft, new KeyGesture(Key.Left, ModifierKeys.Shift));
                        yield return RowView.ExtendSelectionRightCommand.InputBinding(ExtendSelectionRight, CanMoveFocusRight, new KeyGesture(Key.Right));
                        yield return RowView.ExtendSelectionHomeCommand.InputBindings(ExtendSelectionHome, CanMoveToHomeOrEnd, new KeyGesture(Key.PageUp, ModifierKeys.Control | ModifierKeys.Shift), new KeyGesture(Key.Home, ModifierKeys.Control | ModifierKeys.Shift));
                        yield return RowView.ExtendSelectionEndCommand.InputBindings(ExtendSelectionEnd, CanMoveToHomeOrEnd, new KeyGesture(Key.PageDown, ModifierKeys.Control | ModifierKeys.Shift), new KeyGesture(Key.End, ModifierKeys.Control | ModifierKeys.Shift));
                    }
                }
            }
        }

        private bool CanMoveFocus(Orientation orientation)
        {
            if (CurrentRow == null || !LayoutOrientation.HasValue)
                return false;

            if (LayoutOrientation != orientation && FlowCount == 1)
                return false;

            return true;
        }

        private RowPresenter GetBackwardRow(Orientation orientation)
        {
            if (!CanMoveFocus(orientation))
                return null;
            var index = CurrentRow.Index - (LayoutOrientation == orientation ? FlowCount : 1);
            return index >= 0 ? Rows[index] : null;
        }

        private RowPresenter GetForwardRow(Orientation orientation)
        {
            if (!CanMoveFocus(orientation))
                return null;
            var index = CurrentRow.Index + (LayoutOrientation == orientation ? FlowCount : 1);
            return index < Rows.Count - 1 ? Rows[index] : null;
        }
        private RowPresenter FocusUpRow
        {
            get { return GetBackwardRow(Orientation.Vertical); }
        }

        private RowPresenter FocusDownRow
        {
            get { return GetForwardRow(Orientation.Vertical); }
        }

        private RowPresenter FocusLeftRow
        {
            get { return GetBackwardRow(Orientation.Horizontal); }
        }

        private RowPresenter FocusRightRow
        {
            get { return GetForwardRow(Orientation.Horizontal); }
        }

        private void CanMoveFocusUp(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Scrollable != null && FocusUpRow != null;
        }

        private void MoveFocusUp(object sender, ExecutedRoutedEventArgs e)
        {
            Select(FocusUpRow, SelectionMode.Single);
        }

        private void CanMoveFocusDown(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Scrollable != null && FocusDownRow != null;
        }

        private void MoveFocusDown(object sender, ExecutedRoutedEventArgs e)
        {
            Select(FocusDownRow, SelectionMode.Single);
        }

        private void CanMoveFocusLeft(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Scrollable != null && FocusLeftRow != null;
        }

        private void MoveFocusLeft(object sender, ExecutedRoutedEventArgs e)
        {
            Select(FocusLeftRow, SelectionMode.Single);
        }

        private void CanMoveFocusRight(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Scrollable != null && FocusRightRow != null;
        }

        private void MoveFocusRight(object sender, ExecutedRoutedEventArgs e)
        {
            Select(FocusRightRow, SelectionMode.Single);
        }

        private void ExtendSelectionUp(object sender, ExecutedRoutedEventArgs e)
        {
            Select(FocusUpRow, SelectionMode.Extended);
        }

        private void ExtendSelectionDown(object sender, ExecutedRoutedEventArgs e)
        {
            Select(FocusDownRow, SelectionMode.Extended);
        }

        private void ExtendSelectionLeft(object sender, ExecutedRoutedEventArgs e)
        {
            Select(FocusLeftRow, SelectionMode.Extended);
        }

        private void ExtendSelectionRight(object sender, ExecutedRoutedEventArgs e)
        {
            Select(FocusRightRow, SelectionMode.Extended);
        }

        private void CanMoveToHomeOrEnd(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Scrollable != null && Rows.Count > 0;
        }

        private void MoveToHome(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Rows[0], SelectionMode.Single);
        }

        private void MoveToEnd(object sender, ExecutedRoutedEventArgs e)
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

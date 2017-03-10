using DevZest.Windows.Controls;
using DevZest.Windows.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                        yield return RowView.SelectPageUpCommand.InputBinding(SelectPageUp, CanSelect, new KeyGesture(Key.PageUp));
                        yield return RowView.SelectPageDownCommand.InputBinding(SelectPageDown, CanSelect, new KeyGesture(Key.PageDown));
                        yield return RowView.SelectExtendedUpCommand.InputBinding(SelectExtendUp, CanSelectUp, new KeyGesture(Key.Up, ModifierKeys.Shift));
                        yield return RowView.SelectExtendedDownCommand.InputBinding(SelectExtendedDown, CanSelectDown, new KeyGesture(Key.Down, ModifierKeys.Shift));
                        yield return RowView.SelectiExtendedLeftCommand.InputBinding(SelectiExtendedLeft, CanSelectLeft, new KeyGesture(Key.Left, ModifierKeys.Shift));
                        yield return RowView.SelectExtendedRightCommand.InputBinding(SelectExtendedRight, CanSelectRight, new KeyGesture(Key.Right));
                        yield return RowView.SelectExtendedHomeCommand.InputBinding(SelectExtendedHome, CanSelect, new KeyGesture(Key.Home, ModifierKeys.Shift));
                        yield return RowView.SelectExtendedEndCommand.InputBinding(SelectExtendedEnd, CanSelect, new KeyGesture(Key.End, ModifierKeys.Shift));
                        yield return RowView.SelectExtendedPageUpCommand.InputBinding(SelectExtendedPageUp, CanSelect, new KeyGesture(Key.PageUp, ModifierKeys.Shift));
                        yield return RowView.SelectExtendedPageDownCommand.InputBinding(SelectExtendedPageDown, CanSelect, new KeyGesture(Key.PageDown, ModifierKeys.Shift));
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
            Scrollable.ScrollPageUp();
        }

        private void ScrollPageDown(object sender, ExecutedRoutedEventArgs e)
        {
            Scrollable.ScrollPageDown();
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
            e.CanExecute = CanSelect(e) && SelectUpRow != null;
        }

        private void SelectUp(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectUpRow, SelectionMode.Single);
        }

        private void CanSelectDown(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanSelect(e) && SelectDownRow != null;
        }

        private void SelectDown(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectDownRow, SelectionMode.Single);
        }

        private void CanSelectLeft(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanSelect(e) && SelectLeftRow != null;
        }

        private void SelectLeft(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectLeftRow, SelectionMode.Single);
        }

        private void CanSelectRight(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanSelect(e) && SelectRightRow != null;
        }

        private void SelectRight(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectRightRow, SelectionMode.Single);
        }

        private void SelectExtendUp(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectUpRow, SelectionMode.Extended);
        }

        private void SelectExtendedDown(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectDownRow, SelectionMode.Extended);
        }

        private void SelectiExtendedLeft(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectLeftRow, SelectionMode.Extended);
        }

        private void SelectExtendedRight(object sender, ExecutedRoutedEventArgs e)
        {
            Select(SelectRightRow, SelectionMode.Extended);
        }

        private void CanSelect(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanSelect(e);
        }

        private bool CanSelect(CanExecuteRoutedEventArgs e)
        {
            return e.OriginalSource is RowView && Rows.Count > 0;
        }

        private void SelectHome(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Rows[0], SelectionMode.Single);
        }

        private void SelectEnd(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Rows[Rows.Count - 1], SelectionMode.Single);
        }

        private void SelectExtendedHome(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Rows[0], SelectionMode.Extended);
        }

        private void SelectExtendedEnd(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Rows[Rows.Count - 1], SelectionMode.Extended);
        }

        private void SelectPageUp(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Scrollable.ScrollToPageUp(), SelectionMode.Single, false);
        }

        private void SelectPageDown(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Scrollable.ScrollToPageDown(), SelectionMode.Single, false);
        }

        private void SelectExtendedPageUp(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Scrollable.ScrollToPageUp(), SelectionMode.Extended, false);
        }

        private void SelectExtendedPageDown(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Scrollable.ScrollToPageDown(), SelectionMode.Extended, false);
        }
    }
}

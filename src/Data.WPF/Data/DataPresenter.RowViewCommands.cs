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
                        yield return RowView.MoveUpCommand.InputBinding(MoveUp, CanMoveUp, new KeyGesture(Key.Up));
                        yield return RowView.MoveDownCommand.InputBinding(MoveDown, CanMoveDown, new KeyGesture(Key.Down));
                        yield return RowView.MoveLeftCommand.InputBinding(MoveLeft, CanMoveLeft, new KeyGesture(Key.Left));
                        yield return RowView.MoveRightCommand.InputBinding(MoveRight, CanMoveRight, new KeyGesture(Key.Right));
                        yield return RowView.MoveToHomeCommand.InputBinding(MoveToHome, CanSelect, new KeyGesture(Key.Home));
                        yield return RowView.MoveToEndCommand.InputBinding(MoveToEnd, CanSelect, new KeyGesture(Key.End));
                        yield return RowView.MoveToPageUpCommand.InputBinding(MoveToPageUp, CanSelect, new KeyGesture(Key.PageUp));
                        yield return RowView.MoveToPageDownCommand.InputBinding(MoveToPageDown, CanSelect, new KeyGesture(Key.PageDown));
                        if (Template.SelectionMode == SelectionMode.Extended)
                        {
                            yield return RowView.SelectExtendedUpCommand.InputBinding(SelectExtendUp, CanMoveUp, new KeyGesture(Key.Up, ModifierKeys.Shift));
                            yield return RowView.SelectExtendedDownCommand.InputBinding(SelectExtendedDown, CanMoveDown, new KeyGesture(Key.Down, ModifierKeys.Shift));
                            yield return RowView.SelectiExtendedLeftCommand.InputBinding(SelectiExtendedLeft, CanMoveLeft, new KeyGesture(Key.Left, ModifierKeys.Shift));
                            yield return RowView.SelectExtendedRightCommand.InputBinding(SelectExtendedRight, CanMoveRight, new KeyGesture(Key.Right));
                            yield return RowView.SelectExtendedHomeCommand.InputBinding(SelectExtendedHome, CanSelect, new KeyGesture(Key.Home, ModifierKeys.Shift));
                            yield return RowView.SelectExtendedEndCommand.InputBinding(SelectExtendedEnd, CanSelect, new KeyGesture(Key.End, ModifierKeys.Shift));
                            yield return RowView.SelectExtendedPageUpCommand.InputBinding(SelectExtendedPageUp, CanSelect, new KeyGesture(Key.PageUp, ModifierKeys.Shift));
                            yield return RowView.SelectExtendedPageDownCommand.InputBinding(SelectExtendedPageDown, CanSelect, new KeyGesture(Key.PageDown, ModifierKeys.Shift));
                        }
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

        private RowPresenter MoveUpRow
        {
            get { return GetRowBackward(Orientation.Vertical); }
        }

        private RowPresenter MoveDownRow
        {
            get { return GetRowForward(Orientation.Vertical); }
        }

        private RowPresenter MoveLeftRow
        {
            get { return GetRowBackward(Orientation.Horizontal); }
        }

        private RowPresenter MoveRightRow
        {
            get { return GetRowForward(Orientation.Horizontal); }
        }

        private bool ShouldSelectByKey
        {
            get { return Template.SelectionMode == SelectionMode.Single || Template.SelectionMode == SelectionMode.Extended; }
        }

        private void MoveTo(RowPresenter rowPresenter, bool ensureSelectVisible = true)
        {
            if (ShouldSelectByKey)
                Select(rowPresenter, SelectionMode.Single, ensureSelectVisible);
            else
            {
                CurrentRow = rowPresenter;
                Scrollable.EnsureCurrentRowVisible();
            }
        }

        private void CanMoveUp(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanSelect(e) && MoveUpRow != null;
        }

        private void MoveUp(object sender, ExecutedRoutedEventArgs e)
        {
            MoveTo(MoveUpRow);
        }

        private void CanMoveDown(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanSelect(e) && MoveDownRow != null;
        }

        private void MoveDown(object sender, ExecutedRoutedEventArgs e)
        {
            MoveTo(MoveDownRow);
        }

        private void CanMoveLeft(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanSelect(e) && MoveLeftRow != null;
        }

        private void MoveLeft(object sender, ExecutedRoutedEventArgs e)
        {
            MoveTo(MoveLeftRow);
        }

        private void CanMoveRight(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanSelect(e) && MoveRightRow != null;
        }

        private void MoveRight(object sender, ExecutedRoutedEventArgs e)
        {
            MoveTo(MoveRightRow);
        }

        private void MoveToHome(object sender, ExecutedRoutedEventArgs e)
        {
            MoveTo(Rows[0]);
        }

        private void MoveToEnd(object sender, ExecutedRoutedEventArgs e)
        {
            MoveTo(Rows[Rows.Count - 1]);
        }

        private void MoveToPageUp(object sender, ExecutedRoutedEventArgs e)
        {
            MoveTo(Scrollable.ScrollToPageUp(), false);
        }

        private void MoveToPageDown(object sender, ExecutedRoutedEventArgs e)
        {
            MoveTo(Scrollable.ScrollToPageDown(), false);
        }

        private void SelectExtendUp(object sender, ExecutedRoutedEventArgs e)
        {
            Select(MoveUpRow, SelectionMode.Extended);
        }

        private void SelectExtendedDown(object sender, ExecutedRoutedEventArgs e)
        {
            Select(MoveDownRow, SelectionMode.Extended);
        }

        private void SelectiExtendedLeft(object sender, ExecutedRoutedEventArgs e)
        {
            Select(MoveLeftRow, SelectionMode.Extended);
        }

        private void SelectExtendedRight(object sender, ExecutedRoutedEventArgs e)
        {
            Select(MoveRightRow, SelectionMode.Extended);
        }

        private void CanSelect(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanSelect(e);
        }

        private bool CanSelect(CanExecuteRoutedEventArgs e)
        {
            return e.OriginalSource is RowView && Rows.Count > 0;
        }

        private void SelectExtendedHome(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Rows[0], SelectionMode.Extended);
        }

        private void SelectExtendedEnd(object sender, ExecutedRoutedEventArgs e)
        {
            Select(Rows[Rows.Count - 1], SelectionMode.Extended);
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

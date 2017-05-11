using DevZest.Windows.Controls;
using DevZest.Windows.Data.Primitives;
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
                yield return RowView.ExpandCommand.InputBinding(ToggleExpandState, CanExpand, new KeyGesture(Key.OemPlus));
                yield return RowView.CollapseCommand.InputBinding(ToggleExpandState, CanCollapse, new KeyGesture(Key.OemMinus));
            }
        }

        private void ToggleExpandState(object sender, ExecutedRoutedEventArgs e)
        {
            var rowView = (RowView)sender;
            rowView.RowPresenter.ToggleExpandState();
        }

        private void CanExpand(object sender, CanExecuteRoutedEventArgs e)
        {
            var rowView = (RowView)sender;
            var rowPresenter = rowView.RowPresenter;
            e.CanExecute = rowPresenter.HasChildren && !rowPresenter.IsExpanded;
        }

        private void CanCollapse(object sender, CanExecuteRoutedEventArgs e)
        {
            var rowView = (RowView)sender;
            var rowPresenter = rowView.RowPresenter;
            e.CanExecute = rowPresenter.HasChildren && rowPresenter.IsExpanded;
        }

        protected internal virtual IEnumerable<CommandEntry> RowSelectorCommandEntries
        {
            get
            {
                if (Scrollable != null)
                {
                    if (LayoutOrientation.HasValue)
                    {
                        yield return RowSelector.ScrollUpCommand.CommandBinding(ScrollUp);
                        yield return RowSelector.ScrollDownCommand.CommandBinding(ScrollDown);
                        yield return RowSelector.ScrollLeftCommand.CommandBinding(ScrollLeft);
                        yield return RowSelector.ScrollRightCommand.CommandBinding(ScrollRight);
                        yield return RowSelector.ScrollPageUpCommand.CommandBinding(ScrollPageUp);
                        yield return RowSelector.ScrollPageDownCommand.CommandBinding(ScrollPageDown);
                        yield return RowSelector.MoveUpCommand.InputBinding(MoveUp, CanMoveUp, new KeyGesture(Key.Up));
                        yield return RowSelector.MoveDownCommand.InputBinding(MoveDown, CanMoveDown, new KeyGesture(Key.Down));
                        yield return RowSelector.MoveLeftCommand.InputBinding(MoveLeft, CanMoveLeft, new KeyGesture(Key.Left));
                        yield return RowSelector.MoveRightCommand.InputBinding(MoveRight, CanMoveRight, new KeyGesture(Key.Right));
                        yield return RowSelector.MoveToHomeCommand.InputBinding(MoveToHome, CanExecuteByKeyGesture, new KeyGesture(Key.Home));
                        yield return RowSelector.MoveToEndCommand.InputBinding(MoveToEnd, CanExecuteByKeyGesture, new KeyGesture(Key.End));
                        yield return RowSelector.MoveToPageUpCommand.InputBinding(MoveToPageUp, CanExecuteByKeyGesture, new KeyGesture(Key.PageUp));
                        yield return RowSelector.MoveToPageDownCommand.InputBinding(MoveToPageDown, CanExecuteByKeyGesture, new KeyGesture(Key.PageDown));
                        if (Template.SelectionMode == SelectionMode.Extended)
                        {
                            yield return RowSelector.SelectExtendedUpCommand.InputBinding(SelectExtendUp, CanMoveUp, new KeyGesture(Key.Up, ModifierKeys.Shift));
                            yield return RowSelector.SelectExtendedDownCommand.InputBinding(SelectExtendedDown, CanMoveDown, new KeyGesture(Key.Down, ModifierKeys.Shift));
                            yield return RowSelector.SelectiExtendedLeftCommand.InputBinding(SelectiExtendedLeft, CanMoveLeft, new KeyGesture(Key.Left, ModifierKeys.Shift));
                            yield return RowSelector.SelectExtendedRightCommand.InputBinding(SelectExtendedRight, CanMoveRight, new KeyGesture(Key.Right));
                            yield return RowSelector.SelectExtendedHomeCommand.InputBinding(SelectExtendedHome, CanExecuteByKeyGesture, new KeyGesture(Key.Home, ModifierKeys.Shift));
                            yield return RowSelector.SelectExtendedEndCommand.InputBinding(SelectExtendedEnd, CanExecuteByKeyGesture, new KeyGesture(Key.End, ModifierKeys.Shift));
                            yield return RowSelector.SelectExtendedPageUpCommand.InputBinding(SelectExtendedPageUp, CanExecuteByKeyGesture, new KeyGesture(Key.PageUp, ModifierKeys.Shift));
                            yield return RowSelector.SelectExtendedPageDownCommand.InputBinding(SelectExtendedPageDown, CanExecuteByKeyGesture, new KeyGesture(Key.PageDown, ModifierKeys.Shift));
                        }
                        if (Template.SelectionMode == SelectionMode.Multiple)
                            yield return RowSelector.ToggleSelectionCommand.InputBinding(ToggleSelection, CanExecuteByKeyGesture, new KeyGesture(Key.Space));
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
            return index < Rows.Count ? Rows[index] : null;
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
            e.CanExecute = CanExecuteByKeyGesture(e) && MoveUpRow != null;
        }

        private void MoveUp(object sender, ExecutedRoutedEventArgs e)
        {
            MoveTo(MoveUpRow);
        }

        private void CanMoveDown(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanExecuteByKeyGesture(e) && MoveDownRow != null;
        }

        private void MoveDown(object sender, ExecutedRoutedEventArgs e)
        {
            MoveTo(MoveDownRow);
        }

        private void CanMoveLeft(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanExecuteByKeyGesture(e) && MoveLeftRow != null;
        }

        private void MoveLeft(object sender, ExecutedRoutedEventArgs e)
        {
            MoveTo(MoveLeftRow);
        }

        private void CanMoveRight(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanExecuteByKeyGesture(e) && MoveRightRow != null;
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

        private void CanExecuteByKeyGesture(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanExecuteByKeyGesture(e);
        }

        private bool CanExecuteByKeyGesture(CanExecuteRoutedEventArgs e)
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

        private void ToggleSelection(object sender, ExecutedRoutedEventArgs e)
        {
            var rowPresenter = ((RowView)e.OriginalSource).RowPresenter;
            rowPresenter.IsSelected = !rowPresenter.IsSelected;
        }
    }
}

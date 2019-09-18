using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    partial class RowSelector
    {
        /// <summary>
        /// Contains commands implemented by <see cref="RowSelector"/> class.
        /// </summary>
        public static class Commands
        {
            /// <summary>
            /// Command to move current row up.
            /// </summary>
            public static RoutedUICommand MoveUp { get { return ComponentCommands.MoveUp; } }

            /// <summary>
            /// Command to move current row down.
            /// </summary>
            public static RoutedUICommand MoveDown { get { return ComponentCommands.MoveDown; } }

            /// <summary>
            /// Command to move current row left.
            /// </summary>
            public static RoutedUICommand MoveLeft { get { return ComponentCommands.MoveLeft; } }

            /// <summary>
            /// Command to move current row right.
            /// </summary>
            public static RoutedUICommand MoveRight { get { return ComponentCommands.MoveRight; } }

            /// <summary>
            /// Command to move current row to one page up.
            /// </summary>
            public static RoutedUICommand MoveToPageUp { get { return ComponentCommands.MoveToPageUp; } }

            /// <summary>
            /// Command to move current row to one page down.
            /// </summary>
            public static RoutedUICommand MoveToPageDown { get { return ComponentCommands.MoveToPageDown; } }

            /// <summary>
            /// Command to move current row to the first row.
            /// </summary>
            public static RoutedUICommand MoveToHome { get { return ComponentCommands.MoveToHome; } }

            /// <summary>
            /// Command to move current row to the last row.
            /// </summary>
            public static RoutedUICommand MoveToEnd { get { return ComponentCommands.MoveToEnd; } }

            /// <summary>
            /// Command to select multiple consecutive rows up.
            /// </summary>
            public static RoutedUICommand SelectExtendedUp { get { return ComponentCommands.ExtendSelectionUp; } }

            /// <summary>
            /// Command to select multiple consecutive rows down.
            /// </summary>
            public static RoutedUICommand SelectExtendedDown { get { return ComponentCommands.ExtendSelectionDown; } }

            /// <summary>
            /// Command to select multiple consecutive rows left.
            /// </summary>
            public static RoutedUICommand SelectExtendedLeft { get { return ComponentCommands.ExtendSelectionLeft; } }

            /// <summary>
            /// Command to select multiple consecutive rows right.
            /// </summary>
            public static RoutedUICommand SelectExtendedRight { get { return ComponentCommands.ExtendSelectionRight; } }

            /// <summary>
            /// Command to select multiple consecutive rows to the first row.
            /// </summary>
            public static readonly RoutedUICommand SelectExtendedHome = new RoutedUICommand();

            /// <summary>
            /// Command to select multiple consecutive rows to the last row.
            /// </summary>
            public static readonly RoutedUICommand SelectExtendedEnd = new RoutedUICommand();

            /// <summary>
            /// Command to select multiple consecutive rows to one page up.
            /// </summary>
            public static readonly RoutedUICommand SelectExtendedPageUp = new RoutedUICommand();

            /// <summary>
            /// Command to select multiple consecutive rows to one page down.
            /// </summary>
            public static readonly RoutedUICommand SelectExtendedPageDown = new RoutedUICommand();

            /// <summary>
            /// Command to toggle the selection mode of current row.
            /// </summary>
            public static readonly RoutedUICommand ToggleSelection = new RoutedUICommand();
        }

        /// <summary>
        /// Customizable service to provide command implementations.
        /// </summary>
        public interface ICommandService : IService
        {
            /// <summary>
            /// Retrieves command implementations for specified <see cref="RowSelector"/>.
            /// </summary>
            /// <param name="rowSelector">The specified <see cref="RowSelector"/>.</param>
            /// <returns>The retrieved command implementations.</returns>
            IEnumerable<CommandEntry> GetCommandEntries(RowSelector rowSelector);
        }

        private sealed class CommandService : ICommandService
        {
            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public IEnumerable<CommandEntry> GetCommandEntries(RowSelector rowSelector)
            {
                if (DataPresenter.Scrollable != null)
                {
                    if (DataPresenter.LayoutOrientation.HasValue)
                    {
                        yield return Commands.MoveUp.Bind(ExecMoveUp, CanExecMoveUp, new KeyGesture(Key.Up));
                        yield return Commands.MoveDown.Bind(ExecMoveDown, CanExecMoveDown, new KeyGesture(Key.Down));
                        yield return Commands.MoveLeft.Bind(ExecMoveLeft, CanExecMoveLeft, new KeyGesture(Key.Left));
                        yield return Commands.MoveRight.Bind(ExecMoveRight, CanExecMoveRight, new KeyGesture(Key.Right));
                        yield return Commands.MoveToHome.Bind(ExecMoveToHome, CanExecuteByKeyGesture, new KeyGesture(Key.Home));
                        yield return Commands.MoveToEnd.Bind(ExecMoveToEnd, CanExecuteByKeyGesture, new KeyGesture(Key.End));
                        yield return Commands.MoveToPageUp.Bind(ExecMoveToPageUp, CanExecuteByKeyGesture, new KeyGesture(Key.PageUp));
                        yield return Commands.MoveToPageDown.Bind(ExecMoveToPageDown, CanExecuteByKeyGesture, new KeyGesture(Key.PageDown));
                        if (Template.SelectionMode == SelectionMode.Extended)
                        {
                            yield return Commands.SelectExtendedUp.Bind(ExecSelectExtendUp, CanExecMoveUp, new KeyGesture(Key.Up, ModifierKeys.Shift));
                            yield return Commands.SelectExtendedDown.Bind(ExecSelectExtendedDown, CanExecMoveDown, new KeyGesture(Key.Down, ModifierKeys.Shift));
                            yield return Commands.SelectExtendedLeft.Bind(ExecSelectExtendedLeft, CanExecMoveLeft, new KeyGesture(Key.Left, ModifierKeys.Shift));
                            yield return Commands.SelectExtendedRight.Bind(ExecSelectExtendedRight, CanExecMoveRight, new KeyGesture(Key.Right, ModifierKeys.Shift));
                            yield return Commands.SelectExtendedHome.Bind(ExecSelectExtendedHome, CanExecuteByKeyGesture, new KeyGesture(Key.Home, ModifierKeys.Shift));
                            yield return Commands.SelectExtendedEnd.Bind(ExecSelectExtendedEnd, CanExecuteByKeyGesture, new KeyGesture(Key.End, ModifierKeys.Shift));
                            yield return Commands.SelectExtendedPageUp.Bind(ExecSelectExtendedPageUp, CanExecuteByKeyGesture, new KeyGesture(Key.PageUp, ModifierKeys.Shift));
                            yield return Commands.SelectExtendedPageDown.Bind(ExecSelectExtendedPageDown, CanExecuteByKeyGesture, new KeyGesture(Key.PageDown, ModifierKeys.Shift));
                        }
                        if (Template.SelectionMode == SelectionMode.Multiple)
                            yield return Commands.ToggleSelection.Bind(ExecToggleSelection, CanExecuteByKeyGesture, new KeyGesture(Key.Space));
                    }
                }
            }

            private Template Template
            {
                get { return DataPresenter.Template; }
            }

            private IScrollable Scrollable
            {
                get { return DataPresenter.Scrollable; }
            }

            private RowPresenter CurrentRow
            {
                get { return DataPresenter.CurrentRow; }
                set { DataPresenter.CurrentRow = value; }
            }

            private Orientation? LayoutOrientation
            {
                get { return DataPresenter.LayoutOrientation; }
            }

            private int FlowRepeatCount
            {
                get { return DataPresenter.FlowRepeatCount; }
            }

            private IReadOnlyList<RowPresenter> Rows
            {
                get { return DataPresenter.Rows; }
            }

            private void Select(RowPresenter rowPresenter, SelectionMode selectionMode, bool ensureVisible = true)
            {
                DataPresenter.Select(rowPresenter, selectionMode, ensureVisible);
            }

            private bool CanSelect(Orientation orientation)
            {
                if (CurrentRow == null || !LayoutOrientation.HasValue)
                    return false;

                if (LayoutOrientation != orientation && FlowRepeatCount == 1)
                    return false;

                return true;
            }

            private RowPresenter GetRowBackward(Orientation orientation)
            {
                if (!CanSelect(orientation))
                    return null;
                var index = CurrentRow.Index - (LayoutOrientation == orientation ? FlowRepeatCount : 1);
                return index >= 0 ? Rows[index] : null;
            }

            private RowPresenter GetRowForward(Orientation orientation)
            {
                if (!CanSelect(orientation))
                    return null;
                var index = CurrentRow.Index + (LayoutOrientation == orientation ? FlowRepeatCount : 1);
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

            private void CanExecMoveUp(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExecuteByKeyGesture(e) && MoveUpRow != null;
            }

            private void ExecMoveUp(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(MoveUpRow);
            }

            private void CanExecMoveDown(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExecuteByKeyGesture(e) && MoveDownRow != null;
            }

            private void ExecMoveDown(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(MoveDownRow);
            }

            private void CanExecMoveLeft(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExecuteByKeyGesture(e) && MoveLeftRow != null;
            }

            private void ExecMoveLeft(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(MoveLeftRow);
            }

            private void CanExecMoveRight(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExecuteByKeyGesture(e) && MoveRightRow != null;
            }

            private void ExecMoveRight(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(MoveRightRow);
            }

            private void ExecMoveToHome(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(Rows[0]);
            }

            private void ExecMoveToEnd(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(Rows[Rows.Count - 1]);
            }

            private void ExecMoveToPageUp(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(Scrollable.ScrollToPageUp(), false);
            }

            private void ExecMoveToPageDown(object sender, ExecutedRoutedEventArgs e)
            {
                MoveTo(Scrollable.ScrollToPageDown(), false);
            }

            private void ExecSelectExtendUp(object sender, ExecutedRoutedEventArgs e)
            {
                Select(MoveUpRow, SelectionMode.Extended);
            }

            private void ExecSelectExtendedDown(object sender, ExecutedRoutedEventArgs e)
            {
                Select(MoveDownRow, SelectionMode.Extended);
            }

            private void ExecSelectExtendedLeft(object sender, ExecutedRoutedEventArgs e)
            {
                Select(MoveLeftRow, SelectionMode.Extended);
            }

            private void ExecSelectExtendedRight(object sender, ExecutedRoutedEventArgs e)
            {
                Select(MoveRightRow, SelectionMode.Extended);
            }

            private void CanExecuteByKeyGesture(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExecuteByKeyGesture(e);
            }

            private bool CanExecuteByKeyGesture(CanExecuteRoutedEventArgs e)
            {
                return Rows.Count > 0 && !CurrentRow.IsEditing;
            }

            private void ExecSelectExtendedHome(object sender, ExecutedRoutedEventArgs e)
            {
                Select(Rows[0], SelectionMode.Extended);
            }

            private void ExecSelectExtendedEnd(object sender, ExecutedRoutedEventArgs e)
            {
                Select(Rows[Rows.Count - 1], SelectionMode.Extended);
            }

            private void ExecSelectExtendedPageUp(object sender, ExecutedRoutedEventArgs e)
            {
                Select(Scrollable.ScrollToPageUp(), SelectionMode.Extended, false);
            }

            private void ExecSelectExtendedPageDown(object sender, ExecutedRoutedEventArgs e)
            {
                Select(Scrollable.ScrollToPageDown(), SelectionMode.Extended, false);
            }

            private void ExecToggleSelection(object sender, ExecutedRoutedEventArgs e)
            {
                var rowPresenter = ((RowSelector)e.OriginalSource).RowPresenter;
                rowPresenter.IsSelected = !rowPresenter.IsSelected;
            }
        }
    }
}

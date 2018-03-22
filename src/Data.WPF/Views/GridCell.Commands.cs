using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    partial class GridCell
    {
        public abstract class Commands
        {
            public static RoutedCommand ToggleMode { get; private set; } = new RoutedCommand(nameof(ToggleMode), typeof(GridCell));
            public static RoutedCommand ExitEditMode { get; private set; } = new RoutedCommand(nameof(ExitEditMode), typeof(GridCell));
            public static RoutedCommand Activate { get; private set; } = new RoutedCommand(nameof(Activate), typeof(GridCell));
            public static RoutedCommand SelectTo { get; private set; } = new RoutedCommand(nameof(SelectTo), typeof(GridCell));
            public static RoutedCommand SelectAll { get { return ApplicationCommands.SelectAll; } }
            public static RoutedCommand Copy { get { return ApplicationCommands.Copy; } }

            public static RoutedCommand SelectLeft { get { return ComponentCommands.MoveLeft; } }
            public static RoutedCommand SelectRight { get { return ComponentCommands.MoveRight; } }
            public static RoutedCommand SelectUp { get { return ComponentCommands.MoveUp; } }
            public static RoutedCommand SelectDown { get { return ComponentCommands.MoveDown; } }
            public static RoutedCommand SelectPageUp { get { return ComponentCommands.MoveToPageUp; } }
            public static RoutedCommand SelectPageDown { get { return ComponentCommands.MoveToPageDown; } }
            public static RoutedCommand SelectRowHome { get; private set; } = new RoutedCommand(nameof(SelectRowHome), typeof(GridCell));
            public static RoutedCommand SelectRowEnd { get; private set; } = new RoutedCommand(nameof(SelectRowEnd), typeof(GridCell));
            public static RoutedCommand SelectHome { get { return ComponentCommands.MoveToHome; } }
            public static RoutedCommand SelectEnd { get { return ComponentCommands.MoveToEnd; } }
            public static RoutedCommand SelectToLeft { get { return ComponentCommands.ExtendSelectionLeft; } }
            public static RoutedCommand SelectToRight { get { return ComponentCommands.ExtendSelectionRight; } }
            public static RoutedCommand SelectToUp { get { return ComponentCommands.ExtendSelectionUp; } }
            public static RoutedCommand SelectToDown { get { return ComponentCommands.ExtendSelectionDown; } }
            public static RoutedCommand SelectToPageUp { get; private set; } = new RoutedCommand(nameof(SelectToPageUp), typeof(GridCell));
            public static RoutedCommand SelectToPageDown { get; private set; } = new RoutedCommand(nameof(SelectToPageDown), typeof(GridCell));
            public static RoutedCommand SelectToRowHome { get; private set; } = new RoutedCommand(nameof(SelectToRowHome), typeof(GridCell));
            public static RoutedCommand SelectToRowEnd { get; private set; } = new RoutedCommand(nameof(SelectToRowEnd), typeof(GridCell));
            public static RoutedCommand SelectToHome { get; private set; } = new RoutedCommand(nameof(SelectToHome), typeof(GridCell));
            public static RoutedCommand SelectToEnd { get; private set; } = new RoutedCommand(nameof(SelectToEnd), typeof(GridCell));
        }

        public interface ICommandService : IService
        {
            IEnumerable<CommandEntry> GetCommandEntries(GridCell gridCell);
        }

        private sealed class CommandService : ICommandService
        {
            public DataPresenter DataPresenter { get; private set; }

            private Presenter Presenter
            {
                get { return DataPresenter.GetService<Presenter>(); }
            }

            private GridCellMode Mode
            {
                get { return Presenter.Mode; }
            }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public IEnumerable<CommandEntry> GetCommandEntries(GridCell gridCell)
            {
                yield return Commands.ToggleMode.Bind(ExecToggleMode, CanToggleMode, new KeyGesture(Key.F8));
                yield return Commands.Activate.Bind(ExecActivateGridCell, CanActivateGridCell, new MouseGesture(MouseAction.LeftClick));
                yield return Commands.ExitEditMode.Bind(ExecToggleMode, CanExitEditMode, new KeyGesture(Key.Escape));
                yield return Commands.SelectTo.Bind(ExecSelectTo, CanSelectTo, new MouseGesture(MouseAction.LeftClick, ModifierKeys.Shift));
                yield return Commands.SelectAll.Bind(ExecSelectAll, CanSelectTo);
                yield return Commands.Copy.Bind(ExecCopy, CanCopy);
            }

            private void CanToggleMode(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanToggleMode((GridCell)sender);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private bool CanToggleMode(GridCell gridCell)
            {
                return Presenter.CanToggleMode(gridCell);
            }

            private void ExecToggleMode(object sender, ExecutedRoutedEventArgs e)
            {
                e.Handled = ToggleMode((GridCell)sender);
            }

            private bool ToggleMode(GridCell gridCell)
            {
                Presenter.ToggleMode(gridCell);
                return true;
            }

            private void CanEnterEditMode(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanEnterEditMode((GridCell)sender);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private bool CanEnterEditMode(GridCell gridCell)
            {
                return gridCell.IsCurrent && Mode == GridCellMode.Select && gridCell.IsEditable;
            }

            private void CanExitEditMode(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanExitEditMode((GridCell)sender);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private bool CanExitEditMode(GridCell gridCell)
            {
                return gridCell.IsCurrent && Mode == GridCellMode.Edit;
            }

            private void CanActivateGridCell(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanActivate((GridCell)sender);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private bool CanActivate(GridCell gridCell)
            {
                return Presenter.PredictActivate(gridCell).HasValue;
            }

            private void ExecActivateGridCell(object sender, ExecutedRoutedEventArgs e)
            {
                e.Handled = Activate((GridCell)sender);
            }

            private bool Activate(GridCell gridCell)
            {
                Presenter.Activate(gridCell);
                return true;
            }

            private void CanSelectTo(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = CanSelectTo((GridCell)sender);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private bool CanSelectTo(GridCell gridCell)
            {
                return Mode == GridCellMode.Select;
            }

            private void ExecSelectTo(object sender, ExecutedRoutedEventArgs e)
            {
                e.Handled = SelectTo((GridCell)sender);
            }

            private bool SelectTo(GridCell gridCell)
            {
                Presenter.Select(gridCell, true);
                return true;
            }

            private void ExecSelectAll(object sender, ExecutedRoutedEventArgs e)
            {
                e.Handled = SelectAll();
            }

            private bool SelectAll()
            {
                Presenter.SelectAll();
                return true;
            }

            private int SelectedRowsCount
            {
                get
                {
                    var presenter = DataPresenter.GetService<Presenter>();
                    var startRowIndex = presenter.StartSelectedRowIndex;
                    if (startRowIndex < 0)
                        return 0;
                    var endRowIndex = presenter.EndSelectedRowIndex;

                    var result = endRowIndex - startRowIndex + 1;
                    var virtualRow = DataPresenter.VirtualRow;
                    if (virtualRow != null && virtualRow.Index >= startRowIndex && virtualRow.Index <= endRowIndex)
                        result--;
                    return result;
                }
            }

            private RowPresenter[] GetSelectedRows()
            {
                var presenter = DataPresenter.GetService<Presenter>();
                var startRowIndex = presenter.StartSelectedRowIndex;
                var endRowIndex = presenter.EndSelectedRowIndex;

                Debug.Assert(SelectedRowsCount > 0);
                var result = new RowPresenter[SelectedRowsCount];
                var index = 0;
                for (int i = startRowIndex; i <= endRowIndex; i++)
                {
                    var row = DataPresenter.Rows[i];
                    if (!row.IsVirtual)
                        result[index++] = row;
                }
                return result;
            }

            private int SelectedColumnsCount
            {
                get
                {
                    var presenter = DataPresenter.GetService<Presenter>();
                    var startBindingIndex = presenter.StartSelectedBindingIndex;
                    if (startBindingIndex < 0)
                        return 0;
                    var endBindingIndex = presenter.EndSelectedBindingIndex;
                    var gridCellBindings = presenter.GridCellBindings;

                    int result = 0;
                    for (int i = startBindingIndex; i <= endBindingIndex; i++)
                        result += gridCellBindings[i].SerializableColumns.Count;

                    return result;
                }
            }

            private ColumnSerializer[] GetSelectedColumnSerializers()
            {
                var presenter = DataPresenter.GetService<Presenter>();
                var startBindingIndex = presenter.StartSelectedBindingIndex;
                Debug.Assert(startBindingIndex >= 0);
                var endBindingIndex = presenter.EndSelectedBindingIndex;
                var gridCellBindings = presenter.GridCellBindings;

                var result = new ColumnSerializer[SelectedColumnsCount];
                int index = 0;
                for (int i = startBindingIndex; i <= endBindingIndex; i++)
                {
                    var rowBinding = gridCellBindings[i];
                    var serializableColumns = rowBinding.SerializableColumns;
                    for (int j = 0; j < serializableColumns.Count; j++)
                        result[index++] = DataPresenter.GetSerializer(serializableColumns[j]);
                }

                return result;
            }

            private void CanCopy(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = DataPresenter.GetService<Presenter>().Mode == GridCellMode.Select && SelectedRowsCount > 0 && SelectedColumnsCount > 0;
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private void ExecCopy(object sender, ExecutedRoutedEventArgs e)
            {
                var selectedRows = GetSelectedRows();
                var columnSerializers = GetSelectedColumnSerializers();
                new SerializableSelection(selectedRows, columnSerializers).CopyToClipboard(true, true);
                e.Handled = true;
            }
        }

        protected virtual ICommandService GetCommandService(DataPresenter dataPresenter)
        {
            return dataPresenter.GetService<ICommandService>();
        }
    }
}

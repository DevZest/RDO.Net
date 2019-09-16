using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DevZest.Data.Views
{
    partial class DataView
    {
        /// <summary>
        /// Contains commands supported by <see cref="DataView"/>.
        /// </summary>
        public abstract class Commands
        {
            /// <summary>
            /// Command to retry data loading.
            /// </summary>
            public static readonly RoutedUICommand RetryDataLoad = new RoutedUICommand(UserMessages.DataViewCommands_RetryDataLoadCommandText, nameof(RetryDataLoad), typeof(Commands));

            /// <summary>
            /// Command to cancel data loading.
            /// </summary>
            public static readonly RoutedUICommand CancelDataLoad = new RoutedUICommand(UserMessages.DataViewCommands_CancelDataLoadCommandText, nameof(CancelDataLoad), typeof(Commands));

            /// <summary>
            /// Command to toggle scalar editing mode.
            /// </summary>
            public static readonly RoutedUICommand ToggleEditScalars = new RoutedUICommand();

            /// <summary>
            /// Command to begin scalar editing mode.
            /// </summary>
            public static readonly RoutedUICommand BeginEditScalars = new RoutedUICommand();

            /// <summary>
            /// Command to cancel scalar editing mode.
            /// </summary>
            public static readonly RoutedUICommand CancelEditScalars = new RoutedUICommand();

            /// <summary>
            /// Command to end scalar editing mode.
            /// </summary>
            public static readonly RoutedUICommand EndEditScalars = new RoutedUICommand();

            /// <summary>
            /// Command to delete data.
            /// </summary>
            public static RoutedUICommand Delete { get { return ApplicationCommands.Delete; } }

            /// <summary>
            /// Command to copy data from clipboard.
            /// </summary>
            public static RoutedUICommand Copy { get { return ApplicationCommands.Copy; } }

            /// <summary>
            /// Command to paste append data from clipboard.
            /// </summary>
            public static RoutedUICommand PasteAppend { get { return ApplicationCommands.Paste; } }
        }

        /// <summary>
        /// Provides implementation of command excution.
        /// </summary>
        public interface ICommandService : IService
        {
            /// <summary>
            /// Gets the implementation of the commands for specified <see cref="DataView"/>.
            /// </summary>
            /// <param name="dataView">The specified <see cref="DataView"/>.</param>
            /// <returns>The implementation of the commands.</returns>
            IEnumerable<CommandEntry> GetCommandEntries(DataView dataView);
        }

        private sealed class CommandService : ICommandService
        {
            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public IEnumerable<CommandEntry> GetCommandEntries(DataView dataView)
            {
                yield return Commands.CancelDataLoad.Bind(CancelLoadData, CanCancelLoadData);
                yield return Commands.RetryDataLoad.Bind(ReloadData, CanReloadData);
                yield return Commands.ToggleEditScalars.Bind(ToggleEditScalars);
                yield return Commands.BeginEditScalars.Bind(BeginEditScalars, CanBeginEditScalars);
                yield return Commands.CancelEditScalars.Bind(CancelEditScalars, CanCancelEditScalars);
                yield return Commands.EndEditScalars.Bind(EndEditScalars, CanCancelEditScalars);
                yield return Commands.Delete.Bind(ExecDeleteSelected, CanExecDeleteSelected, new KeyGesture(Key.Delete));
                yield return Commands.Copy.Bind(ExecCopy, CanExecCopy);
                yield return Commands.PasteAppend.Bind(ExecPasteAppend, CanExecPasteAppend);
            }

            private void ReloadData(object sender, ExecutedRoutedEventArgs e)
            {
                DataPresenter.Reload();
            }

            private void CanReloadData(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = DataPresenter.CanReload;
            }

            private void CancelLoadData(object sender, ExecutedRoutedEventArgs e)
            {
                DataPresenter.CancelLoading();
            }

            private void CanCancelLoadData(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = DataPresenter.CanCancelLoading;
            }

            private void ToggleEditScalars(object sender, ExecutedRoutedEventArgs e)
            {
                var scalarContainer = ((DataView)sender).DataPresenter.ScalarContainer;
                if (scalarContainer.IsEditing)
                    scalarContainer.EndEdit();
                else
                    scalarContainer.BeginEdit();
            }

            private void CanBeginEditScalars(object sender, CanExecuteRoutedEventArgs e)
            {
                var scalarContainer = ((DataView)sender).DataPresenter.ScalarContainer;
                e.CanExecute = !scalarContainer.IsEditing;
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private void BeginEditScalars(object sender, ExecutedRoutedEventArgs e)
            {
                var scalarContainer = ((DataView)sender).DataPresenter.ScalarContainer;
                scalarContainer.BeginEdit();
            }

            private void CanCancelEditScalars(object sender, CanExecuteRoutedEventArgs e)
            {
                var scalarContainer = ((DataView)sender).DataPresenter.ScalarContainer;
                e.CanExecute = scalarContainer.IsEditing;
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private void CancelEditScalars(object sender, ExecutedRoutedEventArgs e)
            {
                var scalarContainer = ((DataView)sender).DataPresenter.ScalarContainer;
                scalarContainer.CancelEdit();
            }

            private void EndEditScalars(object sender, ExecutedRoutedEventArgs e)
            {
                var scalarContainer = ((DataView)sender).DataPresenter.ScalarContainer;
                scalarContainer.EndEdit();
            }

            private void CanExecDeleteSelected(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = DataPresenter.Template != null && DataPresenter.Template.AllowsDelete && DataPresenter.SelectedRows.Count > 0;
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private void ExecDeleteSelected(object sender, ExecutedRoutedEventArgs e)
            {
                var confirmed = DataPresenter.ConfirmDelete();
                if (confirmed)
                {
                    foreach (var row in DataPresenter.SelectedRows.ToArray())
                        row.Delete();
                }
                e.Handled = true;
            }

            private void CanExecCopy(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = SelectedRowsCount > 0 && ColumnsCount > 0;
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private int SelectedRowsCount
            {
                get
                {
                    var result = DataPresenter.SelectedRows.Count;
                    var virtualRow = DataPresenter.VirtualRow;
                    if (virtualRow != null && virtualRow.IsSelected)
                        result--;
                    return result;
                }
            }

            private RowPresenter[] GetSelectedRows()
            {
                return DataPresenter.SelectedRows.Where(x => !x.IsVirtual).OrderBy(x => x.Index).ToArray();
            }

            private int ColumnsCount
            {
                get
                {
                    var model = DataPresenter.DataSet.Model;
                    return model.GetColumns().Count + model.GetLocalColumns().Count;
                }
            }

            private ColumnSerializer[] GetColumnSerializers()
            {
                var result = new ColumnSerializer[ColumnsCount];

                var model = DataPresenter.DataSet.Model;
                var columns = model.GetColumns();
                var localColumns = model.GetLocalColumns();
                for (int i = 0; i < columns.Count; i++)
                    result[i] = DataPresenter.GetSerializer(columns[i]);
                for (int i = 0; i < localColumns.Count; i++)
                    result[i + columns.Count] = DataPresenter.GetSerializer(localColumns[i]);
                return result;
            }

            private void ExecCopy(object sender, ExecutedRoutedEventArgs e)
            {
                var selectedRows = GetSelectedRows();
                var columnSerializers = GetColumnSerializers();
                new SerializableSelection(selectedRows, columnSerializers).CopyToClipboard(true, true);
                e.Handled = true;
            }

            private RowPresenter CurrentRow
            {
                get { return DataPresenter.CurrentRow; }
            }

            private void CanExecPasteAppend(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = TabularText.CanPasteFromClipboard && !DataPresenter.IsEditing && !DataPresenter.IsRecursive 
                    && DataPresenter.LayoutOrientation.HasValue && CurrentRow != null && CurrentRow.IsVirtual && PastableColumns.Any();
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private IReadOnlyList<RowBinding> RowBindings
            {
                get { return DataPresenter.Template.RowBindings; }
            }

            private void ExecPasteAppend(object sender, ExecutedRoutedEventArgs e)
            {
                var window = new PasteAppendWindow();
                var columns = PastableColumns.ToArray();
                var dataToPaste = window.Show(DataPresenter, columns);
                if (dataToPaste != null)
                    PasteAppend(columns, dataToPaste);
                DataPresenter.Scrollable.EnsureCurrentRowVisible();
                e.Handled = true;
            }

            private void PasteAppend(IReadOnlyList<Column> columns, IReadOnlyList<ColumnValueBag> dataToPaste)
            {
                for (int i = 0; i < dataToPaste.Count; i++)
                {
                    if (!PasteAppend(columns, dataToPaste[i]))
                    {
                        MessageBox.Show(UserMessages.DataView_PasteAppendWithError(i));
                        return;
                    }
                }
                MessageBox.Show(UserMessages.DataView_PasteAppendCompleted(dataToPaste.Count));
            }

            private bool PasteAppend(IReadOnlyList<Column> columns, ColumnValueBag data)
            {
                var presenter = DataPresenter;
                var row = presenter.VirtualRow;
                row.BeginEdit();
                for (int i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    if (data.ContainsKey(column))
                        row[column] = data[column];
                }
                row.EndEdit();
                return !presenter.IsEditing;
            }

            private IEnumerable<Column> SerializableColumns
            {
                get
                {
                    for (int i = 0; i < RowBindings.Count; i++)
                    {
                        var columns = RowBindings[i].SerializableColumns;
                        foreach (var column in columns)
                            yield return column;
                    }
                }
            }

            private IEnumerable<Column> PastableColumns
            {
                get
                {
                    for (int i = 0; i < RowBindings.Count; i++)
                    {
                        var columns = RowBindings[i].GetInputTargetColumns();
                        foreach (var column in columns)
                        {
                            if (!column.IsExpression)
                                yield return column;
                        }
                    }
                }
            }
        }

        private ICommandService GetCommandService(DataPresenter dataPresenter)
        {
            return dataPresenter.GetService<ICommandService>();
        }

        /// <summary>
        /// Provides implementation of paste append operation.
        /// </summary>
        public interface IPasteAppendService : IService
        {
            /// <summary>
            /// Verifies the data for paste append operation.
            /// </summary>
            /// <param name="data">The data.</param>
            /// <returns><see langword="true"/> if data is valid for paste append operation, otherwise <see langword="false"/>.</returns>
            bool Verify(IReadOnlyList<ColumnValueBag> data);
        }
    }
}

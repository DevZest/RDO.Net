using DevZest.Data.Views;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DevZest.Data.Presenters.Services
{
    public class ColumnHeaderCommands : IService
    {
        public static readonly RoutedUICommand ToggleSortDirection = new RoutedUICommand(nameof(ToggleSortDirection), nameof(ToggleSortDirection), typeof(ColumnHeaderCommands));
        public static readonly RoutedUICommand Sort = new RoutedUICommand(UIText.ColumnHeaderCommands_SortCommandText, nameof(Sort), typeof(ColumnHeaderCommands));

        public DataPresenter DataPresenter { get; set; }

        protected internal virtual IEnumerable<CommandEntry> GetCommandEntries(DataView dataView)
        {
            throw new NotImplementedException();
        }

        protected internal virtual IEnumerable<CommandEntry> GetCommandEntries(ColumnHeader columnHeader)
        {
            yield return ToggleSortDirection.InputBinding(ExecToggleSortDirection, CanExecToggleSortDirection, new MouseGesture(MouseAction.LeftClick));
        }

        private void CanExecToggleSortDirection(object sender, CanExecuteRoutedEventArgs e)
        {
            var columnHeader = (ColumnHeader)sender;
            e.CanExecute = columnHeader.Column != null && columnHeader.CanSort;
        }

        private void ExecToggleSortDirection(object sender, ExecutedRoutedEventArgs e)
        {
            var columnHeader = (ColumnHeader)sender;
            var direction = Toggle(columnHeader.SortDirection);
            var sortService = DataPresenter.GetService<ISortService>(() => new SortService());
            if (direction == SortDirection.Unspecified)
                sortService.OrderBy = null;
            else
                sortService.OrderBy = new IColumnComparer[] { DataRow.OrderBy(columnHeader.Column, direction) };
        }

        private static SortDirection Toggle(SortDirection direction)
        {
            if (direction == SortDirection.Unspecified)
                return SortDirection.Ascending;
            else if (direction == SortDirection.Ascending)
                return SortDirection.Descending;
            else
                return SortDirection.Unspecified;
        }
    }
}

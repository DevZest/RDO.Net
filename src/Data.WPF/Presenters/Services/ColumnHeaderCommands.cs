using DevZest.Data.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace DevZest.Data.Presenters.Services
{
    public class ColumnHeaderCommands : IService
    {
        public static readonly RoutedUICommand ToggleSortDirection = new RoutedUICommand(nameof(ToggleSortDirection), nameof(ToggleSortDirection), typeof(ColumnHeaderCommands));
        public static readonly RoutedUICommand Sort = new RoutedUICommand(UIText.ColumnHeaderCommands_SortCommandText, nameof(Sort), typeof(ColumnHeaderCommands));

        private DataPresenter _dataPresenter;
        public DataPresenter DataPresenter
        {
            get { return _dataPresenter; }
            set
            {
                if (_dataPresenter == value)
                    return;

                var oldValue = _dataPresenter;
                _dataPresenter = value;
                OnDataPresenterChanged(oldValue, value);
            }
        }

        private void OnDataPresenterChanged(DataPresenter oldValue, DataPresenter newValue)
        {
            if (oldValue != null)
                oldValue.ViewChanged -= OnViewChanged;
            if (newValue != null)
                newValue.ViewChanged += OnViewChanged;
        }

        private void OnViewChanged(object sender, EventArgs e)
        {
            _dataViewCommandEntriesSetup = false;
        }

        private bool _dataViewCommandEntriesSetup = false;
        internal void EnsureCommandEntriesSetup(DataView dataView)
        {
            if (_dataViewCommandEntriesSetup)
                return;

            dataView.SetupCommandEntries(GetCommandEntries(dataView));
            _dataViewCommandEntriesSetup = true;
        }

        protected virtual IEnumerable<CommandEntry> GetCommandEntries(DataView dataView)
        {
            yield return Sort.CommandBinding(ExecSort, CanExecSort);
        }

        private void CanExecSort(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanShowSortWindow;
        }

        private bool CanShowSortWindow
        {
            get
            {
                var bindings = DataPresenter.Template.ScalarBindings;
                for (int i = 0; i < bindings.Count; i++)
                {
                    var columnHeader = bindings[i][0] as ColumnHeader;
                    if (columnHeader != null && columnHeader.Column != null && columnHeader.CanSort)
                        return true;
                }
                return false;
            }
        }

        private void ExecSort(object sender, ExecutedRoutedEventArgs e)
        {
            var dataView = (DataView)sender;
            var sortWindow = new SortWindow();
            var positionFromScreen = dataView.PointToScreen(new Point(0, 0));
            PresentationSource source = PresentationSource.FromVisual(dataView);
            Point targetPoints = source.CompositionTarget.TransformFromDevice.Transform(positionFromScreen);

            sortWindow.Top = targetPoints.Y + Math.Max(0, (dataView.ActualHeight - sortWindow.Height) / 3);
            sortWindow.Left = targetPoints.X + Math.Max(0, (dataView.ActualWidth - sortWindow.Width) / 3);
            sortWindow.ShowDialog();
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

using DevZest.Data;
using System.Windows;
using System.Windows.Input;

namespace DevZest.Samples.AdventureWorksLT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static class Commands
        {
            public static RoutedUICommand New { get { return ApplicationCommands.New; } }
            public static RoutedUICommand Open { get { return ApplicationCommands.Open; } }
            public static RoutedUICommand Delete { get { return ApplicationCommands.Delete; } }
            public static RoutedUICommand Refresh { get { return NavigationCommands.Refresh; } }
            public static RoutedUICommand Search { get { return SearchBox.Commands.Search; } }
            public static RoutedUICommand ClearSearch { get { return SearchBox.Commands.ClearSearch; } }
            public static RoutedUICommand Close { get { return ApplicationCommands.Close; } }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeCommandBindings();
            _presenter = new Presenter();
            _presenter.ShowAsync(_dataView);
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(Commands.New, New));
            CommandBindings.Add(new CommandBinding(Commands.Open, Open, CanOpen));
            CommandBindings.Add(new CommandBinding(Commands.Delete, Delete, CanDelete));
            CommandBindings.Add(new CommandBinding(Commands.Refresh, Refresh, CanRefresh));
            CommandBindings.Add(new CommandBinding(Commands.Search, Search, CanRefresh));
            CommandBindings.Add(new CommandBinding(Commands.ClearSearch, ClearSearch, CanRefresh));
            CommandBindings.Add(new CommandBinding(Commands.Close, Close));
        }

        private SalesOrderHeader _
        {
            get { return _presenter._; }
        }

        private void New(object sender, ExecutedRoutedEventArgs e)
        {
            var salesOrderInfo = DataSet<SalesOrderInfo>.Create();
            salesOrderInfo.AddRow();
            new SalesOrderWindow().Show(salesOrderInfo, this, Refresh);
        }

        private void Open(object sender, ExecutedRoutedEventArgs e)
        {
            var salesOrderID = _presenter.CurrentRow.GetValue(_.SalesOrderID).Value;
            if (App.Execute((db, ct) => db.GetSalesOrderInfoAsync(salesOrderID, ct), this, out var dataSet))
            {
                if (dataSet.Count == 1)
                    new SalesOrderWindow().Show(dataSet, this, Refresh);
                else
                    MessageBox.Show("No data returned from server!");
            }
        }

        private async void Refresh(int? salesOrderId)
        {
            await _presenter.RefreshAsync();
            _presenter.EnsureVisible(salesOrderId);
        }

        private void CanOpen(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _presenter.CurrentRow != null;
        }

        private void Delete(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedRows = _presenter.SelectedRows;
            var messageBoxText = string.Format("Are you sure you want to delete selected {0} rows?", selectedRows.Count);
            var caption = "Delete Sales Order(s)";
            if (MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.No) == MessageBoxResult.No)
                return;

            var keys = DataSet<SalesOrderHeader.Key>.ParseJson(_presenter.DataSet.Filter(JsonFilter.PrimaryKeyOnly).ToJsonString(_presenter.SelectedDataRows, false));
            var success = App.Execute((db, ct) => db.DeleteSalesOrderAsync(keys, ct), this, caption);
            if (success)
                RefreshList();
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            var selectedRows = _presenter.SelectedRows;
            e.CanExecute = selectedRows != null && selectedRows.Count > 0;
        }

        private void Refresh(object sender, ExecutedRoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            _presenter.RefreshAsync();
        }

        private void CanRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _presenter.DataSet != null;
        }

        private void Search(object sender, ExecutedRoutedEventArgs e)
        {
            _presenter.SearchText = _searchBox.SearchText;
        }

        private void ClearSearch(object sender, ExecutedRoutedEventArgs e)
        {
            _presenter.SearchText = null;
        }

        private void Close(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private Presenter _presenter;
    }
}

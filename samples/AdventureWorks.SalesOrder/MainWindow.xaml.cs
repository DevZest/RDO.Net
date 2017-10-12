using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly RoutedUICommand ResetFilterCommand = new RoutedUICommand();

        public MainWindow()
        {
            InitializeComponent();
            InitializeCommandBindings();
            _salesOrderList = new Presenter();
            _salesOrderList.ShowAsync(_dataView);
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, New));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, Open, CanOpen));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, Delete, CanDelete));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, Refresh, CanRefresh));
            CommandBindings.Add(new CommandBinding(ResetFilterCommand, ResetFilter, CanResetFilter));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, Close));
        }

        private SalesOrder _
        {
            get { return _salesOrderList._; }
        }

        private void New(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("New!");
        }

        private void Open(object sender, ExecutedRoutedEventArgs e)
        {
            var salesOrderId = _salesOrderList.CurrentRow.GetValue(_.SalesOrderID);
            MessageBox.Show(string.Format("Open: {0}", salesOrderId));
        }

        private void CanOpen(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _salesOrderList.CurrentRow != null;
        }

        private void Delete(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedRows = _salesOrderList.SelectedRows;
            var messageBoxText = string.Format("Are you sure you want to delete selected {0} rows?", selectedRows.Count);
            var caption = "Delete Sales Order(s)";
            if (MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.No) == MessageBoxResult.No)
                return;

            var refs = DataSet<SalesOrder.Ref>.New();
            foreach (var rowPresenter in selectedRows)
            {
                var id = rowPresenter.GetValue(_.SalesOrderID);
                refs.AddRow((_, row) => _.SalesOrderID[row] = id);
            }

            var success = App.Execute(ct => Data.DeleteAsync(refs, ct), this, caption);
            if (success)
                RefreshList();
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            var selectedRows = _salesOrderList.SelectedRows;
            e.CanExecute = selectedRows != null && selectedRows.Count > 0;
        }

        private void Refresh(object sender, ExecutedRoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            _salesOrderList.RefreshAsync();
        }

        private void CanRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _salesOrderList.DataSet != null;
        }

        private void ResetFilter(object sender, ExecutedRoutedEventArgs e)
        {
            _findTextBox.Text = null;
        }

        private void CanResetFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrEmpty(_findTextBox.Text);
        }

        private void Close(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private Presenter _salesOrderList;

        private void FindTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _salesOrderList.SearchText = _findTextBox.Text;
        }
    }
}

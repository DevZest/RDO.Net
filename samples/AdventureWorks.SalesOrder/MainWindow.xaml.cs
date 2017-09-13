using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeCommandBindings();
            _salesOrderList = new SalesOrderList();
            _salesOrderList.Show(_dataView, LoadSalesOrders());
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, New));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, Open, CanOpen));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, Delete, CanDelete));
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
            var selectedRows = _salesOrderList.SelectedRows.ToArray();
            var salesOrderIds = new int?[selectedRows.Length];
            for (int i = 0; i < selectedRows.Length; i++)
                salesOrderIds[i] = selectedRows[i].GetValue(_.SalesOrderID);
            MessageBox.Show(string.Format("Delete: {0}", string.Join(", ", salesOrderIds)));
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _salesOrderList.SelectedRows.Count > 0;
        }

        private SalesOrderList _salesOrderList;

        private DataSet<SalesOrder> LoadSalesOrders()
        {
            using (var db = Db.Open(App.ConnectionString))
            {
                return db.SalesOrders.ToDataSet();
            }
        }
    }
}

using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;
using System.Windows;

namespace SampleApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _salesOrderList = new SalesOrderList();
            _salesOrderList.Show(_dataView, LoadSalesOrders());
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

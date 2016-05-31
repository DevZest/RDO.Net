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
            _salesOrders = LoadSalesOrders();
            _salesOrderList.Show(_salesOrders);
        }

        private DataSet<SalesOrder> _salesOrders;

        private DataSet<SalesOrder> LoadSalesOrders()
        {
            using (var db = Db.Open(App.ConnectionString))
            {
                return db.SalesOrders.ToDataSet();
            }
        }
    }
}

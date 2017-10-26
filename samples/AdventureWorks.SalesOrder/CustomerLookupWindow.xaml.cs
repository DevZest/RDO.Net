using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Samples.AdventureWorksLT;
using System.Windows;
using System.Windows.Controls;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for CustomerLookupWindow.xaml
    /// </summary>
    public partial class CustomerLookupWindow : Window
    {
        public CustomerLookupWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private Customer.Key _key;
        private Customer.Lookup _lookup;
        public ColumnValueBag Result { get; private set; }

        public void Show(Window ownerWindow, int? currentCustomerID, Customer.Key key, Customer.Lookup lookup)
        {
            Owner = ownerWindow;
            _key = key;
            _lookup = lookup;
            _presenter = new Presenter(_dataView, currentCustomerID);
            ShowDialog();
        }

        private void FindTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}

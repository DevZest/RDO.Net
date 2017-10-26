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

        private Presenter _presenter = new Presenter();
        private Customer.Key _key;
        private Customer.Lookup _lookup;
        public ColumnValueBag Result { get; private set; }

        public void Show(Window ownerWindow, DataSet<Customer> data, int? currentCustomerID, Customer.Key key, Customer.Lookup lookup)
        {
            Owner = ownerWindow;
            _key = key;
            _lookup = lookup;
            _currentCustomerID = currentCustomerID;
            _presenter.Show(_dataView, data);
            _dataView.Loaded += OnDataViewLoaded;
            ShowDialog();
        }

        int? _currentCustomerID;
        private void OnDataViewLoaded(object sender, RoutedEventArgs e)
        {
            _dataView.Loaded -= OnDataViewLoaded;
            Select(_currentCustomerID);
        }

        private void Select(int? currentCustomerID)
        {
            if (!currentCustomerID.HasValue)
                return;

            var current = GetRow(currentCustomerID.Value);
            if (current != null)
                _presenter.Select(current, SelectionMode.Single);
        }

        private RowPresenter GetRow(int currentCustomerID)
        {
            var _ = _presenter._;
            foreach (var row in _presenter.Rows)
            {
                if (row.GetValue(_.CustomerID) == currentCustomerID)
                    return row;
            }
            return null;
        }
    }
}

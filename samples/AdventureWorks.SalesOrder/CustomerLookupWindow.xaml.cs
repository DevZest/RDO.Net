using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Samples.AdventureWorksLT;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            InitializeCommandBindings();
            ShowDialog();
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, Open, CanOpen));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, Refresh, CanRefresh));
            CommandBindings.Add(new CommandBinding(SearchBox.SearchCommand, Search, CanRefresh));
            CommandBindings.Add(new CommandBinding(SearchBox.ClearSearchCommand, ClearSearch, CanRefresh));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, Close));
        }

        private void Open(object sender, ExecutedRoutedEventArgs e)
        {
            Result = _presenter.CurrentRow.CreateValueBag(_key, _lookup);
            Close();
        }

        private void CanOpen(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _presenter.CurrentRow != null;
        }

        private void Refresh(object sender, ExecutedRoutedEventArgs e)
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
    }
}

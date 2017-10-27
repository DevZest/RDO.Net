using DevZest.Data.Presenters;
using DevZest.Data.Views;
using DevZest.Samples.AdventureWorksLT;
using System.Windows;
using System.Windows.Input;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for CustomerLookupWindow.xaml
    /// </summary>
    public partial class CustomerLookupWindow : Window
    {
        public static RoutedUICommand SelectCurrentCommand { get { return ApplicationCommands.Open; } }

        public CustomerLookupWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private ForeignKeyBox _foreignKeyBox;
        private ForeignKeyBox _shipToAddressBox;
        private ForeignKeyBox _billToAddressBox;
        private Customer.Key Key
        {
            get { return (Customer.Key)_foreignKeyBox.ForeignKey; }
        }
        private Customer.Lookup Lookup
        {
            get { return (Customer.Lookup)_foreignKeyBox.Extension; }
        }

        public void Show(Window ownerWindow, ForeignKeyBox foreignKeyBox, int? currentCustomerID, ForeignKeyBox shipToAddressBox, ForeignKeyBox billToAddressBox)
        {
            Owner = ownerWindow;
            _foreignKeyBox = foreignKeyBox;
            _shipToAddressBox = shipToAddressBox;
            _billToAddressBox = billToAddressBox;
            _presenter = new Presenter(_dataView, currentCustomerID);
            InitializeCommandBindings();
            ShowDialog();
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(SelectCurrentCommand, SelectCurrent, CanSelectCurrent));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, Refresh, CanRefresh));
            CommandBindings.Add(new CommandBinding(SearchBox.SearchCommand, Search, CanRefresh));
            CommandBindings.Add(new CommandBinding(SearchBox.ClearSearchCommand, ClearSearch, CanRefresh));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, Close));
        }

        private RowPresenter CurrentRow
        {
            get { return _presenter.CurrentRow; }
        }

        private Customer _
        {
            get { return _presenter._; }
        }

        private void SelectCurrent(object sender, ExecutedRoutedEventArgs e)
        {
            if (_presenter.CurrentCustomerID != CurrentRow.GetValue(_.CustomerID))
            {
                _foreignKeyBox.EndLookup(CurrentRow.AutoSelect(Key, Lookup));
                _shipToAddressBox?.ClearValue();
                _billToAddressBox?.ClearValue();
            }
            Close();
        }

        private void CanSelectCurrent(object sender, CanExecuteRoutedEventArgs e)
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

using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DevZest.Samples.AdventureWorksLT
{
    /// <summary>
    /// Interaction logic for CustomerLookupWindow.xaml
    /// </summary>
    public partial class CustomerLookupWindow : Window
    {
        public static class Commands
        {
            public static RoutedUICommand SelectCurrent { get { return ApplicationCommands.Open; } }
            public static RoutedUICommand Refresh { get { return NavigationCommands.Refresh; } }
            public static RoutedUICommand Search { get { return SearchBox.Commands.Search; } }
            public static RoutedUICommand ClearSearch { get { return SearchBox.Commands.ClearSearch; } }
            public static RoutedUICommand Close { get { return ApplicationCommands.Close; } }
        }

        public CustomerLookupWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private ForeignKeyBox _foreignKeyBox;
        private ForeignKeyBox _shipToAddressBox;
        private ForeignKeyBox _billToAddressBox;
        private Customer.PK FK
        {
            get { return (Customer.PK)_foreignKeyBox.ForeignKey; }
        }
        private Customer.Lookup Lookup
        {
            get { return (Customer.Lookup)_foreignKeyBox.Lookup; }
        }

        public void Show(Window ownerWindow, ForeignKeyBox foreignKeyBox, int? currentCustomerID, ForeignKeyBox shipToAddressBox, ForeignKeyBox billToAddressBox)
        {
            Debug.Assert(ownerWindow != null);
            Debug.Assert(foreignKeyBox != null);
            Debug.Assert(shipToAddressBox != null);
            Debug.Assert(billToAddressBox != null);

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
            CommandBindings.Add(new CommandBinding(Commands.SelectCurrent, SelectCurrent, CanSelectCurrent));
            CommandBindings.Add(new CommandBinding(Commands.Refresh, Refresh, CanRefresh));
            CommandBindings.Add(new CommandBinding(Commands.Search, Search, CanRefresh));
            CommandBindings.Add(new CommandBinding(Commands.ClearSearch, ClearSearch, CanRefresh));
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
                _foreignKeyBox.EndLookup(CurrentRow.MakeValueBag(FK, Lookup));
                _shipToAddressBox.ClearValue();
                _billToAddressBox.ClearValue();
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
            e.Handled = true;
        }

        private void ClearSearch(object sender, ExecutedRoutedEventArgs e)
        {
            _presenter.SearchText = null;
            e.Handled = true;
        }

        private void Close(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
            e.Handled = true;
        }
    }
}

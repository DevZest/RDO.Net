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
    public partial class AddressLookupWindow : Window
    {
        public static RoutedUICommand SelectCurrentCommand { get { return ApplicationCommands.Open; } }

        public AddressLookupWindow()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private Address.Key _key;
        private Address.Lookup _lookup;
        public ColumnValueBag Result { get; private set; }

        public void Show(Window ownerWindow, int? currentAddressID, int customerID, Address.Key key, Address.Lookup lookup)
        {
            Owner = ownerWindow;
            _key = key;
            _lookup = lookup;
            _presenter = new Presenter(_dataView, currentAddressID, customerID);
            InitializeCommandBindings();
            ShowDialog();
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(SelectCurrentCommand, SelectCurrent, CanSelectCurrent));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, Refresh, CanRefresh));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, Close));
        }

        private void SelectCurrent(object sender, ExecutedRoutedEventArgs e)
        {
            Result = _presenter.CurrentRow.AutoSelect(_key, _lookup);
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

        private void Close(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}

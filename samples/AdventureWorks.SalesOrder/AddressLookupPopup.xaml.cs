using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using DevZest.Samples.AdventureWorksLT;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for CustomerLookupWindow.xaml
    /// </summary>
    public partial class AddressLookupPopup : Popup
    {
        public static RoutedUICommand SelectCurrentCommand { get { return ApplicationCommands.Open; } }

        public AddressLookupPopup()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private ForeignKeyBox _foreignKeyBox;
        public Address.Key Key
        {
            get { return _foreignKeyBox == null ? null : (Address.Key)_foreignKeyBox.ForeignKey; }
        }
        private Address.Lookup Lookup
        {
            get { return (Address.Lookup)_foreignKeyBox.Extension; }
        }

        public void Show(ForeignKeyBox foreignKeyBox, int? currentAddressID, int customerID)
        {
            if (IsOpen)
                IsOpen = false;
            PlacementTarget = _foreignKeyBox = foreignKeyBox;
            _presenter = new Presenter(_dataView, currentAddressID, customerID);
            InitializeCommandBindings();
            IsOpen = true;
        }

        private void Popup_Closed(object sender, System.EventArgs e)
        {
            PlacementTarget = _foreignKeyBox = null;
            _presenter.DetachView();
            _presenter = null;
            CommandBindings.Clear();
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(SelectCurrentCommand, SelectCurrent, CanSelectCurrent));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, Refresh, CanRefresh));
        }

        private void SelectCurrent(object sender, ExecutedRoutedEventArgs e)
        {
            _foreignKeyBox.EndLookup(_presenter.CurrentRow.AutoSelect(Key, Lookup));
            IsOpen = false;
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
    }
}

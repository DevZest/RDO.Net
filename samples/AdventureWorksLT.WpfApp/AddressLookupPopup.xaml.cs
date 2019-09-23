using DevZest.Data.Views;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DevZest.Samples.AdventureWorksLT
{
    /// <summary>
    /// Interaction logic for CustomerLookupWindow.xaml
    /// </summary>
    public partial class AddressLookupPopup : Popup
    {
        public AddressLookupPopup()
        {
            InitializeComponent();
        }

        private Presenter _presenter;
        private ForeignKeyBox _foreignKeyBox;
        public Address.PK FK
        {
            get { return _foreignKeyBox == null ? null : (Address.PK)_foreignKeyBox.ForeignKey; }
        }
        private Address.Lookup Lookup
        {
            get { return (Address.Lookup)_foreignKeyBox.Lookup; }
        }

        public void Show(ForeignKeyBox foreignKeyBox, int? currentAddressID, int customerID)
        {
            if (IsOpen)
                IsOpen = false;
            PlacementTarget = _foreignKeyBox = foreignKeyBox;
            _foreignKeyBox.IsEnabled = false;
            _presenter = new Presenter(_dataView, currentAddressID, customerID);
            InitializeCommandBindings();
            IsOpen = true;
        }

        private void Popup_Closed(object sender, System.EventArgs e)
        {
            _foreignKeyBox.IsEnabled = true;
            PlacementTarget = _foreignKeyBox = null;
            _presenter.DetachView();
            _presenter = null;
            CommandBindings.Clear();
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(Commands.SelectCurrent, SelectCurrent, CanSelectCurrent));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, Refresh, CanRefresh));
        }

        private void SelectCurrent(object sender, ExecutedRoutedEventArgs e)
        {
            _foreignKeyBox.EndLookup(_presenter.CurrentRow.MakeValueBag(FK, Lookup));
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

using DevZest.Data.Presenters;
using DevZest.Samples.AdventureWorksLT;
using System;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for AddressBox.xaml
    /// </summary>
    public partial class AddressBox
    {
        public AddressBox()
        {
            InitializeComponent();
        }

        private static void Refresh(AddressBox v, Address.Lookup _, RowPresenter p)
        {
            v._addressLine1.Text = p.GetValue(_.AddressLine1);
            v._addressLine2.Text = p.GetValue(_.AddressLine2);
            v._city.Text = p.GetValue(_.City);
            v._stateProvince.Text = p.GetValue(_.StateProvince);
            v._countryRegion.Text = p.GetValue(_.CountryRegion);
            v._postalCode.Text = p.GetValue(_.PostalCode);
        }

        public static Action<AddressBox, Address.Lookup, RowPresenter> RefreshAction
        {
            get { return Refresh; }
        }
    }
}

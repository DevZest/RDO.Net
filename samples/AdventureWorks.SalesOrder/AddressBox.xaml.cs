using DevZest.Data;
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

        private static void Refresh(AddressBox v, Address.Lookup _, ColumnValueBag valueBag)
        {
            v._addressLine1.Text = valueBag.GetValue(_.AddressLine1);
            v._addressLine2.Text = valueBag.GetValue(_.AddressLine2);
            v._city.Text = valueBag.GetValue(_.City);
            v._stateProvince.Text = valueBag.GetValue(_.StateProvince);
            v._countryRegion.Text = valueBag.GetValue(_.CountryRegion);
            v._postalCode.Text = valueBag.GetValue(_.PostalCode);
        }

        public static Action<AddressBox, Address.Lookup, ColumnValueBag> RefreshAction
        {
            get { return Refresh; }
        }
    }
}

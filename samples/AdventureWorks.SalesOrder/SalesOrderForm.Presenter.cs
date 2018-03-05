using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using DevZest.Data;
using System.Windows;
using System;

namespace AdventureWorks.SalesOrders
{
    partial class SalesOrderForm
    {
        public static class Styles
        {
            public static readonly StyleId DataSheet = new StyleId(typeof(SalesOrderForm));
        }

        private class Presenter : DataPresenter<SalesOrderToEdit>, ForeignKeyBox.ILookupService
        {
            public Presenter(Window ownerWindow, AddressLookupPopup addressLookupPopup)
            {
                _ownerWindow = ownerWindow;
                _addressLookupPopup = addressLookupPopup;
            }

            private Window _ownerWindow;
            private AddressLookupPopup _addressLookupPopup;
            private RowBinding<ForeignKeyBox> _shipToAddressBinding;
            private RowBinding<ForeignKeyBox> _billToAddressBinding;

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridRows("Auto", "*", "Auto")
                    .GridColumns("580")
                    .AddBinding(0, 0, _.BindToSalesOrderHeaderBox(out _shipToAddressBinding, out _billToAddressBinding))
                    .AddBinding(0, 1, _.SalesOrderDetails.BindToDataView(() => new DetailPresenter()).WithStyle(Styles.DataSheet))
                    .AddBinding(0, 2, _.BindToSalesOrderFooterBox());
            }

            bool ForeignKeyBox.ILookupService.CanLookup(PrimaryKey foreignKey)
            {
                if (foreignKey == _.Customer)
                    return true;
                else if (foreignKey == _.BillToAddress)
                    return true;
                else if (foreignKey == _.ShipToAddress)
                    return true;
                else
                    return false;
            }

            void ForeignKeyBox.ILookupService.BeginLookup(ForeignKeyBox foreignKeyBox)
            {
                if (foreignKeyBox.ForeignKey == _.Customer)
                {
                    var dialogWindow = new CustomerLookupWindow();
                    dialogWindow.Show(_ownerWindow, foreignKeyBox, CurrentRow.GetValue(_.CustomerID), _shipToAddressBinding[CurrentRow], _billToAddressBinding[CurrentRow]);
                }
                else if (foreignKeyBox.ForeignKey == _.ShipToAddress || foreignKeyBox.ForeignKey == _.BillToAddress)
                    BeginLookupAddress(foreignKeyBox);
                else
                    throw new NotSupportedException();
            }

            private void BeginLookupAddress(ForeignKeyBox foreignKeyBox)
            {
                var foreignKey = (Address.Key)foreignKeyBox.ForeignKey;
                if (_addressLookupPopup.Key == foreignKey)
                    _addressLookupPopup.IsOpen = false;
                else
                    _addressLookupPopup.Show(foreignKeyBox, CurrentRow.GetValue(foreignKey.AddressID), CurrentRow.GetValue(_.CustomerID).Value);
            }
        }
    }
}

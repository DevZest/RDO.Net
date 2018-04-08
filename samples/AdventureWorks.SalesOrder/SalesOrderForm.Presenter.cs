using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using DevZest.Data;
using System.Windows;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace AdventureWorks.SalesOrders
{
    partial class SalesOrderForm
    {
        public static class Styles
        {
            public static readonly StyleId DataSheet = new StyleId(typeof(SalesOrderForm));
        }

        private class Presenter : DataPresenter<SalesOrderInfo>, ForeignKeyBox.ILookupService
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
            private RowBinding<DataView> _subFormBinding;

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                _subFormBinding = _.SalesOrderDetails.BindToDataView(() => new DetailPresenter(_ownerWindow)).WithStyle(Styles.DataSheet);
                builder.GridRows("Auto", "*", "Auto")
                    .GridColumns("580")
                    .AddBinding(0, 0, _.BindToSalesOrderHeaderBox(IsNew, out _shipToAddressBinding, out _billToAddressBinding))
                    .AddBinding(0, 1, _subFormBinding)
                    .AddBinding(0, 2, _.BindToSalesOrderFooterBox());
            }

            public DetailPresenter CurrentRowDetailPresenter
            {
                get { return (DetailPresenter)_subFormBinding[CurrentRow].DataPresenter; }
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
                {
                    var customerID = CurrentRow.GetValue(_.CustomerID);
                    if (customerID.HasValue)
                        _addressLookupPopup.Show(foreignKeyBox, CurrentRow.GetValue(foreignKey.AddressID), customerID.Value);
                }
            }

            public int SalesOrderId
            {
                get { return _.SalesOrderID[0].Value; }
            }

            public bool IsNew
            {
                get { return SalesOrderId < 1; }
            }

            public override bool SubmitInput(bool focusToErrorInput = true)
            {
                if (!base.SubmitInput(focusToErrorInput))
                    return false;

                var detailsPresenter = _subFormBinding[CurrentRow].DataPresenter;
                return detailsPresenter.SubmitInput(focusToErrorInput);
            }

            public Task SaveToDb(CancellationToken ct)
            {
                if (IsNew)
                    return Data.CreateSalesOrder(DataSet, ct);
                else
                    return Data.UpdateSalesOrder(DataSet, ct);
            }
        }
    }
}

using DevZest.Data.Presenters;
using DevZest.Data.Views;
using DevZest.Data;
using System.Windows;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace DevZest.Samples.AdventureWorksLT
{
    partial class SalesOrderWindow
    {
        public static class Styles
        {
            public static readonly StyleId DataSheet = new StyleId(typeof(SalesOrderWindow));
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
                var lineCountValidation = _.LineCount.BindToValidationPlaceholder(_subFormBinding);
                builder.GridRows("Auto", "*", "Auto")
                    .GridColumns("580")
                    .AddBinding(0, 0, _.BindToSalesOrderHeaderBox(IsNew, out _shipToAddressBinding, out _billToAddressBinding))
                    .AddBinding(0, 1, lineCountValidation)
                    .AddBinding(0, 1, _subFormBinding)
                    .AddBinding(0, 2, _.BindToSalesOrderFooterBox());
            }

            public DetailPresenter CurrentRowDetailPresenter
            {
                get { return (DetailPresenter)_subFormBinding[CurrentRow].DataPresenter; }
            }

            bool ForeignKeyBox.ILookupService.CanLookup(CandidateKey foreignKey)
            {
                if (foreignKey == _.FK_Customer)
                    return true;
                else if (foreignKey == _.FK_BillToAddress)
                    return true;
                else if (foreignKey == _.FK_ShipToAddress)
                    return true;
                else
                    return false;
            }

            void ForeignKeyBox.ILookupService.BeginLookup(ForeignKeyBox foreignKeyBox)
            {
                if (foreignKeyBox.ForeignKey == _.FK_Customer)
                {
                    var dialogWindow = new CustomerLookupWindow();
                    dialogWindow.Show(_ownerWindow, foreignKeyBox, CurrentRow.GetValue(_.CustomerID), _shipToAddressBinding[CurrentRow], _billToAddressBinding[CurrentRow]);
                }
                else if (foreignKeyBox.ForeignKey == _.FK_ShipToAddress || foreignKeyBox.ForeignKey == _.FK_BillToAddress)
                    BeginLookupAddress(foreignKeyBox);
                else
                    throw new NotSupportedException();
            }

            private void BeginLookupAddress(ForeignKeyBox foreignKeyBox)
            {
                var foreignKey = (Address.PK)foreignKeyBox.ForeignKey;
                if (_addressLookupPopup.FK == foreignKey)
                    _addressLookupPopup.IsOpen = false;
                else
                {
                    var customerID = CurrentRow.GetValue(_.CustomerID);
                    if (customerID.HasValue)
                    {
                        var addressID = foreignKeyBox.ForeignKey == _.FK_ShipToAddress ? _.ShipToAddressID : _.BillToAddressID;
                        _addressLookupPopup.Show(foreignKeyBox, CurrentRow.GetValue(addressID), customerID.Value);
                    }
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

            public async Task<int?> SaveToDb(CancellationToken ct)
            {
                if (IsNew)
                    return await App.ExecuteAsync(db => db.CreateSalesOrderAsync(DataSet, ct));
                else
                {
                    await App.ExecuteAsync(db => db.UpdateSalesOrderAsync(DataSet, ct));
                    return null;
                }
            }
        }
    }
}

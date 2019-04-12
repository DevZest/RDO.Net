using DevZest.Data.Presenters;
using DevZest.Data.Views;

namespace DevZest.Samples.AdventureWorksLT
{
    public static class BindingFactory
    {
        public static RowCompositeBinding<SalesOrderHeaderBox> BindToSalesOrderHeaderBox(this SalesOrderInfo _, bool isNew,
            out RowBinding<ForeignKeyBox> shipToAddressBinding, out RowBinding<ForeignKeyBox> billToAddressBinding)
        {
            var result = new RowCompositeBinding<SalesOrderHeaderBox>()
                .AddChild(_.FK_Customer.BindToForeignKeyBox(_.Customer, CustomerBox.RefreshAction), v => v._customer)
                .AddChild(shipToAddressBinding = _.FK_ShipToAddress.BindToForeignKeyBox(_.ShipToAddress, AddressBox.RefreshAction), v => v._shipTo)
                .AddChild(billToAddressBinding = _.FK_BillToAddress.BindToForeignKeyBox(_.BillToAddress, AddressBox.RefreshAction), v => v._billTo)
                .AddChild(_.OrderDate.BindToDatePicker(), v => v._orderDate)
                .AddChild(_.ShipDate.BindToDatePicker(), v => v._shipDate)
                .AddChild(_.DueDate.BindToDatePicker(), v => v._dueDate)
                .AddChild(_.PurchaseOrderNumber.BindToTextBox(), v => v._purchaseOrderNumber)
                .AddChild(_.AccountNumber.BindToTextBox(), v => v._accountNumber)
                .AddChild(_.ShipMethod.BindToTextBox(), v => v._shipMethod)
                .AddChild(_.CreditCardApprovalCode.BindToTextBox(), v => v._creditCardApprovalCode)
                .AddChild(_.Status.BindToComboBox(), v => v._status)
                .AddChild(_.OnlineOrderFlag.BindToCheckBox(), v => v._onlineOrderFlag)
                .AddChild(_.Comment.BindToTextBox(), v => v._comment);
            if (!isNew)
                result.AddChild(_.SalesOrderNumber.BindToTextBlock(), v => v._salesOrderNumber);
            return result;
        }

        public static RowCompositeBinding<SalesOrderFooterBox> BindToSalesOrderFooterBox<T>(this SalesOrderBase<T> _)
            where T : SalesOrderDetail, new()
        {
            return new RowCompositeBinding<SalesOrderFooterBox>()
                .AddChild(_.SubTotal.BindToTextBlock("{0:C}"), v => v._subTotal)
                .AddChild(_.Freight.BindToTextBox(), v => v._freight)
                .AddChild(_.TaxAmt.BindToTextBox(), v => v._taxAmt)
                .AddChild(_.TotalDue.BindToTextBlock("{0:C}"), v => v._totalDue);
        }

        public static RowCompositeBinding<AddressBox> BindToAddressBox(this Address _)
        {
            return new RowCompositeBinding<AddressBox>()
                .AddChild(_.AddressLine1.BindToTextBlock(), v => v._addressLine1)
                .AddChild(_.AddressLine2.BindToTextBlock(), v => v._addressLine2)
                .AddChild(_.City.BindToTextBlock(), v => v._city)
                .AddChild(_.StateProvince.BindToTextBlock(), v => v._stateProvince)
                .AddChild(_.CountryRegion.BindToTextBlock(), v => v._countryRegion)
                .AddChild(_.PostalCode.BindToTextBlock(), v => v._postalCode);
        }
    }
}

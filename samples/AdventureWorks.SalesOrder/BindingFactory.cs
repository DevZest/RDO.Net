using DevZest.Data.Presenters;
using DevZest.Data.Views;
using DevZest.Samples.AdventureWorksLT;
using System;

namespace AdventureWorks.SalesOrders
{
    public static class BindingFactory
    {
        public static RowCompositeBinding<SalesOrderHeaderBox> AsSalesOrderHeaderBox(this SalesOrderToEdit _,
            out RowBinding<ForeignKeyBox> shipToAddressBinding, out RowBinding<ForeignKeyBox> billToAddressBinding)
        {
            var ext = _.GetExtension<SalesOrderToEdit.Ext>();
            return new RowCompositeBinding<SalesOrderHeaderBox>()
                .AddChild(_.Customer.AsForeignKeyBox(ext.Customer, CustomerBox.RefreshAction), v => v._customer)
                .AddChild(shipToAddressBinding = _.ShipToAddress.AsForeignKeyBox(ext.ShipToAddress, AddressBox.RefreshAction), v => v._shipTo)
                .AddChild(billToAddressBinding = _.BillToAddress.AsForeignKeyBox(ext.BillToAddress, AddressBox.RefreshAction), v => v._billTo)
                .AddChild(_.OrderDate.AsDatePicker(), v => v._orderDate)
                .AddChild(_.ShipDate.AsDatePicker(), v => v._shipDate)
                .AddChild(_.DueDate.AsDatePicker(), v => v._dueDate)
                .AddChild(_.SalesOrderNumber.AsTextBlock(), v => v._salesOrderNumber)
                .AddChild(_.PurchaseOrderNumber.AsTextBox(), v => v._purchaseOrderNumber)
                .AddChild(_.AccountNumber.AsTextBox(), v => v._accountNumber)
                .AddChild(_.ShipMethod.AsTextBox(), v => v._shipMethod)
                .AddChild(_.CreditCardApprovalCode.AsTextBox(), v => v._creditCardApprovalCode)
                .AddChild(_.Status.AsComboBox(), v => v._status)
                .AddChild(_.OnlineOrderFlag.AsCheckBox(), v => v._onlineOrderFlag)
                .AddChild(_.Comment.AsTextBox(), v => v._comment);
        }

        public static RowCompositeBinding<SalesOrderFooterBox> AsSalesOrderFooterBox(this SalesOrder _)
        {
            return new RowCompositeBinding<SalesOrderFooterBox>()
                .AddChild(_.SubTotal.AsTextBlock("{0:C}"), v => v._subTotal)
                .AddChild(_.Freight.AsTextBox(), v => v._freight)
                .AddChild(_.TaxAmt.AsTextBox(), v => v._taxAmt)
                .AddChild(_.TotalDue.AsTextBlock("{0:C}"), v => v._totalDue);
        }

        public static RowCompositeBinding<AddressBox> AsAddressBox(this Address _)
        {
            return new RowCompositeBinding<AddressBox>()
                .AddChild(_.AddressLine1.AsTextBlock(), v => v._addressLine1)
                .AddChild(_.AddressLine2.AsTextBlock(), v => v._addressLine2)
                .AddChild(_.City.AsTextBlock(), v => v._city)
                .AddChild(_.StateProvince.AsTextBlock(), v => v._stateProvince)
                .AddChild(_.CountryRegion.AsTextBlock(), v => v._countryRegion)
                .AddChild(_.PostalCode.AsTextBlock(), v => v._postalCode);
        }
    }
}

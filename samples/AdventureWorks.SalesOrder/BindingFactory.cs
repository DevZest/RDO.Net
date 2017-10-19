using DevZest.Data.Presenters;
using DevZest.Samples.AdventureWorksLT;
using System;

namespace AdventureWorks.SalesOrders
{
    public static class BindingFactory
    {
        public static CompositeRowBinding<SalesOrderHeaderBox> AsSalesOrderHeaderBox(this SalesOrder _)
        {
            var ext = _.GetExtension<SalesOrder.Ext>();
            return new CompositeRowBinding<SalesOrderHeaderBox>()
                .AddChild(_.Customer.AsForeignKeyBox(ext.Customer, CustomerBox.RefreshAction), v => v._customer)
                .AddChild(_.ShipToAddress.AsForeignKeyBox(ext.ShipToAddress, AddressBox.RefreshAction), v => v._shipTo)
                .AddChild(_.BillToAddress.AsForeignKeyBox(ext.BillToAddress, AddressBox.RefreshAction), v => v._billTo)
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

        public static CompositeRowBinding<SalesOrderFooterBox> AsSalesOrderFooterBox(this SalesOrder _)
        {
            return new CompositeRowBinding<SalesOrderFooterBox>()
                .AddChild(_.SubTotal.AsTextBlock("{0:C}"), v => v._subTotal)
                .AddChild(_.Freight.AsTextBox(), v => v._freight)
                .AddChild(_.TaxAmt.AsTextBox(), v => v._taxAmt)
                .AddChild(_.TotalDue.AsTextBlock("{0:C}"), v => v._totalDue);
        }
    }
}

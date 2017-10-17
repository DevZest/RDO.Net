﻿using DevZest.Data.Presenters;
using DevZest.Samples.AdventureWorksLT;
using System;

namespace AdventureWorks.SalesOrders
{
    public static class BindingFactory
    {
        public static Action<CustomerBox, Customer.Key, RowPresenter> ToCustomerBox(this Customer.Lookup _)
        {
            return (v, key, p) =>
            {
                var customerID = p.GetValue(key.CustomerID);
                v._companyName.Text = customerID == null ? null : p.GetValue(_.CompanyName);
                v._contactPerson.Text = customerID == null ? null : GetContactPerson(p.GetValue(_.LastName), p.GetValue(_.FirstName), p.GetValue(_.Title));
                v._phone.Text = customerID == null ? null : p.GetValue(_.Phone);
                v._email.Text = customerID == null ? null : p.GetValue(_.EmailAddress);
            };
        }

        private static string GetContactPerson(string lastName, string firstName, string title)
        {
            string result = string.IsNullOrEmpty(lastName) ? string.Empty : lastName.ToUpper();
            if (!string.IsNullOrEmpty(firstName))
            {
                result += ", ";
                result += firstName;
            }
            if (!string.IsNullOrEmpty(title))
            {
                result += " (";
                result += title;
                result += ")";
            }

            return result;
        }

        public static Action<AddressBox, Address.Key, RowPresenter> ToAddressBox(this Address.Lookup _)
        {
            return (v, key, p) =>
            {
                var addressID = p.GetValue(key.AddressID);
                v._addressLine1.Text = addressID == null ? null : p.GetValue(_.AddressLine1);
                v._addressLine2.Text = addressID == null ? null : p.GetValue(_.AddressLine2);
                v._city.Text = addressID == null ? null : p.GetValue(_.City);
                v._stateProvince.Text = addressID == null ? null : p.GetValue(_.StateProvince);
                v._countryRegion.Text = addressID == null ? null : p.GetValue(_.CountryRegion);
                v._postalCode.Text = addressID == null ? null : p.GetValue(_.PostalCode);
            };
        }

        public static CompositeRowBinding<SalesOrderHeaderBox> AsSalesOrderHeaderBox(this SalesOrder _)
        {
            var ext = _.GetExtension<SalesOrder.Ext>();
            return new CompositeRowBinding<SalesOrderHeaderBox>()
                .AddChild(_.Customer.AsForeignKeyBox(ext.Customer.ToCustomerBox()), v => v._customer)
                .AddChild(_.ShipToAddress.AsForeignKeyBox(ext.ShipToAddress.ToAddressBox()), v => v._shipTo)
                .AddChild(_.BillToAddress.AsForeignKeyBox(ext.BillToAddress.ToAddressBox()), v => v._billTo)
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

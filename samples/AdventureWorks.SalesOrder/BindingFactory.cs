using DevZest.Data.Presenters;
using DevZest.Samples.AdventureWorksLT;
using System;

namespace AdventureWorks.SalesOrders
{
    public static class BindingFactory
    {
        public static Action<CustomerBox, RowPresenter> AsCustomerBox(this Customer.Key key, Customer.Lookup _)
        {
            return (v, p) =>
            {
                var customerID = p.GetValue(key.CustomerID);
                v.CompanyName = customerID == null ? null : p.GetValue(_.CompanyName);
                v.ContactPerson = customerID == null ? null : GetContactPerson(p.GetValue(_.LastName), p.GetValue(_.FirstName), p.GetValue(_.Title));
                v.Phone = customerID == null ? null : p.GetValue(_.Phone);
                v.Email = customerID == null ? null : p.GetValue(_.EmailAddress);
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

        public static Action<AddressBox, RowPresenter> AsAddressBox(this Address.Key key, Address.Lookup _)
        {
            return (v, p) =>
            {
                var addressID = p.GetValue(key.AddressID);
                v.AddressLine1 = addressID == null ? null : p.GetValue(_.AddressLine1);
                v.AddressLine2 = addressID == null ? null : p.GetValue(_.AddressLine2);
                v.City = addressID == null ? null : p.GetValue(_.City);
                v.StateProvince = addressID == null ? null : p.GetValue(_.StateProvince);
                v.CountryRegion = addressID == null ? null : p.GetValue(_.CountryRegion);
                v.PostalCode = addressID == null ? null : p.GetValue(_.PostalCode);
            };
        }
    }
}

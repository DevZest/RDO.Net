using DevZest.Data.Presenters;
using DevZest.Samples.AdventureWorksLT;
using System;

namespace AdventureWorks.SalesOrders
{
    public static class BindingFactory
    {
        public static Action<CustomerBox, RowPresenter> AsCustomerBox(Customer.Lookup _, Customer.Key key)
        {
            return (v, p) =>
            {
                var customerID = p.GetValue(key.CustomerID);
                if (customerID == null)
                {
                    v.CompanyName = "Company";
                    v.ContactPerson = null;
                    v.Phone = null;
                    v.Email = null;
                }
                else
                {
                    v.CompanyName = p.GetValue(_.CompanyName);
                    v.ContactPerson = GetContactPerson(p.GetValue(_.LastName), p.GetValue(_.FirstName), p.GetValue(_.Title));
                    v.Phone = p.GetValue(_.Phone);
                    v.Email = p.GetValue(_.EmailAddress);
                }
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
    }
}

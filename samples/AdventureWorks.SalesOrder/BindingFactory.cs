using DevZest.Data.Presenters;
using DevZest.Samples.AdventureWorksLT;

namespace AdventureWorks.SalesOrders
{
    public static class BindingFactory
    {
        public static RowBinding<CustomerControl> AsCustomerControl(this Customer.Key key, Customer.Lookup _)
        {
            return new RowBinding<CustomerControl>((v, p) =>
            {
                v.CustomerID = p.GetValue(key.CustomerID);
                v.CompanyName = p.GetValue(_.CompanyName);
                v.ContactPerson = GetContactPerson(p.GetValue(_.LastName), p.GetValue(_.FirstName), p.GetValue(_.Title));
                v.Phone = p.GetValue(_.Phone);
                v.Email = p.GetValue(_.EmailAddress);
            }).WithInput(new PropertyChangedTrigger<CustomerControl>(CustomerControl.CustomerIDProperty), key.CustomerID, v => v.CustomerID);
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

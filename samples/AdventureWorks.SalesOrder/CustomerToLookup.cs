using DevZest.Data;
using DevZest.Samples.AdventureWorksLT;

namespace AdventureWorks.SalesOrders
{
    public class CustomerToLookup : Customer
    {
        public Column<string> ContactPerson { get; private set; }

        protected override void OnBuilding()
        {
            base.OnBuilding();
            ContactPerson = CreateLocalColumn(LastName, FirstName, Title, GetContactPerson); 
        }

        private static string GetContactPerson(DataRow dataRow, _String lastName, _String firstName, _String title)
        {
            return GetContactPerson(lastName[dataRow], firstName[dataRow], title[dataRow]);
        }

        internal static string GetContactPerson(string lastName, string firstName, string title)
        {
            string result = string.IsNullOrEmpty(lastName) ? string.Empty : lastName.ToUpper();
            if (!string.IsNullOrEmpty(firstName))
            {
                if (result.Length > 0)
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

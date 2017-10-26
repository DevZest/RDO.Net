using DevZest.Data;
using System;

namespace DevZest.Samples.AdventureWorksLT
{
    public static class CustomerContactPerson
    {
        private sealed class Manager : AttachedLocalColumnManager<Customer, string>
        {
            public static readonly Manager Singleton = new Manager();

            private Manager()
            {
            }

            protected override Column<string> CreateLocalColumn(DataSetContainer dataSetContainer, Customer _)
            {
                return dataSetContainer.CreateLocalColumn(_, _.LastName, _.FirstName, _.Title, GetContactPerson);
            }
        }

        public static Column<string> GetContactPerson(this Customer _)
        {
            return Manager.Singleton.GetAttachedColumn(_);
        }

        public static Action<Customer> Initializer
        {
            get { return Manager.Singleton.Initializer; }
        }

        private static string GetContactPerson(DataRow dataRow, _String lastName, _String firstName, _String title)
        {
            return GetContactPerson(lastName[dataRow], firstName[dataRow], title[dataRow]);
        }

        public static string GetContactPerson(string lastName, string firstName, string title)
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

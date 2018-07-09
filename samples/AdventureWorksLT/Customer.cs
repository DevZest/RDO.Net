using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class Customer : BaseModel<Customer.PK>
    {
        [DbPrimaryKey("PK_Customer_CustomerID", Description = "Primary key (clustered) constraint")]
        public sealed class PK : PrimaryKey
        {
            public static IDataValues Const(int customerID)
            {
                return DataValues.Create(_Int32.Const(customerID));
            }

            public PK(_Int32 customerID)
                : base(customerID)
            {
            }

            public _Int32 CustomerID
            {
                get { return GetColumn<_Int32>(0); }
            }
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                RegisterColumn((Key _) => _.CustomerID, _CustomerID);
            }

            protected override PK CreatePrimaryKey()
            {
                return new PK(CustomerID);
            }

            public _Int32 CustomerID { get; private set; }
        }

        public class Ref : Ref<PK>
        {
            static Ref()
            {
                Register((Ref _) => _.CustomerID, _CustomerID);
            }

            public _Int32 CustomerID { get; private set; }

            protected override PK GetForeignKey()
            {
                return new PK(CustomerID);
            }
        }

        public class Lookup : ColumnGroup
        {
            static Lookup()
            {
                Register((Lookup _) => _.Title, _Title);
                Register((Lookup _) => _.FirstName, _FirstName);
                Register((Lookup _) => _.MiddleName, _MiddleName);
                Register((Lookup _) => _.LastName, _LastName);
                Register((Lookup _) => _.CompanyName, _CompanyName);
                Register((Lookup _) => _.EmailAddress, _EmailAddress);
                Register((Lookup _) => _.Phone, _Phone);
            }

            public _String Title { get; private set; }
            public _String FirstName { get; private set; }
            public _String MiddleName { get; private set; }
            public _String LastName { get; private set; }
            public _String CompanyName { get; private set; }
            public _String EmailAddress { get; private set; }
            public _String Phone { get; private set; }
        }

        protected static readonly Mounter<_Int32> _CustomerID = RegisterColumn((Customer _) => _.CustomerID);
        protected static readonly Mounter<_Boolean> _NameStyle = RegisterColumn((Customer _) => _.NameStyle);
        protected static readonly Mounter<_String> _Title = RegisterColumn((Customer _) => _.Title);
        protected static readonly Mounter<_String> _FirstName = RegisterColumn((Customer _) => _.FirstName);
        protected static readonly Mounter<_String> _MiddleName = RegisterColumn((Customer _) => _.MiddleName);
        protected static readonly Mounter<_String> _LastName = RegisterColumn((Customer _) => _.LastName);
        protected static readonly Mounter<_String> _Suffix = RegisterColumn((Customer _) => _.Suffix);
        protected static readonly Mounter<_String> _CompanyName = RegisterColumn((Customer _) => _.CompanyName);
        protected static readonly Mounter<_String> _SalesPerson = RegisterColumn((Customer _) => _.SalesPerson);
        protected static readonly Mounter<_String> _EmailAddress = RegisterColumn((Customer _) => _.EmailAddress);
        protected static readonly Mounter<_String> _Phone = RegisterColumn((Customer _) => _.Phone);
        protected static readonly Mounter<_String> _PasswordHash = RegisterColumn((Customer _) => _.PasswordHash);
        protected static readonly Mounter<_String> _PasswordSalt = RegisterColumn((Customer _) => _.PasswordSalt);

        static Customer()
        {
            RegisterLocalColumn((Customer _) => _.ContactPerson);
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(CustomerID);
        }

        [Identity(1, 1)]
        [DbColumn(Description = "Primary key for Customer records.")]
        public _Int32 CustomerID { get; private set; }

        [UdtNameStyle]
        [DefaultValue(false, Name = "DF_Customer_NameStyle", Description = "Default constraint value of 0")]
        [DbColumn(Description = "0 = The data in FirstName and LastName are stored in western style (first name, last name) order.  1 = Eastern style (last name, first name) order.")]
        public _Boolean NameStyle { get; private set; }

        [AsNVarChar(8)]
        [DbColumn(Description = "A courtesy title. For example, Mr. or Ms.")]
        public _String Title { get; private set; }

        [UdtName]
        [DbColumn(Description = "First name of the person.")]
        public _String FirstName { get; private set; }

        [UdtName]
        [DbColumn(Description = "Middle name or middle initial of the person.")]
        public _String MiddleName { get; private set; }

        [UdtName]
        [DbColumn(Description = "Last name of the person.")]
        public _String LastName { get; private set; }

        [AsNVarChar(10)]
        [DbColumn(Description = "Surname suffix. For example, Sr. or Jr.")]
        public _String Suffix { get; private set; }

        [AsNVarChar(128)]
        [DbColumn(Description = "The customer's organization.")]
        public _String CompanyName { get; private set; }

        [AsNVarChar(256)]
        [DbColumn(Description = "The customer's sales person, an employee of AdventureWorks Cycles.")]
        public _String SalesPerson { get; private set; }

        [AsNVarChar(256)]
        [EmailAddress]
        [DbColumn(Description = "E-mail address for the person.")]
        [DbIndex("IX_Customer_EmailAddress", Description = "Nonclustered index.")]
        public _String EmailAddress { get; private set; }

        [UdtPhone]
        [DbColumn(Description = "Phone number associated with the person.")]
        public _String Phone { get; private set; }

        [Required]
        [AsVarChar(128)]
        [DbColumn(Description = "Password for the e-mail account.")]
        public _String PasswordHash { get; private set; }

        [Required]
        [AsVarChar(10)]
        [DbColumn(Description = "Random value concatenated with the password string before the password is hashed.")]
        public _String PasswordSalt { get; private set; }

        public LocalColumn<string> ContactPerson { get; private set; }

        [Computation]
        private void ComputeContactPerson()
        {
            ContactPerson.ComputedAs(LastName, FirstName, Title, GetContactPerson, false);
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

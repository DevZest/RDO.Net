using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class Customer : BaseModel<Customer.Key>
    {
        public sealed class Key : ModelKey
        {
            public Key(_Int32 customerID)
            {
                CustomerID = customerID;
            }

            public _Int32 CustomerID { get; private set; }
        }

        public static readonly Property<_Int32> _CustomerID = RegisterColumn((Customer x) => x.CustomerID);
        public static readonly Property<_Boolean> _NameStyle = RegisterColumn((Customer x) => x.NameStyle);
        public static readonly Property<_String> _Title = RegisterColumn((Customer x) => x.Title);
        public static readonly Property<_String> _FirstName = RegisterColumn((Customer x) => x.FirstName);
        public static readonly Property<_String> _MiddleName = RegisterColumn((Customer x) => x.MiddleName);
        public static readonly Property<_String> _LastName = RegisterColumn((Customer x) => x.LastName);
        public static readonly Property<_String> _Suffix = RegisterColumn((Customer x) => x.Suffix);
        public static readonly Property<_String> _CompanyName = RegisterColumn((Customer x) => x.CompanyName);
        public static readonly Property<_String> _SalesPersion = RegisterColumn((Customer x) => x.SalesPerson);
        public static readonly Property<_String> _EmailAddress = RegisterColumn((Customer x) => x.EmailAddress);
        public static readonly Property<_String> _Phone = RegisterColumn((Customer x) => x.Phone);
        public static readonly Property<_String> _PasswordHash = RegisterColumn((Customer x) => x.PasswordHash);
        public static readonly Property<_String> _PasswordSalt = RegisterColumn((Customer x) => x.PasswordSalt);

        public Customer()
        {
            _primaryKey = new Key(CustomerID);
        }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get { return _primaryKey; }
        }

        [Identity(1, 1)]
        public _Int32 CustomerID { get; private set; }

        [UdtNameStyle]
        public _Boolean NameStyle { get; private set; }

        [AsNVarChar(8)]
        public _String Title { get; private set; }

        [UdtName]
        public _String FirstName { get; private set; }

        [UdtName]
        public _String MiddleName { get; private set; }

        [UdtName]
        public _String LastName { get; private set; }

        [AsNVarChar(10)]
        public _String Suffix { get; private set; }

        [AsNVarChar(128)]
        public _String CompanyName { get; private set; }

        [AsNVarChar(256)]
        public _String SalesPerson { get; private set; }

        [AsNVarChar(256)]
        public _String EmailAddress { get; private set; }

        [UdtPhone]
        public _String Phone { get; private set; }

        [Required]
        [AsVarChar(128)]
        public _String PasswordHash { get; private set; }

        [Required]
        [AsVarChar(10)]
        public _String PasswordSalt { get; private set; }
    }
}

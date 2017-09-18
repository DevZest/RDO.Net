using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class Customer : BaseModel<Customer.Key>
    {
        public sealed class Key : KeyBase
        {
            public Key(_Int32 customerID)
            {
                CustomerID = customerID;
            }

            public _Int32 CustomerID { get; private set; }
        }

        public class Ref : Model<Key>
        {
            public static readonly Mounter<_Int32> _CustomerID = RegisterColumn((Ref _) => _.CustomerID);

            public Ref()
            {
                _primaryKey = new Key(CustomerID);
            }

            private Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get { return _primaryKey; }
            }

            public _Int32 CustomerID { get; private set; }
        }

        public static readonly Mounter<_Int32> _CustomerID = RegisterColumn((Customer _) => _.CustomerID, Ref._CustomerID);
        public static readonly Mounter<_Boolean> _NameStyle = RegisterColumn((Customer _) => _.NameStyle);
        public static readonly Mounter<_String> _Title = RegisterColumn((Customer _) => _.Title);
        public static readonly Mounter<_String> _FirstName = RegisterColumn((Customer _) => _.FirstName);
        public static readonly Mounter<_String> _MiddleName = RegisterColumn((Customer _) => _.MiddleName);
        public static readonly Mounter<_String> _LastName = RegisterColumn((Customer _) => _.LastName);
        public static readonly Mounter<_String> _Suffix = RegisterColumn((Customer _) => _.Suffix);
        public static readonly Mounter<_String> _CompanyName = RegisterColumn((Customer _) => _.CompanyName);
        public static readonly Mounter<_String> _SalesPersion = RegisterColumn((Customer _) => _.SalesPerson);
        public static readonly Mounter<_String> _EmailAddress = RegisterColumn((Customer _) => _.EmailAddress);
        public static readonly Mounter<_String> _Phone = RegisterColumn((Customer _) => _.Phone);
        public static readonly Mounter<_String> _PasswordHash = RegisterColumn((Customer _) => _.PasswordHash);
        public static readonly Mounter<_String> _PasswordSalt = RegisterColumn((Customer _) => _.PasswordSalt);

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

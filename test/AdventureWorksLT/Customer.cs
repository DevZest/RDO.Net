using DevZest.Data;
using DevZest.Data.Annotations;
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

        public class Ref : Model<Key>
        {
            static Ref()
            {
                RegisterColumn((Ref _) => _.CustomerID, _CustomerID);
            }

            private Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get
                {
                    if (_primaryKey == null)
                        _primaryKey = new Key(CustomerID);
                    return _primaryKey;
                }
            }

            public _Int32 CustomerID { get; private set; }
        }

        public class Lookup : ModelExtension
        {
            static Lookup()
            {
                RegisterColumn((Lookup _) => _.Title, _Title);
                RegisterColumn((Lookup _) => _.FirstName, _FirstName);
                RegisterColumn((Lookup _) => _.MiddleName, _MiddleName);
                RegisterColumn((Lookup _) => _.LastName, _LastName);
                RegisterColumn((Lookup _) => _.CompanyName, _CompanyName);
                RegisterColumn((Lookup _) => _.EmailAddress, _EmailAddress);
                RegisterColumn((Lookup _) => _.Phone, _Phone);
            }

            public _String Title { get; private set; }
            public _String FirstName { get; private set; }
            public _String MiddleName { get; private set; }
            public _String LastName { get; private set; }
            public _String CompanyName { get; private set; }
            public _String EmailAddress { get; private set; }
            public _String Phone { get; private set; }
        }

        public static readonly Mounter<_Int32> _CustomerID;
        public static readonly Mounter<_Boolean> _NameStyle;
        public static readonly Mounter<_String> _Title;
        public static readonly Mounter<_String> _FirstName;
        public static readonly Mounter<_String> _MiddleName;
        public static readonly Mounter<_String> _LastName;
        public static readonly Mounter<_String> _Suffix;
        public static readonly Mounter<_String> _CompanyName;
        public static readonly Mounter<_String> _SalesPerson;
        public static readonly Mounter<_String> _EmailAddress;
        public static readonly Mounter<_String> _Phone;
        public static readonly Mounter<_String> _PasswordHash;
        public static readonly Mounter<_String> _PasswordSalt;

        static Customer()
        {
            _CustomerID = RegisterColumn((Customer _) => _.CustomerID);
            _NameStyle = RegisterColumn((Customer _) => _.NameStyle);
            _Title = RegisterColumn((Customer _) => _.Title);
            _FirstName = RegisterColumn((Customer _) => _.FirstName);
            _MiddleName = RegisterColumn((Customer _) => _.MiddleName);
            _LastName = RegisterColumn((Customer _) => _.LastName);
            _Suffix = RegisterColumn((Customer _) => _.Suffix);
            _CompanyName = RegisterColumn((Customer _) => _.CompanyName);
            _SalesPerson = RegisterColumn((Customer _) => _.SalesPerson);
            _EmailAddress = RegisterColumn((Customer _) => _.EmailAddress);
            _Phone = RegisterColumn((Customer _) => _.Phone);
            _PasswordHash = RegisterColumn((Customer _) => _.PasswordHash);
            _PasswordSalt = RegisterColumn((Customer _) => _.PasswordSalt);
        }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get
            {
                if (_primaryKey == null)
                    _primaryKey = new Key(CustomerID);
                return _primaryKey;
            }
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

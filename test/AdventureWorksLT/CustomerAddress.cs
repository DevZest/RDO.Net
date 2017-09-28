using DevZest.Data;

namespace DevZest.Samples.AdventureWorksLT
{
    public class CustomerAddress : BaseModel<CustomerAddress.Key>
    {
        public sealed class Key : KeyBase
        {
            public Key(_Int32 customerID, _Int32 addressID)
            {
                CustomerID = customerID;
                AddressID = addressID;
            }

            public _Int32 CustomerID { get; private set; }

            public _Int32 AddressID { get; private set; }
        }

        public class Ref : Model<Key>
        {
            static Ref()
            {
                RegisterColumn((Ref _) => _.CustomerID, AdventureWorksLT.Customer._CustomerID);
                RegisterColumn((Ref _) => _.AddressID, AdventureWorksLT.Address._AddressID);
            }

            private Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get
                {
                    if (_primaryKey == null)
                        _primaryKey = new Key(CustomerID, AddressID);
                    return _primaryKey;
                }
            }

            public _Int32 CustomerID { get; private set; }

            public _Int32 AddressID { get; private set; }
        }

        public class Lookup : ModelExtension
        {
            static Lookup()
            {
                RegisterColumn((Lookup _) => _.AddressType, _AddressType);
            }

            public _String AddressType { get; private set; }
        }

        public static readonly Mounter<_String> _AddressType;

        static CustomerAddress()
        {
            RegisterColumn((CustomerAddress _) => _.CustomerID, AdventureWorksLT.Customer._CustomerID);
            RegisterColumn((CustomerAddress _) => _.AddressID, AdventureWorksLT.Address._AddressID);
            _AddressType = RegisterColumn((CustomerAddress _) => _.AddressType);
        }

        private Key _primaryKey;
        public override Key PrimaryKey
        {
            get
            {
                if (_primaryKey == null)
                    _primaryKey = new Key(CustomerID, AddressID);
                return _primaryKey;
            }
        }

        private Customer.Key _customer;
        public Customer.Key Customer
        {
            get
            {
                if (_customer == null)
                    _customer = new Customer.Key(CustomerID);
                return _customer;
            }
        }

        private Address.Key _address;
        public Address.Key Address
        {
            get
            {
                if (_address == null)
                    _address = new Address.Key(AddressID);
                return _address;
            }
        }

        public _Int32 CustomerID { get; private set; }

        public _Int32 AddressID { get; private set; }

        [UdtName]
        public _String AddressType { get; private set; }
    }
}

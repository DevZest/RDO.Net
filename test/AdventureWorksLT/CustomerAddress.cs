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
            public static readonly Property<_Int32> _CustomerID = RegisterColumn((Ref _) => _.CustomerID, Customer.Ref._CustomerID);
            public static readonly Property<_Int32> _AddressID = RegisterColumn((Ref _) => _.AddressID);

            public Ref()
            {
                _primaryKey = new Key(CustomerID, AddressID);
            }

            private readonly Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get { return _primaryKey; }
            }

            public _Int32 CustomerID { get; private set; }

            public _Int32 AddressID { get; private set; }
        }

        public static readonly Property<_Int32> _CustomerID = RegisterColumn((CustomerAddress _) => _.CustomerID, Ref._CustomerID);
        public static readonly Property<_Int32> _AddressID = RegisterColumn((CustomerAddress _) => _.AddressID, Ref._AddressID);
        public static readonly Property<_String> _AddressType = RegisterColumn((CustomerAddress _) => _.AddressType);

        public CustomerAddress()
        {
            _primaryKey = new Key(CustomerID, AddressID);
            CustomerKey = new Customer.Key(CustomerID);
            AddressKey = new Address.Key(AddressID);
        }

        private readonly Key _primaryKey;
        public override Key PrimaryKey
        {
            get { return _primaryKey; }
        }

        public Customer.Key CustomerKey { get; private set; }

        public Address.Key AddressKey { get; private set; }

        public _Int32 CustomerID { get; private set; }

        public _Int32 AddressID { get; private set; }

        [UdtName]
        public _String AddressType { get; private set; }
    }
}

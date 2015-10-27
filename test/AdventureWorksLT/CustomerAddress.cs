using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class CustomerAddress : BaseModel<CustomerAddress.Key>
    {
        public sealed class Key : ModelKey
        {
            public Key(_Int32 customerID, _Int32 addressID)
            {
                CustomerID = customerID;
                AddressID = addressID;
            }

            public _Int32 CustomerID { get; private set; }

            public _Int32 AddressID { get; private set; }
        }

        public static readonly Accessor<CustomerAddress, _Int32> CustomerIDAccessor = RegisterColumn((CustomerAddress x) => x.CustomerID);
        public static readonly Accessor<CustomerAddress, _Int32> AddressIDAccessor = RegisterColumn((CustomerAddress x) => x.AddressID);
        public static readonly Accessor<CustomerAddress, _String> AddressTypeAccessor = RegisterColumn((CustomerAddress x) => x.AddressType);

        public CustomerAddress()
        {
            _primaryKey = new Key(CustomerID, AddressID);
            CustomerKey = new Customer.Key(CustomerID);
            AddressKey = new Address.Key(AddressID);
        }

        private Key _primaryKey;
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

using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class CustomerAddress : BaseModel<CustomerAddress.PK>
    {
        [DbConstraint("PK_CustomerAddress_CustomerID_AddressID", Description = "Primary key (clustered) constraint")]
        public sealed class PK : PrimaryKey
        {
            public PK(_Int32 customerID, _Int32 addressID)
            {
                CustomerID = customerID;
                AddressID = addressID;
            }

            public _Int32 CustomerID { get; private set; }

            public _Int32 AddressID { get; private set; }
        }

        public IDataValues GetKey(int customerId, int addressId)
        {
            return DataValues.Create(_Int32.Const(customerId), _Int32.Const(addressId));
        }

        public class Key : Model<PK>
        {
            static Key()
            {
                RegisterColumn((Key _) => _.CustomerID, AdventureWorksLT.Customer._CustomerID);
                RegisterColumn((Key _) => _.AddressID, AdventureWorksLT.Address._AddressID);
            }

            private PK _primaryKey;
            public sealed override PK PrimaryKey
            {
                get { return _primaryKey ?? (_primaryKey = new PK(CustomerID, AddressID)); }
            }

            public _Int32 CustomerID { get; private set; }

            public _Int32 AddressID { get; private set; }
        }

        public class Lookup : ModelExtender
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

        private PK _primaryKey;
        public override PK PrimaryKey
        {
            get { return _primaryKey ?? (_primaryKey = new PK(CustomerID, AddressID)); }
        }

        private Customer.PK _customer;
        public Customer.PK Customer
        {
            get { return _customer ?? (_customer = new Customer.PK(CustomerID)); }
        }

        private Address.PK _address;
        public Address.PK Address
        {
            get { return _address ?? (_address = new Address.PK(AddressID)); }
        }

        [DbColumn(Description = "Primary key. Foreign key to Customer.CustomerID.")]
        public _Int32 CustomerID { get; private set; }

        [DbColumn(Description = "Primary key. Foreign key to Address.AddressID.")]
        public _Int32 AddressID { get; private set; }

        [UdtName]
        [DbColumn(Description = "The kind of Address. One of: Archive, Billing, Home, Main Office, Primary, Shipping")]
        public _String AddressType { get; private set; }
    }
}

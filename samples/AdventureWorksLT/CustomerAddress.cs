using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class CustomerAddress : BaseModel<CustomerAddress.PK>
    {
        [DbPrimaryKey("PK_CustomerAddress_CustomerID_AddressID", Description = "Primary key (clustered) constraint")]
        public sealed class PK : PrimaryKey
        {
            public static IDataValues Const(int customerID, int addressID)
            {
                return DataValues.Create(_Int32.Const(customerID), _Int32.Const(addressID));
            }

            public PK(_Int32 customerID, _Int32 addressID)
                : base(customerID, addressID)
            {
            }

            public _Int32 CustomerID
            {
                get { return GetColumn<_Int32>(0); }
            }

            public _Int32 AddressID
            {
                get { return GetColumn<_Int32>(1); }
            }
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                RegisterColumn((Key _) => _.CustomerID, _CustomerID);
                RegisterColumn((Key _) => _.AddressID, _AddressID);
            }

            protected override PK CreatePrimaryKey()
            {
                return new PK(CustomerID, AddressID);
            }

            public _Int32 CustomerID { get; private set; }

            public _Int32 AddressID { get; private set; }
        }

        public class Ref : Ref<PK>
        {
            static Ref()
            {
                Register((Ref _) => _.CustomerID, _CustomerID);
                Register((Ref _) => _.AddressID, _AddressID);
            }

            public _Int32 CustomerID { get; private set; }

            public _Int32 AddressID { get; private set; }

            protected override PK GetForeignKey()
            {
                return new PK(CustomerID, AddressID);
            }
        }

        public class Lookup : Projection
        {
            static Lookup()
            {
                Register((Lookup _) => _.AddressType, _AddressType);
            }

            public _String AddressType { get; private set; }
        }

        public static readonly Mounter<_Int32> _CustomerID = RegisterColumn((CustomerAddress _) => _.CustomerID);
        public static readonly Mounter<_Int32> _AddressID = RegisterColumn((CustomerAddress _) => _.AddressID);
        public static readonly Mounter<_String> _AddressType = RegisterColumn((CustomerAddress _) => _.AddressType);

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(CustomerID, AddressID);
        }

        private Customer.PK _fk_customer;
        public Customer.PK FK_Customer
        {
            get { return _fk_customer ?? (_fk_customer = new Customer.PK(CustomerID)); }
        }

        private Address.PK _fk_address;
        public Address.PK FK_Address
        {
            get { return _fk_address ?? (_fk_address = new Address.PK(AddressID)); }
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

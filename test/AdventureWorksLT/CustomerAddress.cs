using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class CustomerAddress : BaseModel<CustomerAddress.PK>
    {
        [DbPrimaryKey("PK_CustomerAddress_CustomerID_AddressID", Description = "Primary key (clustered) constraint")]
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

        public sealed class Key : Model<PK>
        {
            static Key()
            {
                RegisterColumn((Key _) => _.CustomerID, Customer._CustomerID);
                RegisterColumn((Key _) => _.AddressID, Address._AddressID);
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
                RegisterColumn((Ref _) => _.CustomerID, Customer._CustomerID);
                RegisterColumn((Ref _) => _.AddressID, Address._AddressID);
            }

            public _Int32 CustomerID { get; private set; }

            public _Int32 AddressID { get; private set; }

            protected override PK CreatePrimaryKey()
            {
                return new PK(CustomerID, AddressID);
            }
        }

        public class Lookup : Lookup<PK>
        {
            static Lookup()
            {
                RegisterColumn((Lookup _) => _.AddressType, _AddressType);
            }

            public _String AddressType { get; private set; }
        }

        public sealed class FK : ForeignKey<PK>
        {
            static FK()
            {
                RegisterChildContainer((FK _) => _.FK_Customer);
                RegisterChildContainer((FK _) => _.FK_Address);
            }

            public Customer.Ref FK_Customer { get; private set; }
            public Address.Ref FK_Address { get; private set; }

            public sealed class Ext : ColumnContainer
            {
                static Ext()
                {
                    RegisterChildContainer((Ext _) => _.Customer);
                    RegisterChildContainer((Ext _) => _.Address);
                }

                public Customer.Lookup Customer { get; private set; }
                public Address.Lookup Address { get; private set; }
            }
        }

        public static readonly Mounter<_String> _AddressType;

        static CustomerAddress()
        {
            RegisterColumn((CustomerAddress _) => _.CustomerID, AdventureWorksLT.Customer._CustomerID);
            RegisterColumn((CustomerAddress _) => _.AddressID, AdventureWorksLT.Address._AddressID);
            _AddressType = RegisterColumn((CustomerAddress _) => _.AddressType);
        }

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

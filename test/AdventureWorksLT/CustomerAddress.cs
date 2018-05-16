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

        public class PK_ : Model<PK>
        {
            static PK_()
            {
                RegisterColumn((PK_ _) => _.CustomerID, AdventureWorksLT.Customer._CustomerID);
                RegisterColumn((PK_ _) => _.AddressID, AdventureWorksLT.Address._AddressID);
            }

            private PK _primaryKey;
            public sealed override PK PrimaryKey
            {
                get { return _primaryKey ?? (_primaryKey = new PK(CustomerID, AddressID)); }
            }

            public _Int32 CustomerID { get; private set; }

            public _Int32 AddressID { get; private set; }
        }

        public class Lookup : ColumnContainer
        {
            static Lookup()
            {
                RegisterColumn((Lookup _) => _.AddressType, _AddressType);
            }

            public _String AddressType { get; private set; }
        }

        public class ForeignKey : Model
        {
            static ForeignKey()
            {
                RegisterColumn((ForeignKey _) => _.CustomerID, AdventureWorksLT.Customer._CustomerID);
                RegisterColumn((ForeignKey _) => _.AddressID, AdventureWorksLT.Address._AddressID);
            }

            public _Int32 CustomerID { get; private set; }
            public _Int32 AddressID { get; private set; }

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
        }

        [ExtraColumns(typeof(Ext))]
        public class ForeignKeyLookup : Model
        {
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

            public Customer.Lookup Customer
            {
                get { return GetExtraColumns<Ext>().Customer; }
            }

            public Address.Lookup Address
            {
                get { return GetExtraColumns<Ext>().Address; }
            }
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

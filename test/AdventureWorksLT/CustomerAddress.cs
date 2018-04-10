﻿using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class CustomerAddress : BaseModel<CustomerAddress.Key>
    {
        [DbConstraint("PK_CustomerAddress_CustomerID_AddressID", Description = "Primary key (clustered) constraint")]
        public sealed class Key : PrimaryKey
        {
            public Key(_Int32 customerID, _Int32 addressID)
            {
                CustomerID = customerID;
                AddressID = addressID;
            }

            public _Int32 CustomerID { get; private set; }

            public _Int32 AddressID { get; private set; }
        }

        public IDataValues GetValueRef(int customerId, int addressId)
        {
            return DataValues.Create(_Int32.Const(customerId), _Int32.Const(addressId));
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
                get { return _primaryKey ?? (_primaryKey = new Key(CustomerID, AddressID)); }
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

        private Key _primaryKey;
        public override Key PrimaryKey
        {
            get { return _primaryKey ?? (_primaryKey = new Key(CustomerID, AddressID)); }
        }

        private Customer.Key _customer;
        public Customer.Key Customer
        {
            get { return _customer ?? (_customer = new Customer.Key(CustomerID)); }
        }

        private Address.Key _address;
        public Address.Key Address
        {
            get { return _address ?? (_address = new Address.Key(AddressID)); }
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

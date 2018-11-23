using DevZest.Data.Addons;
using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class ForeignKeyAttributeTests
    {
        private sealed class Address : Model<Address.PK>
        {
            public sealed class PK : PrimaryKey
            {
                public PK(_Int32 addressId)
                    : base(addressId)
                {
                }
            }

            protected override PK CreatePrimaryKey()
            {
                return new PK(AddressId);
            }

            public static readonly Mounter<_Int32> _AddressId = RegisterColumn((Address _) => _.AddressId);

            public _Int32 AddressId { get; private set; }
        }

        private sealed class Customer : Model<Customer.PK>
        {
            public sealed class PK : PrimaryKey
            {
                public PK(_Int32 customerId)
                    : base(customerId)
                {
                }
            }

            protected override PK CreatePrimaryKey()
            {
                return new PK(CustomerId);
            }

            public static readonly Mounter<_Int32> _CustomerId = RegisterColumn((Customer _) => _.CustomerId);
            public static readonly Mounter<_Int32> _AddressId = RegisterColumn((Customer _) => _.AddressId);

            public _Int32 CustomerId { get; private set; }

            public _Int32 AddressId { get; private set; }

            private Address.PK _fk_customer_address;
            public Address.PK FK_Customer_Address
            {
                get { return _fk_customer_address ?? (_fk_customer_address = new Address.PK(AddressId)); }
            }
        }

        private sealed class Db : SqlSession
        {
            public Db()
                : base(new SqlConnection())
            {
            }

            private DbTable<Address> _address;
            [DbTable("dbo.Address", Description = "Description of Address table.")]
            public DbTable<Address> Address
            {
                get { return GetTable(ref _address); }
            }

            private DbTable<Customer> _customer;
            [DbTable("dbo.Customer", Description = "Description of Customer table.")]
            [ForeignKey(nameof(FK_Customer_Address), Description = "Description of foreign key.")]
            public DbTable<Customer> Customer
            {
                get { return GetTable(ref _customer); }
            }

            [_ForeignKey]
            private KeyMapping FK_Customer_Address(Customer _)
            {
                return _.FK_Customer_Address.Join(Address._);
            }
        }

        [TestMethod]
        public void ForeignKeyAttribute()
        {
            using (var db = new Db())
            {
                var _ = db.Customer._;
                var fkConstraint = _.GetAddon<DbForeignKeyConstraint>();
                Assert.AreEqual(_.FK_Customer_Address, fkConstraint.ForeignKey);
                Assert.AreEqual(db.Address._.PrimaryKey, fkConstraint.ReferencedKey);
            }
        }
    }
}

using DevZest.Data.SqlServer;
using System.Data.SqlClient;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public abstract class RelationshipDiagnosticsBase : SqlSession
    {
        public sealed class Address : Model<Address.PK>
        {
            public sealed class PK : CandidateKey
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

        public sealed class Customer : Model<Customer.PK>
        {
            public sealed class PK : CandidateKey
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

            private Address.PK _fk_Address;
            public Address.PK FK_Address
            {
                get { return _fk_Address ?? (_fk_Address = new Address.PK(AddressId)); }
            }
        }

        protected RelationshipDiagnosticsBase(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }
    }
}

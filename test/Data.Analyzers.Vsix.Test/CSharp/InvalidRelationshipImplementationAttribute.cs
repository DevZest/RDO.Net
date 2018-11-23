using DevZest.Data.Annotations;
using System.Data.SqlClient;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class InvalidRelationshipImplementationAttribute : RelationshipDiagnosticsBase
    {
        public InvalidRelationshipImplementationAttribute(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        private DbTable<Address> _addresses;
        public DbTable<Address> Addresses
        {
            get { return GetTable(ref _addresses); }
        }

        private DbTable<Customer> _customers;
        [Relationship(nameof(FK_Customer_Address))]
        public DbTable<Customer> Customers
        {
            get { return GetTable(ref _customers); }
        }

        [_Relationship]
        private KeyMapping FK_Customer_Address(Customer _)
        {
            return _.FK_Address.Join(Addresses._);
        }

        [_Relationship]
        private void FK_Customer_Address()
        {
        }
    }
}

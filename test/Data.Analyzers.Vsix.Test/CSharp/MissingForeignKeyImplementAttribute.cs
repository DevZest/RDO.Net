using DevZest.Data.Annotations;
using System.Data.SqlClient;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MissingForeignKeyImplementationAttribute : ForeignKeyDiagnosticsBase
    {
        public MissingForeignKeyImplementationAttribute(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        private DbTable<Address> _addresses;
        public DbTable<Address> Addresses
        {
            get { return GetTable(ref _addresses); }
        }

        private DbTable<Customer> _customers;
        [ForeignKey(nameof(FK_Customer_Address))]
        public DbTable<Customer> Customers
        {
            get { return GetTable(ref _customers); }
        }

        private KeyMapping FK_Customer_Address(Customer _)
        {
            return _.FK_Address.Join(Addresses._);
        }
    }
}

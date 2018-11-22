using DevZest.Data.Annotations;
using System.Data.SqlClient;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MissingForeignKeyImplementation : ForeignKeyDiagnosticsBase
    {
        public MissingForeignKeyImplementation(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        private DbTable<Address> _addresses;
        public DbTable<Address> Addresses
        {
            get { return GetTable(ref _addresses); }
        }

        private DbTable<Customer> _customers;
        [ForeignKey("FK_Customer_Address")]
        public DbTable<Customer> Customers
        {
            get { return GetTable(ref _customers); }
        }
    }
}

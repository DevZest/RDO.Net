using System.Collections.Generic;
using DevZest.Data.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public class ForeignKeyAnalyzerTests : DiagnosticVerifier
    {
        private static readonly MetadataReference SqlServerReference = MetadataReference.CreateFromFile(typeof(DevZest.Data.SqlServer.SqlSession).Assembly.Location);

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ForeignKeyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new ForeignKeyAnalyzer();
        }

        protected override IEnumerable<MetadataReference> AdditionalReferences
        {
            get { yield return SqlServerReference; }
        }

        [TestMethod]
        public void MissingDeclaraionAttribute_CS()
        {
            var test =
@"using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;
using System.Data.SqlClient;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MissingForeignKeyDeclaration : SqlSession
    {
        public sealed class Address : Model<Address.PK>
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

        public sealed class Customer : Model<Customer.PK>
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

            private Address.PK _fk_Customer_Address;
            public Address.PK FK_Customer_Address
            {
                get { return _fk_Customer_Address ?? (_fk_Customer_Address = new Address.PK(AddressId)); }
            }
        }

        public MissingForeignKeyDeclaration(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        private DbTable<Address> _addresses;
        public DbTable<Address> Addresses
        {
            get { return GetTable(ref _addresses); }
        }

        private DbTable<Customer> _customers;
        public DbTable<Customer> Customers
        {
            get { return GetTable(ref _customers); }
        }

        [_ForeignKey]
        private KeyMapping FK_Customer_Address(Customer _)
        {
            return _.FK_Customer_Address.Join(Addresses._);
        }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MissingDeclarationAttribute,
                Message = string.Format(Resources.MissingDeclarationAttribute_Message, typeof(ForeignKeyAttribute), "FK_Customer_Address"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 74, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
    }
}

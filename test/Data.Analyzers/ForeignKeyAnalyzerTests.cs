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
    public class Db : SqlSession
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

        public Db(SqlConnection sqlConnection)
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

        [TestMethod]
        public void InvalidImplementationAttribute_CS()
        {
            var test =
@"using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;
using System.Data.SqlClient;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class Db : SqlSession
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

        public Db(SqlConnection sqlConnection)
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

        [_ForeignKey]
        private KeyMapping FK_Customer_Address(Customer _)
        {
            return _.FK_Customer_Address.Join(Addresses._);
        }

        [_ForeignKey]
        private void FK_Customer_Address()
        {
        }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.InvalidImplementationAttribute,
                Message = string.Format(Resources.InvalidImplementationAttribute_Message, typeof(_ForeignKeyAttribute), Resources.StringFormatArg_Method, typeof(KeyMapping), "DevZest.Data.Analyzers.Vsix.Test.CSharp.Db.Customer"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 81, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void DuplicateDeclarationAttribute_CS()
        {
            var test =
@"using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;
using System.Data.SqlClient;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class Db : SqlSession
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

        public Db(SqlConnection sqlConnection)
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
        [ForeignKey(nameof(FK_Customer_Address))]
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
                Id = DiagnosticIds.DuplicateDeclarationAttribute,
                Message = string.Format(Resources.DuplicateDeclarationAttribute_Message, typeof(ForeignKeyAttribute), "FK_Customer_Address"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 70, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MissingImplementation_CS()
        {
            var test =
@"using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;
using System.Data.SqlClient;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class Db : SqlSession
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

        public Db(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        private DbTable<Address> _addresses;
        public DbTable<Address> Addresses
        {
            get { return GetTable(ref _addresses); }
        }

        private DbTable<Customer> _customers;
        [ForeignKey(""FK_Customer_Address"")]
        public DbTable<Customer> Customers
        {
            get { return GetTable(ref _customers); }
        }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MissingImplementation,
                Message = string.Format(Resources.MissingImplementation_Message, Resources.StringFormatArg_Method, "FK_Customer_Address", typeof(KeyMapping), "DevZest.Data.Analyzers.Vsix.Test.CSharp.Db.Customer"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 69, 10) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MissingImplementationAttribute_CS()
        {
            var test =
@"using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;
using System.Data.SqlClient;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class Db : SqlSession
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

        public Db(SqlConnection sqlConnection)
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
            return _.FK_Customer_Address.Join(Addresses._);
        }
    }
}
";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MissingImplementationAttribute,
                Message = string.Format(Resources.MissingImplementationAttribute_Message, typeof(_ForeignKeyAttribute)),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 75, 28) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MissingDeclaraionAttribute_VB()
        {
            var test =
@"Imports System.Data.SqlClient
Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer

Public Class Db
    Inherits SqlSession

    Public NotInheritable Class Address
        Inherits Model(Of PK)

        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Sub New(addressId As _Int32)
                MyBase.New(addressId)
            End Sub
        End Class

        Protected Overrides Function CreatePrimaryKey() As PK
            Return New PK(AddressId)
        End Function

        Public Shared ReadOnly _AddressId As Mounter(Of _Int32) = RegisterColumn(Function(x As Address) x.AddressId)

        Private m_AddressId As _Int32
        Public Property AddressId As _Int32
            Get
                Return m_AddressId
            End Get
            Private Set
                m_AddressId = Value
            End Set
        End Property
    End Class

    Public NotInheritable Class Customer
        Inherits Model(Of PK)

        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Sub New(customerId As _Int32)
                MyBase.New(customerId)
            End Sub
        End Class

        Protected Overrides Function CreatePrimaryKey() As PK
            Return New PK(CustomerId)
        End Function

        Public Shared ReadOnly _CustomerId As Mounter(Of _Int32) = RegisterColumn(Function(x As Customer) x.CustomerId)
        Public Shared ReadOnly _AddressId As Mounter(Of _Int32) = RegisterColumn(Function(x As Customer) x.AddressId)

        Private m_CustomerId As _Int32
        Public Property CustomerId As _Int32
            Get
                Return m_CustomerId
            End Get
            Private Set
                m_CustomerId = Value
            End Set
        End Property

        Private m_AddressId As _Int32
        Public Property AddressId As _Int32
            Get
                Return m_AddressId
            End Get
            Private Set
                m_AddressId = Value
            End Set
        End Property

        Private m_FK_Address As Address.PK
        Public ReadOnly Property FK_Address As Address.PK
            Get
                If m_FK_Address Is Nothing Then
                    m_FK_Address = New Address.PK(AddressId)
                End If
                Return m_FK_Address
            End Get
        End Property
    End Class

    Protected Sub New(sqlConnection As SqlConnection)
        MyBase.New(sqlConnection)
    End Sub

    Private m_Addresses As DbTable(Of Address)
    Public ReadOnly Property Addresses As DbTable(Of Address)
        Get
            Return GetTable(m_Addresses)
        End Get
    End Property

    Private m_Customers As DbTable(Of Customer)
    Public ReadOnly Property Customers As DbTable(Of Customer)
        Get
            Return GetTable(m_Customers)
        End Get
    End Property

    <_ForeignKey>
    Private Function FK_Customer_Address(x As Customer) As KeyMapping
        Return x.FK_Address.Join(ModelOf(Addresses))
    End Function
End Class

";

            var expected = new DiagnosticResult
            {
                Id = DiagnosticIds.MissingDeclarationAttribute,
                Message = string.Format(Resources.MissingDeclarationAttribute_Message, typeof(ForeignKeyAttribute), "FK_Customer_Address"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.vb", 104, 6) }
            };

            VerifyBasicDiagnostic(test, expected);
        }
    }

    // Not necessary to repeat other VB diagnostic tests because ForeignKeyAnalyzer is language agnostic.
}

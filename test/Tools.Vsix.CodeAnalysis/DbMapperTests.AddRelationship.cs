using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    partial class DbMapperTests
    {
        [TestMethod]
        public void DbMapperTests_AddRelationship_CS()
        {
            var src =
@"using DevZest.Data;
using DevZest.Data.SqlServer;
using System.Data.SqlClient;

namespace Test
{
    public class Db : SqlSession
    {
        public Db(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        private DbTable<Address> _address;
        public DbTable<Address> Address
        {
            get
            {
                return GetTable(ref _address);
            }
        }

        private DbTable<Customer> _customer;
        public DbTable<Customer> Customer
        {
            get
            {
                return GetTable(ref _address);
            }
        }
    }

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

        private Address.FK _fk_Address;
        public Address.PK FK_Address
        {
            get { return _fk_address ?? (_fk_Address = new Address.PK(AddressId)); }
        }
    }
}
";

            var document = src.CreateDocument(SqlReference);
            var mapper = DbMapper.Refresh(null, document, new TextSpan(160, 0));
            var dbTable = mapper.DbType.GetMembers("Customer").OfType<IPropertySymbol>().Single();
            var foreignKey = mapper.Compilation.GetTypeByMetadataName("Test.Customer").GetMembers("FK_Address").OfType<IPropertySymbol>().Single();
            var refTable = mapper.DbType.GetMembers("Address").OfType<IPropertySymbol>().Single();
            var result = mapper.AddRelationship(dbTable, "FK_Customer_Address", foreignKey, refTable, "Description of FK_Customer_Address.", ForeignKeyRule.Cascade, ForeignKeyRule.SetNull);
            var actual = result.GetTextAsync().Result.ToString();
            var expected =
@"using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;
using System.Data.SqlClient;

namespace Test
{
    public class Db : SqlSession
    {
        public Db(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
        }

        private DbTable<Address> _address;
        public DbTable<Address> Address
        {
            get
            {
                return GetTable(ref _address);
            }
        }

        private DbTable<Customer> _customer;
        [Relationship(nameof(FK_Customer_Address), Description = ""Description of FK_Customer_Address."", DeleteRule = ForeignKeyRule.Cascade, UpdateRule = ForeignKeyRule.SetNull)]
        public DbTable<Customer> Customer
        {
            get
            {
                return GetTable(ref _address);
            }
        }

        [_Relationship]
        private KeyMapping FK_Customer_Address(Customer _)
        {
            return _.FK_Address.Join(Address._);
        }
    }

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

        private Address.FK _fk_Address;
        public Address.PK FK_Address
        {
            get { return _fk_address ?? (_fk_Address = new Address.PK(AddressId)); }
        }
    }
}
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DbMapperTests_AddRelationship_VB()
        {
            var src =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer
Imports System.Data.SqlClient

Public Class Db
    Inherits SqlSession

    Protected Sub New(sqlConnection As SqlConnection)
        MyBase.New(sqlConnection)
    End Sub

    Private m_Address As DbTable(Of Address)
    Public ReadOnly Property Address As DbTable(Of Address)
        Get
            Return GetTable(m_Address)
        End Get
    End Property

    Private m_Customer As DbTable(Of Customer)
    Public ReadOnly Property Customer As DbTable(Of Customer)
        Get
            Return GetTable(m_Customer)
        End Get
    End Property
End Class

Public NotInheritable Class Address
    Inherits Model(Of PK)

    Public NotInheritable Class PK
        Inherits CandidateKey

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
        Inherits CandidateKey

        Public Sub New(customerId As _Int32)
            MyBase.New(customerId)
        End Sub
    End Class

    Protected Overrides Function CreatePrimaryKey() As PK
        Return New PK(CustomerId)
    End Function

    Public Shared ReadOnly _CustomerId As Mounter(Of _Int32) = RegisterColumn(Function(x As Customer) x.CustomerId)
    Public Shared ReadOnly _AddressId As Mounter(Of _Int32) = RegisterColumn(Function(x As Customer) x.AddressId)

    Private m_customerId As _Int32
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
                m_FK_Address = New Address.PK(AddressID)
            End If
            Return m_FK_Address
        End Get
    End Property
End Class
";

            var document = src.CreateDocument(SqlReference, LanguageNames.VisualBasic);
            var mapper = DbMapper.Refresh(null, document, new TextSpan(160, 0));
            var dbTable = mapper.DbType.GetMembers("Customer").OfType<IPropertySymbol>().Single();
            var foreignKey = mapper.Compilation.GetTypeByMetadataName("Customer").GetMembers("FK_Address").OfType<IPropertySymbol>().Single();
            var refTable = mapper.DbType.GetMembers("Address").OfType<IPropertySymbol>().Single();
            var result = mapper.AddRelationship(dbTable, "FK_Customer_Address", foreignKey, refTable, "Description of FK_Customer_Address.", ForeignKeyRule.Cascade, ForeignKeyRule.SetNull);
            var actual = result.GetTextAsync().Result.ToString();
            var expected =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer
Imports System.Data.SqlClient

Public Class Db
    Inherits SqlSession

    Protected Sub New(sqlConnection As SqlConnection)
        MyBase.New(sqlConnection)
    End Sub

    Private m_Address As DbTable(Of Address)
    Public ReadOnly Property Address As DbTable(Of Address)
        Get
            Return GetTable(m_Address)
        End Get
    End Property

    Private m_Customer As DbTable(Of Customer)
    <Relationship(NameOf(FK_Customer_Address), Description:=""Description of FK_Customer_Address."", DeleteRule:=ForeignKeyRule.Cascade, UpdateRule:=ForeignKeyRule.SetNull)>
    Public ReadOnly Property Customer As DbTable(Of Customer)
        Get
            Return GetTable(m_Customer)
        End Get
    End Property

    <_Relationship>
    Private Function FK_Customer_Address(x As Customer) As KeyMapping
        Return x.FK_Address.Join(Address.Entity)
    End Function
End Class

Public NotInheritable Class Address
    Inherits Model(Of PK)

    Public NotInheritable Class PK
        Inherits CandidateKey

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
        Inherits CandidateKey

        Public Sub New(customerId As _Int32)
            MyBase.New(customerId)
        End Sub
    End Class

    Protected Overrides Function CreatePrimaryKey() As PK
        Return New PK(CustomerId)
    End Function

    Public Shared ReadOnly _CustomerId As Mounter(Of _Int32) = RegisterColumn(Function(x As Customer) x.CustomerId)
    Public Shared ReadOnly _AddressId As Mounter(Of _Int32) = RegisterColumn(Function(x As Customer) x.AddressId)

    Private m_customerId As _Int32
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
                m_FK_Address = New Address.PK(AddressID)
            End If
            Return m_FK_Address
        End Get
    End Property
End Class
";
            Assert.AreEqual(expected, actual);
        }
    }
}

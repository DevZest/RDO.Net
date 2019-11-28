using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis
{
    partial class DbMapperTests
    {
        [TestMethod]
        public void DbMapperTests_AddDbTable_CS()
        {
            var src =
@"using DevZest.Data.Annotations;
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
}
";

            var document = src.CreateDocument(SqlReference);
            var mapper = DbMapper.Refresh(null, document, new TextSpan(160, 0));
            var modelType = mapper.Project.GetCompilationAsync().Result.GetTypeByMetadataName("Test.Address");
            var result = mapper.AddDbTable(modelType, "Address", "dbo.Address", "Description of dbo.Address table.");
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
        [DbTable(""dbo.Address"", Description = ""Description of dbo.Address table."")]
        public DbTable<Address> Address
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
}
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DbMapperTests_AddDbTable_VB()
        {
            var src =
@"Imports System.Data.SqlClient
Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer

Public Class Db
    Inherits SqlSession

    Protected Sub New(sqlConnection As SqlConnection)
        MyBase.New(sqlConnection)
    End Sub
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
";

            var document = src.CreateDocument(SqlReference, LanguageNames.VisualBasic);
            var mapper = DbMapper.Refresh(null, document, new TextSpan(160, 0));
            var modelType = mapper.Project.GetCompilationAsync().Result.GetTypeByMetadataName("Address");
            var result = mapper.AddDbTable(modelType, "Address", "dbo.Address", "Description of dbo.Address table.");
            var actual = result.GetTextAsync().Result.ToString();
            var expected =
@"Imports System.Data.SqlClient
Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer

Public Class Db
    Inherits SqlSession

    Protected Sub New(sqlConnection As SqlConnection)
        MyBase.New(sqlConnection)
    End Sub

    Private m_Address As DbTable(Of Address)
    <DbTable(""dbo.Address"", Description:=""Description of dbo.Address table."")>
    Public ReadOnly Property Address As DbTable(Of Address)
        Get
            Return GetTable(m_Address)
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
";
            Assert.AreEqual(expected, actual);
        }
    }
}

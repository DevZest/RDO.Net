using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapperTests
    {
        [TestMethod]
        public void ModelMapper_AddUniqueConstraint_CS()
        {
            var src =
@"using DevZest.Data;
using DevZest.Data.Annotations;

[assembly: MessageResource(typeof(Test.Resources.Messages))]

namespace Test
{
    public class MyModel : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}

namespace Test.Resources
{
    public static class Messages
    {
        public static string AK_MyModel_ID
        {
            get { return ""Alternative key for MyModel.""; }
        }
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(160, 0));
            var entries = mapper.CreateIndexEntries();
            var dataRow = entries.AddRow();
            var _ = entries._;
            _.Column[dataRow] = mapper.GetColumns().Single();
            _.SortDirection[dataRow] = Data.SortDirection.Descending;
            var messageResourceType = mapper.GetMessageResourceType();
            var messageResourceProperty = messageResourceType.GetMembers("AK_MyModel_ID").OfType<IPropertySymbol>().Single();
            document = mapper.AddUniqueConstraint("AK_MyModel_ID", "Description", "DBAK_MyModel_ID", messageResourceType, messageResourceProperty, null, entries);
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"using DevZest.Data;
using DevZest.Data.Annotations;
using Test.Resources;

[assembly: MessageResource(typeof(Test.Resources.Messages))]

namespace Test
{
    [UniqueConstraint(nameof(AK_MyModel_ID), typeof(Messages), nameof(Messages.AK_MyModel_ID), Description = ""Description"", DbName = ""DBAK_MyModel_ID"")]
    public class MyModel : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }

        [_UniqueConstraint]
        private ColumnSort[] AK_MyModel_ID
        {
            get
            {
                return new ColumnSort[] { ID.Desc() };
            }
        }
    }
}

namespace Test.Resources
{
    public static class Messages
    {
        public static string AK_MyModel_ID
        {
            get { return ""Alternative key for MyModel.""; }
        }
    }
}
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ModelMapper_AddUniqueConstraint_VB()
        {
            var src =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

<assembly: MessageResource(GetType(Test.Resources.Messages))>

Namespace Test
    Public Class MyModel
        Inherits Model

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        <DbColumn()>
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class
End Namespace

Namespace Test.Resources
    Public Shared Class Messages
        Public Shared ReadOnly Property AK_MyModel_ID As String
            Get
                Return ""Alternative key for MyModel.""
            End Get
        End Property
    End Class
End Namespace
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(160, 0));
            var entries = mapper.CreateIndexEntries();
            var dataRow = entries.AddRow();
            var _ = entries._;
            _.Column[dataRow] = mapper.GetColumns().Single();
            _.SortDirection[dataRow] = Data.SortDirection.Descending;
            var messageResourceType = mapper.GetMessageResourceType();
            var messageResourceProperty = messageResourceType.GetMembers("AK_MyModel_ID").OfType<IPropertySymbol>().Single();
            document = mapper.AddUniqueConstraint("AK_MyModel_ID", "Description", "DBAK_MyModel_ID", messageResourceType, messageResourceProperty, null, entries);
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports Test.Resources

<assembly: MessageResource(GetType(Test.Resources.Messages))>

Namespace Test
    <UniqueConstraint(""AK_MyModel_ID"", GetType(Messages), NameOf(Messages.AK_MyModel_ID), Description:=""Description"", DbName:=""DBAK_MyModel_ID"")>
    Public Class MyModel
        Inherits Model

        Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As MyModel) x.ID)

        Private m_ID As _Int32
        <DbColumn()>
        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property

        <_UniqueConstraint>
        Private ReadOnly Property AK_MyModel_ID As ColumnSort()
            Get
                Return New ColumnSort() {ID.Desc()}
            End Get
        End Property
    End Class
End Namespace

Namespace Test.Resources
    Public Shared Class Messages
        Public Shared ReadOnly Property AK_MyModel_ID As String
            Get
                Return ""Alternative key for MyModel.""
            End Get
        End Property
    End Class
End Namespace
";
            Assert.AreEqual(expected, actual);
        }
    }
}

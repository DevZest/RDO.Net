using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapperTests
    {
        [TestMethod]
        public void ModelMapper_AddIndex_CS()
        {
            var src =
@"using DevZest.Data;

namespace Test
{
    public class MyModel : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }
    }
}
";

            var document = src.CreateDocument();
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            var entries = mapper.CreateIndexEntries();
            var dataRow = entries.AddRow();
            var _ = entries._;
            _.Column[dataRow] = mapper.GetColumns().Single();
            _.SortDirection[dataRow] = Data.SortDirection.Descending;
            document = mapper.AddIndex("IX_MyModel_ID", "Description", "DBIX_MyModel_ID", true, false, true, entries);
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"using DevZest.Data;
using DevZest.Data.Annotations;

namespace Test
{
    [DbIndex(nameof(IX_MyModel_ID), Description = ""Description"", DbName = ""DBIX_MyModel_ID"", IsUnique = true, IsValidOnTable = false, IsValidOnTempTable = true)]
    public class MyModel : Model
    {
        public static readonly Mounter<_Int32> _ID = RegisterColumn((MyModel _) => _.ID);

        public _Int32 ID { get; private set; }

        [_DbIndex]
        private ColumnSort[] IX_MyModel_ID
        {
            get
            {
                return new ColumnSort[] { ID.Desc() };
            }
        }
    }
}
";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ModelMapper_AddIndex_VB()
        {
            var src =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

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
";

            var document = src.CreateDocument(LanguageNames.VisualBasic);
            var mapper = ModelMapper.Refresh(null, document, new TextSpan(0, 0));
            var entries = mapper.CreateIndexEntries();
            var dataRow = entries.AddRow();
            var _ = entries._;
            _.Column[dataRow] = mapper.GetColumns().Single();
            _.SortDirection[dataRow] = Data.SortDirection.Descending;
            document = mapper.AddIndex("IX_MyModel_ID", "Description", "DBIX_MyModel_ID", true, false, true, entries);
            var actual = document.GetTextAsync().Result.ToString();
            var expected =
@"Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace Test
    <DbIndex(""IX_MyModel_ID"", Description:=""Description"", DbName:=""DBIX_MyModel_ID"", IsUnique:=True, IsValidOnTable:=False, IsValidOnTempTable:=True)>
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

        <_DbIndex>
        Private ReadOnly Property IX_MyModel_ID As ColumnSort()
            Get
                Return New ColumnSort() {ID.Desc()}
            End Get
        End Property
    End Class
End Namespace
";
            Assert.AreEqual(expected, actual);
        }
    }
}

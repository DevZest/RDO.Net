using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.CodeAnalysis
{
    [TestClass]
    public partial class DbMapperTests
    {
        private static readonly MetadataReference SqlReference = MetadataReference.CreateFromFile(typeof(SqlServer.SqlSession).Assembly.Location);

        [TestMethod]
        public void DbMapper_Refresh_CS()
        {
            var src = string.Empty;
            var document = src.CreateDocument(SqlReference);
            var mapper = DbMapper.Refresh(null, document, new TextSpan(0, 0));
            Assert.IsNull(mapper);

            src =
@"using DevZest.Data;
using DevZest.Data.SqlServer;

namespace Test
{
    public class Db : SqlSession
    {
    }
}
";

            document = src.CreateDocument(SqlReference);
            mapper = DbMapper.Refresh(null, document, new TextSpan(0, 0));
            Assert.IsNotNull(mapper);
            Assert.AreEqual(document, mapper.Document);
        }

        [TestMethod]
        public void DbMapper_Refresh_VB()
        {
            var src = string.Empty;
            var document = src.CreateDocument(SqlReference, LanguageNames.VisualBasic);
            var mapper = DbMapper.Refresh(null, document, new TextSpan(0, 0));
            Assert.IsNull(mapper);

            src =
@"Imports DevZest.Data
Imports DevZest.Data.SqlServer

Namespace Test
    Public Class Db
        Inherits SqlSession
    End Class
End Namespace
";

            document = src.CreateDocument(SqlReference, LanguageNames.VisualBasic);
            mapper = DbMapper.Refresh(null, document, new TextSpan(0, 0));
            Assert.IsNotNull(mapper);
            Assert.AreEqual(document, mapper.Document);
        }
    }
}

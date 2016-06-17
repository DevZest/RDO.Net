using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using DevZest.Data.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlTypes;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class _SqlXmlTests : ColumnConverterTestsBase
    {
        [TestMethod]
        public void _SqlXml_Param()
        {
            TestParam(_SqlXml.CreateSqlXml("<a></a>"));
            TestParam(null);
        }

        private void TestParam(SqlXml x)
        {
            var column = _SqlXml.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _SqlXml_Implicit()
        {
            TestImplicit(_SqlXml.CreateSqlXml("<a></a>"));
            TestImplicit(null);
        }

        private void TestImplicit(SqlXml x)
        {
            _SqlXml column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _SqlXml_Const()
        {
            TestConst(_SqlXml.CreateSqlXml("<a></a>"));
            TestConst(null);
        }

        private void TestConst(SqlXml x)
        {
            _SqlXml column = _SqlXml.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _SqlXml_FromString()
        {
            var x = _SqlXml.CreateSqlXml("<a></a>");
            TestFromString("<a></a>", x);
            TestFromString(null, null);
        }

        private void TestFromString(string x, SqlXml expectedValue)
        {
            _String column1 = x;
            _SqlXml expr = (_SqlXml)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(string), typeof(SqlXml));
            var result = expr.Eval();
            Assert.AreEqual(expectedValue == null ? null : expectedValue.Value, result == null ? null : result.Value);
        }

        [TestMethod]
        public void _SqlXml_FromString_Converter()
        {
            var column = (_SqlXml)_String.Const("<a></a>");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_SqlXml_FromString, json);

            var columnFromJson = (_SqlXml)Column.FromJson(null, json);
            Assert.AreEqual("<a></a>", columnFromJson.Eval().Value);
        }

        [TestMethod]
        public void _SqlXml_CastToString()
        {
            TestCastToString(_SqlXml.CreateSqlXml("<a></a>"), "<a></a>");
            TestCastToString(null, null);
        }

        private void TestCastToString(SqlXml x, string expectedValue)
        {
            _SqlXml column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(SqlXml), typeof(string));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _SqlXml_CastToString_Converter()
        {
            var column = (_String)_SqlXml.Const(_SqlXml.CreateSqlXml("<a></a>"));
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_SqlXml_CastToString, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("<a></a>", columnFromJson.Eval());
        }
    }
}

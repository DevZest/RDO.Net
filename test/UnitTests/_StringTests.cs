using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using DevZest.Data.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace DevZest.Data
{
    [TestClass]
    public class _StringTests : ColumnConverterTestsBase
    {
        [TestMethod]
        public void _String_Param()
        {
            TestParam("ABC");
            TestParam(null);
        }

        private void TestParam(String x)
        {
            var column = _String.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _String_Implicit()
        {
            TestImplicit("ABC");
            TestImplicit(null);
        }

        private void TestImplicit(String x)
        {
            _String column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _String_Const()
        {
            TestConst("ABC");
            TestConst(null);
        }

        private void TestConst(String x)
        {
            _String column = _String.Const(x);
            column.VerifyConst(x);
        }
        [TestMethod]
        public void _String_Add()
        {
            TestAdd("A", "B", "AB");
            TestAdd("A", null, null);
            TestAdd(null, null, null);
        }

        private void TestAdd(String x, String y, String expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 + column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Add, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_LessThan()
        {
            TestLessThan("099", "100", true);
            TestLessThan("99", "99", false);
            TestLessThan("99", "98", false);
            TestLessThan("5", null, null);
            TestLessThan(null, "5", null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(String x, String y, bool? expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_LessThanOrEqual()
        {
            TestLessThanOrEqual("99", "98", false);
            TestLessThanOrEqual("99", "99", true);
            TestLessThanOrEqual("099", "100", true);
            TestLessThanOrEqual("5", null, null);
            TestLessThanOrEqual(null, "5", null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(String x, String y, bool? expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_GreaterThan()
        {
            TestGreaterThan("100", "099", true);
            TestGreaterThan("100", "100", false);
            TestGreaterThan("099", "100", false);
            TestGreaterThan("5", null, null);
            TestGreaterThan(null, "5", null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(String x, String y, bool? expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_GreaterThanOrEqual()
        {
            TestGreaterThanOrEqual("100", "099", true);
            TestGreaterThanOrEqual("100", "100", true);
            TestGreaterThanOrEqual("099", "100", false);
            TestGreaterThanOrEqual("5", null, null);
            TestGreaterThanOrEqual(null, "5", null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(String x, String y, bool? expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_Equal()
        {
            TestEqual("2", "2", true);
            TestEqual("4", "5", false);
            TestEqual("1", null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(String x, String y, bool? expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_NotEqual()
        {
            TestNotEqual("1", "1", false);
            TestNotEqual("1", "2", true);
            TestNotEqual("1", null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(String x, String y, bool? expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_Add_Converter()
        {
            var column = _String.Const("a") + _String.Const("b");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_Add, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("ab", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_Equal_Converter()
        {
            var column = _String.Const("a") == _String.Const("a");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_GreaterThan_Converter()
        {
            var column = _String.Const("b") > _String.Const("a");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_GreaterThanOrEqual_Converter()
        {
            var column = _String.Const("a") >= _String.Const("a");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_LessThan_Converter()
        {
            var column = _String.Const("a") < _String.Const("b");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_LessThanOrEqual_Converter()
        {
            var column = _String.Const("a") <= _String.Const("a");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_NotEqual_Converter()
        {
            var column = _String.Const("a") != _String.Const("b");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }
    }
}

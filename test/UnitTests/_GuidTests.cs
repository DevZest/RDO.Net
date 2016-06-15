using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using DevZest.Data.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _GuidTests : ColumnConverterTestsBase
    {
        [TestMethod]
        public void _Guid_Param()
        {
            TestParam(Guid.NewGuid());
            TestParam(null);
        }

        private void TestParam(Guid? x)
        {
            var column = _Guid.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Guid_Implicit()
        {
            TestImplicit(Guid.NewGuid());
            TestImplicit(null);
        }

        private void TestImplicit(Guid? x)
        {
            _Guid column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Guid_Const()
        {
            TestConst(Guid.NewGuid());
            TestConst(null);
        }

        private void TestConst(Guid? x)
        {
            _Guid column = _Guid.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _Guid_FromString()
        {
            var guid = Guid.NewGuid();
            TestFromString(guid.ToString(), guid);
            TestFromString(null, null);
        }

        private void TestFromString(String x, Guid? expectedValue)
        {
            _String column1 = x;
            _Guid expr = (_Guid)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(Guid?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Guid_LessThan()
        {
            var x = new Guid();
            var y = Guid.NewGuid();
            TestLessThan(x, y, true);
            TestLessThan(x, x, false);
            TestLessThan(y, x, false);
            TestLessThan(x, null, null);
            TestLessThan(null, x, null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(Guid? x, Guid? y, bool? expectedValue)
        {
            _Guid column1 = x;
            _Guid column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Guid_LessThanOrEqual()
        {
            var x = new Guid();
            var y = Guid.NewGuid();
            TestLessThanOrEqual(y, x, false);
            TestLessThanOrEqual(x, x, true);
            TestLessThanOrEqual(x, y, true);
            TestLessThanOrEqual(x, null, null);
            TestLessThanOrEqual(null, x, null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(Guid? x, Guid? y, bool? expectedValue)
        {
            _Guid column1 = x;
            _Guid column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Guid_GreaterThan()
        {
            var x = new Guid();
            var y = Guid.NewGuid();
            TestGreaterThan(y, x, true);
            TestGreaterThan(x, x, false);
            TestGreaterThan(x, y, false);
            TestGreaterThan(x, null, null);
            TestGreaterThan(null, x, null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(Guid? x, Guid? y, bool? expectedValue)
        {
            _Guid column1 = x;
            _Guid column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Guid_GreaterThanOrEqual()
        {
            var x = new Guid();
            var y = Guid.NewGuid();
            TestGreaterThanOrEqual(y, x, true);
            TestGreaterThanOrEqual(x, x, true);
            TestGreaterThanOrEqual(x, y, false);
            TestGreaterThanOrEqual(x, null, null);
            TestGreaterThanOrEqual(null, x, null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(Guid? x, Guid? y, bool? expectedValue)
        {
            _Guid column1 = x;
            _Guid column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Guid_Equal()
        {
            var x = new Guid();
            var y = Guid.NewGuid();
            TestEqual(x, x, true);
            TestEqual(x, y, false);
            TestEqual(x, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(Guid? x, Guid? y, bool? expectedValue)
        {
            _Guid column1 = x;
            _Guid column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Guid_NotEqual()
        {
            var x = new Guid();
            var y = Guid.NewGuid();
            TestNotEqual(x, x, false);
            TestNotEqual(x, y, true);
            TestNotEqual(x, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(Guid? x, Guid? y, bool? expectedValue)
        {
            _Guid column1 = x;
            _Guid column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Guid_Equal_Converter()
        {
            var column = _Guid.Const(new Guid()) == _Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Guid_FromString_Converter()
        {
            var column = (_Guid)_String.Const("00000000-0000-0000-0000-000000000000");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_FromString, json);

            var columnFromJson = (_Guid)Column.FromJson(null, json);
            Assert.AreEqual(new Guid(), columnFromJson.Eval());
        }

        [TestMethod]
        public void _Guid_GreaterThan_Converter()
        {
            var column = _Guid.Const(new Guid()) > _Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(false, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Guid_GreaterThanOrEqual_Converter()
        {
            var column = _Guid.Const(new Guid()) >= _Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Guid_LessThan_Converter()
        {
            var column = _Guid.Const(new Guid()) < _Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(false, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Guid_LessThanOrEqual_Converter()
        {
            var column = _Guid.Const(new Guid()) <= _Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Guid_NotEqual_Converter()
        {
            var column = _Guid.Const(new Guid()) != _Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(false, columnFromJson.Eval());
        }
    }
}

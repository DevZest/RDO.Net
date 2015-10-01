using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace DevZest.Data
{
    [TestClass]
    public class _StringTests
    {
        [TestMethod]
        public void StringColumn_Param()
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
        public void StringColumn_implicit_convert()
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
        public void StringColumn_Const()
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
        public void StringColumn_add()
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
        public void StringColumn_less_than()
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
        public void StringColumn_less_than_or_equal()
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
        public void StringColumn_greater_than()
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
        public void StringColumn_greater_than_or_equal()
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
        public void StringColumn_equal()
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
        public void StringColumn_not_equal()
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
        public void StringColumn_cast_from_DbBoolean()
        {
            TestDbBooleanCast(_Boolean.True, "True");
            TestDbBooleanCast(_Boolean.False, "False");
            TestDbBooleanCast(_Boolean.Null, null);
        }

        private void TestDbBooleanCast(_Boolean x, String expectedValue)
        {
            _String expr = (_String)x;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(x, typeof(bool?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void StringColumn_cast_from_DbByte()
        {
            TestDbByteCast(null, null);
            TestDbByteCast(127, "127");
        }

        private void TestDbByteCast(byte? x, String expectedValue)
        {
            _Byte column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(byte?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void StringColumn_cast_from_DbChar()
        {
            TestDbCharCast(null, null);
            TestDbCharCast('A', "A");
        }

        private void TestDbCharCast(Char? x, String expectedValue)
        {
            _Char column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Char?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void StringColumn_cast_from_DbDateTime()
        {
            var now = DateTime.Now;
            TestDbDateTimeCast(null, null);
            TestDbDateTimeCast(now, now.ToString("O", CultureInfo.InvariantCulture));
        }

        private void TestDbDateTimeCast(DateTime? x, String expectedValue)
        {
            _DateTime column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(DateTime?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void StringColumn_cast_from_DbGuid()
        {
            var guid = Guid.NewGuid();
            TestDbGuidCast(null, null);
            TestDbGuidCast(guid, guid.ToString());
        }

        private void TestDbGuidCast(Guid? x, String expectedValue)
        {
            _Guid column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Guid?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void StringColumn_cast_from_DbInt16()
        {
            TestDbInt16Cast(null, null);
            TestDbInt16Cast(5, "5");
        }

        private void TestDbInt16Cast(Int16? x, String expectedValue)
        {
            _Int16 column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int16?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void StringColumn_cast_from_DbInt32()
        {
            TestDbInt32Cast(1234567, "1234567");
            TestDbInt32Cast(null, null);
        }

        private void TestDbInt32Cast(Int32? x, String expectedValue)
        {
            _Int32 column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int32?), typeof(String));
            expr.VerifyEval(expectedValue);
        }


        [TestMethod]
        public void StringColumn_cast_from_DbInt64()
        {
            TestDbInt64Cast(8, "8");
            TestDbInt64Cast(null, null);
        }

        private void TestDbInt64Cast(Int64? x, String expectedValue)
        {
            _Int64 column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int64?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void StringColumn_cast_from_DbDecimal()
        {
            TestDbDecimalCast(8, "8");
            TestDbDecimalCast(null, null);
        }

        private void TestDbDecimalCast(Decimal? x, String expectedValue)
        {
            _Decimal column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Decimal?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void StringColumn_cast_from_DbDouble()
        {
            TestDbDoubleCast(8, "8");
            TestDbDoubleCast(null, null);
        }

        private void TestDbDoubleCast(Double? x, String expectedValue)
        {
            _Double column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Double?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void StringColumn_cast_from_DbSingle()
        {
            TestDbSingleCast(8, "8");
            TestDbSingleCast(null, null);
        }

        private void TestDbSingleCast(Single? x, String expectedValue)
        {
            _Single column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Single?), typeof(String));
            expr.VerifyEval(expectedValue);
        }
    }
}

using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class _DateTimeOffsetTests
    {
        [TestMethod]
        public void DateTimeOffsetColumn_Param()
        {
            TestParam(DateTimeOffset.Now);
            TestParam(null);
        }

        private void TestParam(DateTimeOffset? x)
        {
            var column = _DateTimeOffset.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void DateTimeOffsetColumn_implicit_convert()
        {
            TestImplicit(DateTimeOffset.Now);
            TestImplicit(null);
        }

        private void TestImplicit(DateTimeOffset? x)
        {
            _DateTimeOffset column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void DateTimeOffsetColumn_Const()
        {
            TestConst(DateTimeOffset.Now);
            TestConst(null);
        }

        private void TestConst(DateTimeOffset? x)
        {
            _DateTimeOffset column = _DateTimeOffset.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void DateTimeOffsetColumn_convert_from_StringColumn()
        {
            var now = DateTimeOffset.Now;
            TestDbStringCast(now.ToString("O", CultureInfo.InvariantCulture), now);
            TestDbStringCast(null, null);
        }

        private void TestDbStringCast(String x, DateTimeOffset? expectedValue)
        {
            _String column1 = x;
            _DateTimeOffset expr = (_DateTimeOffset)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(DateTimeOffset?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void DateTimeOffsetColumn_less_than()
        {
            var x = DateTimeOffset.Now;
            var y = x.AddSeconds(1);
            TestLessThan(x, y, true);
            TestLessThan(x, x, false);
            TestLessThan(y, x, false);
            TestLessThan(x, null, null);
            TestLessThan(null, x, null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(DateTimeOffset? x, DateTimeOffset? y, bool? expectedValue)
        {
            _DateTimeOffset column1 = x;
            _DateTimeOffset column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void DateTimeOffsetColumn_less_than_or_equal()
        {
            var x = DateTimeOffset.Now;
            var y = x.AddSeconds(1);
            TestLessThanOrEqual(y, x, false);
            TestLessThanOrEqual(x, x, true);
            TestLessThanOrEqual(x, y, true);
            TestLessThanOrEqual(x, null, null);
            TestLessThanOrEqual(null, x, null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(DateTimeOffset? x, DateTimeOffset? y, bool? expectedValue)
        {
            _DateTimeOffset column1 = x;
            _DateTimeOffset column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void DateTimeOffsetColumn_greater_than()
        {
            var x = DateTimeOffset.Now;
            var y = x.AddSeconds(1);
            TestGreaterThan(y, x, true);
            TestGreaterThan(x, x, false);
            TestGreaterThan(x, y, false);
            TestGreaterThan(x, null, null);
            TestGreaterThan(null, x, null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(DateTimeOffset? x, DateTimeOffset? y, bool? expectedValue)
        {
            _DateTimeOffset column1 = x;
            _DateTimeOffset column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void DbDateTime_greater_than_or_equal()
        {
            var x = DateTimeOffset.Now;
            var y = x.AddSeconds(1);
            TestGreaterThanOrEqual(y, x, true);
            TestGreaterThanOrEqual(x, x, true);
            TestGreaterThanOrEqual(x, y, false);
            TestGreaterThanOrEqual(x, null, null);
            TestGreaterThanOrEqual(null, x, null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(DateTimeOffset? x, DateTimeOffset? y, bool? expectedValue)
        {
            _DateTimeOffset column1 = x;
            _DateTimeOffset column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void DateTimeOffsetColumn_equal()
        {
            var x = DateTimeOffset.Now;
            var y = x.AddSeconds(1);
            TestEqual(x, x, true);
            TestEqual(x, y, false);
            TestEqual(x, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(DateTimeOffset? x, DateTimeOffset? y, bool? expectedValue)
        {
            _DateTimeOffset column1 = x;
            _DateTimeOffset column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void DateTimeOffsetColumn_not_equal()
        {
            var x = DateTimeOffset.Now;
            var y = x.AddSeconds(1);
            TestNotEqual(x, x, false);
            TestNotEqual(x, y, true);
            TestNotEqual(x, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(DateTimeOffset? x, DateTimeOffset? y, bool? expectedValue)
        {
            _DateTimeOffset column1 = x;
            _DateTimeOffset column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }
    }
}

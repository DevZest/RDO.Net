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
        public void _DateTimeOffset_Param()
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
        public void _DateTimeOffset_Implicit()
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
        public void _DateTimeOffset_Const()
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
        public void _DateTimeOffset_FromString()
        {
            var now = DateTimeOffset.Now;
            TestFromString(now.ToString("O", CultureInfo.InvariantCulture), now);
            TestFromString(null, null);
        }

        private void TestFromString(String x, DateTimeOffset? expectedValue)
        {
            _String column1 = x;
            _DateTimeOffset expr = (_DateTimeOffset)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(DateTimeOffset?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _DateTimeOffset_LessThan()
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
        public void _DateTimeOffset_LessThanOrEqual()
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
        public void _DateTimeOffset_GreaterThan()
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
        public void _DateTimeOffset_GreaterThanOrEqual()
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
        public void _DateTimeOffset_Equal()
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
        public void _DateTimeOffset_NotEqual()
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

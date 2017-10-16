using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace DevZest.Data
{
    [TestClass]
    public class _DateTimeTests
    {
        [TestMethod]
        public void _DateTime_Param()
        {
            TestParam(DateTime.Now);
            TestParam(null);
        }

        private void TestParam(DateTime? x)
        {
            var column = _DateTime.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _DateTime_Implicit()
        {
            TestImplicit(DateTime.Now);
            TestImplicit(null);
        }

        private static void TestImplicit(DateTime? x)
        {
            _DateTime column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _DateTime_Const()
        {
            TestConst(DateTime.Now);
            TestConst(null);
        }

        private static void TestConst(DateTime? x)
        {
            _DateTime column = _DateTime.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _DateTime_FromString()
        {
            var now = DateTime.Now;
            TestFromString(now.ToString("O"), now);
            TestFromString(null, null);
        }

        private void TestFromString(String x, DateTime? expectedValue)
        {
            _String column1 = x;
            _DateTime expr = (_DateTime)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(DateTime?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _DateTime_LessThan()
        {
            var x = DateTime.Now;
            var y = x.AddSeconds(1);
            TestLessThan(x, y, true);
            TestLessThan(x, x, false);
            TestLessThan(y, x, false);
            TestLessThan(x, null, null);
            TestLessThan(null, x, null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(DateTime? x, DateTime? y, bool? expectedValue)
        {
            _DateTime column1 = x;
            _DateTime column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _DateTime_LessThanOrEqual()
        {
            var x = DateTime.Now;
            var y = x.AddSeconds(1);
            TestLessThanOrEqual(y, x, false);
            TestLessThanOrEqual(x, x, true);
            TestLessThanOrEqual(x, y, true);
            TestLessThanOrEqual(x, null, null);
            TestLessThanOrEqual(null, x, null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(DateTime? x, DateTime? y, bool? expectedValue)
        {
            _DateTime column1 = x;
            _DateTime column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _DateTime_GreaterThan()
        {
            var x = DateTime.Now;
            var y = x.AddSeconds(1);
            TestGreaterThan(y, x, true);
            TestGreaterThan(x, x, false);
            TestGreaterThan(x, y, false);
            TestGreaterThan(x, null, null);
            TestGreaterThan(null, x, null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(DateTime? x, DateTime? y, bool? expectedValue)
        {
            _DateTime column1 = x;
            _DateTime column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _DateTime_GreaterThanOrEqual()
        {
            var x = DateTime.Now;
            var y = x.AddSeconds(1);
            TestGreaterThanOrEqual(y, x, true);
            TestGreaterThanOrEqual(x, x, true);
            TestGreaterThanOrEqual(x, y, false);
            TestGreaterThanOrEqual(x, null, null);
            TestGreaterThanOrEqual(null, x, null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(DateTime? x, DateTime? y, bool? expectedValue)
        {
            _DateTime column1 = x;
            _DateTime column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _DateTime_Equal()
        {
            var x = DateTime.Now;
            var y = x.AddSeconds(1);
            TestEqual(x, x, true);
            TestEqual(x, y, false);
            TestEqual(x, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(DateTime? x, DateTime? y, bool? expectedValue)
        {
            _DateTime column1 = x;
            _DateTime column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _DateTime_NotEqual()
        {
            var x = DateTime.Now;
            var y = x.AddSeconds(1);
            TestNotEqual(x, x, false);
            TestNotEqual(x, y, true);
            TestNotEqual(x, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(DateTime? x, DateTime? y, bool? expectedValue)
        {
            _DateTime column1 = x;
            _DateTime column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _DateTime_CastToString()
        {
            var now = DateTime.Now;
            TestCastToString(null, null);
            TestCastToString(now, now.ToString("O", CultureInfo.InvariantCulture));
        }

        private void TestCastToString(DateTime? x, String expectedValue)
        {
            _DateTime column1 = x;
            _String expr = column1.CastToString();
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(DateTime?), typeof(String));
            expr.VerifyEval(expectedValue);
        }
    }
}

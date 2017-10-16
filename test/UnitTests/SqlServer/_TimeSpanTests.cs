using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class _TimeSpanTests
    {
        [TestMethod]
        public void _TimeSpan_Param()
        {
            TestParam(TimeSpan.FromDays(5));
            TestParam(null);
        }

        private void TestParam(TimeSpan? x)
        {
            var column = _TimeSpan.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _TimeSpan_Implicit()
        {
            TestImplicit(TimeSpan.FromDays(5));
            TestImplicit(null);
        }

        private void TestImplicit(TimeSpan? x)
        {
            _TimeSpan column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _TimeSpan_Const()
        {
            TestConst(TimeSpan.FromDays(5));
            TestConst(null);
        }

        private void TestConst(TimeSpan? x)
        {
            _TimeSpan column = _TimeSpan.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _TimeSpan_FromString()
        {
            var x = TimeSpan.FromDays(5);
            TestFromString(x.ToString(), x);
            TestFromString(null, null);
        }

        private void TestFromString(String x, TimeSpan? expectedValue)
        {
            _String column1 = x;
            _TimeSpan expr = (_TimeSpan)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(TimeSpan?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _TimeSpan_LessThan()
        {
            var x = TimeSpan.FromDays(5);
            var y = TimeSpan.FromDays(6);
            TestLessThan(x, y, true);
            TestLessThan(x, x, false);
            TestLessThan(y, x, false);
            TestLessThan(x, null, null);
            TestLessThan(null, x, null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(TimeSpan? x, TimeSpan? y, bool? expectedValue)
        {
            _TimeSpan column1 = x;
            _TimeSpan column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _TimeSpan_LessThanOrEqual()
        {
            var x = TimeSpan.FromDays(5);
            var y = TimeSpan.FromDays(6);
            TestLessThanOrEqual(y, x, false);
            TestLessThanOrEqual(x, x, true);
            TestLessThanOrEqual(x, y, true);
            TestLessThanOrEqual(x, null, null);
            TestLessThanOrEqual(null, x, null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(TimeSpan? x, TimeSpan? y, bool? expectedValue)
        {
            _TimeSpan column1 = x;
            _TimeSpan column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _TimeSpan_GreaterThan()
        {
            var x = TimeSpan.FromDays(5);
            var y = TimeSpan.FromDays(6);
            TestGreaterThan(y, x, true);
            TestGreaterThan(x, x, false);
            TestGreaterThan(x, y, false);
            TestGreaterThan(x, null, null);
            TestGreaterThan(null, x, null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(TimeSpan? x, TimeSpan? y, bool? expectedValue)
        {
            _TimeSpan column1 = x;
            _TimeSpan column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _TimeSpan_GreaterThanOrEqual()
        {
            var x = TimeSpan.FromDays(5);
            var y = TimeSpan.FromDays(6);
            TestGreaterThanOrEqual(y, x, true);
            TestGreaterThanOrEqual(x, x, true);
            TestGreaterThanOrEqual(x, y, false);
            TestGreaterThanOrEqual(x, null, null);
            TestGreaterThanOrEqual(null, x, null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(TimeSpan? x, TimeSpan? y, bool? expectedValue)
        {
            _TimeSpan column1 = x;
            _TimeSpan column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _TimeSpan_Equal()
        {
            var x = TimeSpan.FromDays(5);
            var y = TimeSpan.FromDays(6);
            TestEqual(x, x, true);
            TestEqual(x, y, false);
            TestEqual(x, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(TimeSpan? x, TimeSpan? y, bool? expectedValue)
        {
            _TimeSpan column1 = x;
            _TimeSpan column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _TimeSpan_NotEqual()
        {
            var x = TimeSpan.FromDays(5);
            var y = TimeSpan.FromDays(6);
            TestNotEqual(x, x, false);
            TestNotEqual(x, y, true);
            TestNotEqual(x, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(TimeSpan? x, TimeSpan? y, bool? expectedValue)
        {
            _TimeSpan column1 = x;
            _TimeSpan column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _TimeSpan_CastToString()
        {
            TestCastToString(TimeSpan.FromDays(5), "5.00:00:00");
            TestCastToString(null, null);
        }

        private void TestCastToString(TimeSpan? x, String expectedValue)
        {
            _TimeSpan column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(TimeSpan?), typeof(String));
            expr.VerifyEval(expectedValue);
        }
    }
}

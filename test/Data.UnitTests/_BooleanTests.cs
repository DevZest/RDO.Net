using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _BooleanTests
    {
        [TestMethod]
        public void _Boolean_Param()
        {
            TestParam(true);
            TestParam(false);
            TestParam(null);
        }

        private void TestParam(bool? x)
        {
            _Boolean column = _Boolean.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Boolean_implicit_convert()
        {
            TestImplicit(true);
            TestImplicit(false);
            TestImplicit(null);
        }

        private void TestImplicit(bool? x)
        {
            _Boolean column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Boolean_Const()
        {
            TestConst(true);
            TestConst(false);
            TestConst(null);
        }

        private static void TestConst(bool? x)
        {
            _Boolean column = _Boolean.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _Boolean_Not()
        {
            TestNot(_Boolean.True);
            TestNot(_Boolean.False);
            TestNot(_Boolean.Null);
        }

        private void TestNot(_Boolean x)
        {
            var expr = !x;
            expr.VerifyEval(!x.Eval());
            var dbExpr = (DbUnaryExpression)expr.DbExpression;
            dbExpr.Verify(DbUnaryExpressionKind.Not, x);
        }

        [TestMethod]
        public void _Boolean_And()
        {
            TestAnd(true, true, true);
            TestAnd(false, true, false);
            TestAnd(true, false, false);
            TestAnd(false, false, false);
            TestAnd(false, null, false);
            TestAnd(null, false, false);
            TestAnd(null, null, null);
        }

        private void TestAnd(bool? x, bool? y, bool? expectedValue)
        {
            _Boolean left = x;
            _Boolean right = y;
            var expr = left & right;
            expr.VerifyEval(expectedValue);
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.And, left, right);
        }

        [TestMethod]
        public void _Boolean_Or()
        {
            TestOr(true, true, true);
            TestOr(false, true, true);
            TestOr(true, false, true);
            TestOr(false, false, false);
            TestOr(false, null, null);
            TestOr(null, false, null);
            TestOr(null, null, null);
        }

        private void TestOr(bool? x, bool? y, bool? expectedValue)
        {
            _Boolean left = x;
            _Boolean right = y;
            var expr = left | right;
            expr.VerifyEval(expectedValue);
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Or, left, right);
        }

        [TestMethod]
        public void _Boolean_FromString()
        {
            TestStringColumnCast(null, null);
            TestStringColumnCast("True", true);
            TestStringColumnCast("False", false);
        }

        private void TestStringColumnCast(String x, Boolean? expectedValue)
        {
            _String column1 = _String.Const(x);
            _Boolean expr = (_Boolean)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(Boolean?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Boolean_Equal()
        {
            TestEqual(null, null, null);
            TestEqual(null, true, null);
            TestEqual(null, false, null);
            TestEqual(true, null, null);
            TestEqual(false, null, null);
            TestEqual(true, true, true);
            TestEqual(false, false, true);
            TestEqual(true, false, false);
            TestEqual(false, true, false);
        }

        private void TestEqual(bool? x, bool? y, bool? expectedValue)
        {
            _Boolean column1 = x;
            _Boolean column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Boolean_NotEqual()
        {
            TestNotEqual(true, true, false);
            TestNotEqual(false, false, false);
            TestNotEqual(true, false, true);
            TestNotEqual(false, true, true);
            TestNotEqual(true, null, null);
            TestNotEqual(false, null, null);
            TestNotEqual(null, true, null);
            TestNotEqual(null, false, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(bool? x, bool? y, bool? expectedValue)
        {
            _Boolean column1 = x;
            _Boolean column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }
    }
}

using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _CharEnumTests
    {
        private enum WeekDay
        {
            Sun = '0',
            Mon = '1',
            Tue = '2',
            Wed = '3',
            Thu = '4',
            Fri = '5',
            Sat = '6'
        }

        [TestMethod]
        public void _CharEnum_Param()
        {
            TestParam(WeekDay.Fri);
            TestParam(null);
        }

        private void TestParam(WeekDay? x)
        {
            var column = _CharEnum<WeekDay>.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _CharEnum_implicit_convert()
        {
            TestImplicit(WeekDay.Mon);
            TestImplicit(null);
        }

        private static void TestImplicit(WeekDay? x)
        {
            _CharEnum<WeekDay> column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _CharEnum_Const()
        {
            TestConst(WeekDay.Mon);
            TestConst(null);
        }

        private static void TestConst(WeekDay? x)
        {
            var column = _CharEnum<WeekDay>.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _CharEnum_CastToString()
        {
            TestCastToString(WeekDay.Mon, nameof(WeekDay.Mon));
            TestCastToString(null, null);
        }

        private static void TestCastToString(WeekDay? x, string strValue)
        {
            var column = (_String)_CharEnum<WeekDay>.Const(x);
            column.VerifyEval(strValue);
        }

        [TestMethod]
        public void _CharEnum_Equal()
        {
            TestEqual(WeekDay.Tue, WeekDay.Tue, true);
            TestEqual(WeekDay.Thu, WeekDay.Fri, false);
            TestEqual(WeekDay.Mon, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(WeekDay? x, WeekDay? y, bool? expectedValue)
        {
            _CharEnum<WeekDay> column1 = x;
            _CharEnum<WeekDay> column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _CharEnum_NotEqual()
        {
            TestNotEqual(WeekDay.Mon, WeekDay.Mon, false);
            TestNotEqual(WeekDay.Mon, WeekDay.Tue, true);
            TestNotEqual(WeekDay.Mon, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(WeekDay? x, WeekDay? y, bool? expectedValue)
        {
            _CharEnum<WeekDay> column1 = x;
            _CharEnum<WeekDay> column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _CharEnum_FromChar()
        {
            TestFromChar(null, null);
            TestFromChar('1', WeekDay.Mon);
        }

        private void TestFromChar(Char? x, WeekDay? expectedValue)
        {
            _Char column1 = x;
            var expr = (_CharEnum<WeekDay>)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Char?), typeof(WeekDay?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ToChar()
        {
            TestToChar(null, null);
            TestToChar(WeekDay.Mon, '1');
        }

        private void TestToChar(WeekDay? x, Char? expectedValue)
        {
            _CharEnum<WeekDay> column1 = x;
            var expr = (_Char)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(WeekDay?), typeof(Char?));
            expr.VerifyEval(expectedValue);
        }
    }
}

using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _Int16EnumTests
    {
        private enum WeekDay
        {
            Sun = 0,
            Mon,
            Tue,
            Wed,
            Thu,
            Fri,
            Sat
        }

        [Flags]
        private enum Bits
        {
            None = 0,
            Flag1 = 1,
            Flag2 = 2,
            Flag3 = 4,
            Flag4 = 8,
            Flag5 = 16,
            Flag6 = 32,
            All = Flag1 | Flag2 | Flag3 | Flag4 | Flag5 | Flag6
        }

        [TestMethod]
        public void _Int16Enum_Param()
        {
            TestParam(WeekDay.Fri);
            TestParam(null);
        }

        private void TestParam(WeekDay? x)
        {
            var column = _Int16Enum<WeekDay>.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Int16Enum_implicit_convert()
        {
            TestImplicit(WeekDay.Mon);
            TestImplicit(null);
        }

        private static void TestImplicit(WeekDay? x)
        {
            _Int16Enum<WeekDay> column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Int16Enum_Const()
        {
            TestConst(WeekDay.Mon);
            TestConst(null);
        }

        private static void TestConst(WeekDay? x)
        {
            var column = _Int16Enum<WeekDay>.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _Int16Enum_CastToString()
        {
            TestCastToString(WeekDay.Mon, nameof(WeekDay.Mon));
            TestCastToString(null, null);
        }

        private static void TestCastToString(WeekDay? x, string strValue)
        {
            var column = (_String)_Int16Enum<WeekDay>.Const(x);
            column.VerifyEval(strValue);
        }

        [TestMethod]
        public void _Int16_BitwiseAnd()
        {
            TestBitwiseAnd(Bits.Flag5, Bits.None, Bits.None);
            TestBitwiseAnd(Bits.Flag5, null, null);
            TestBitwiseAnd(null, null, null);
        }

        private void TestBitwiseAnd(Bits? x, Bits? y, Bits? expectedValue)
        {
            _Int16Enum<Bits> column1 = x;
            _Int16Enum<Bits> column2 = y;
            var expr = column1 & column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseAnd, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Int16_BitwiseOr()
        {
            TestBitwiseOr(Bits.Flag1, Bits.Flag2, Bits.Flag1 | Bits.Flag2);
            TestBitwiseOr(Bits.Flag1, null, null);
            TestBitwiseOr(null, null, null);
        }

        private void TestBitwiseOr(Bits? x, Bits? y, Bits? expectedValue)
        {
            _Int16Enum<Bits> column1 = x;
            _Int16Enum<Bits> column2 = y;
            var expr = column1 | column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseOr, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Int16Enum_Equal()
        {
            TestEqual(WeekDay.Tue, WeekDay.Tue, true);
            TestEqual(WeekDay.Thu, WeekDay.Fri, false);
            TestEqual(WeekDay.Mon, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(WeekDay? x, WeekDay? y, bool? expectedValue)
        {
            _Int16Enum<WeekDay> column1 = x;
            _Int16Enum<WeekDay> column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Int16Enum_NotEqual()
        {
            TestNotEqual(WeekDay.Mon, WeekDay.Mon, false);
            TestNotEqual(WeekDay.Mon, WeekDay.Tue, true);
            TestNotEqual(WeekDay.Mon, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(WeekDay? x, WeekDay? y, bool? expectedValue)
        {
            _Int16Enum<WeekDay> column1 = x;
            _Int16Enum<WeekDay> column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Int16Enum_FromInt16()
        {
            TestFromInt16(null, null);
            TestFromInt16(1, WeekDay.Mon);
            TestFromInt16(128, (WeekDay)128);
        }

        private void TestFromInt16(Int16? x, WeekDay? expectedValue)
        {
            _Int16 column1 = x;
            var expr = (_Int16Enum<WeekDay>)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int16?), typeof(WeekDay?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ToInt16()
        {
            TestToInt16(null, null);
            TestToInt16(WeekDay.Mon, 1);
            TestToInt16((WeekDay)128, 128);
        }

        private void TestToInt16(WeekDay? x, Int16? expectedValue)
        {
            _Int16Enum<WeekDay> column1 = x;
            var expr = (_Int16)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(WeekDay?), typeof(Int16?));
            expr.VerifyEval(expectedValue);
        }
    }
}

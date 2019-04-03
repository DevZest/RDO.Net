using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _ByteEnumTests
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
        public void _ByteEnum_Param()
        {
            TestParam(WeekDay.Fri);
            TestParam(null);
        }

        private void TestParam(WeekDay? x)
        {
            var column = _ByteEnum<WeekDay>.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _ByteEnum_implicit_convert()
        {
            TestImplicit(WeekDay.Mon);
            TestImplicit(null);
        }

        private static void TestImplicit(WeekDay? x)
        {
            _ByteEnum<WeekDay> column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _ByteEnum_Const()
        {
            TestConst(WeekDay.Mon);
            TestConst(null);
        }

        private static void TestConst(WeekDay? x)
        {
            var column = _ByteEnum<WeekDay>.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _ByteEnum_CastToString()
        {
            TestCastToString(WeekDay.Mon, nameof(WeekDay.Mon));
            TestCastToString(null, null);
        }

        private static void TestCastToString(WeekDay? x, string strValue)
        {
            var column = (_String)_ByteEnum<WeekDay>.Const(x);
            column.VerifyEval(strValue);
        }

        [TestMethod]
        public void _Byte_BitwiseAnd()
        {
            TestBitwiseAnd(Bits.Flag5, Bits.None, Bits.None);
            TestBitwiseAnd(Bits.Flag5, null, null);
            TestBitwiseAnd(null, null, null);
        }

        private void TestBitwiseAnd(Bits? x, Bits? y, Bits? expectedValue)
        {
            _ByteEnum<Bits> column1 = x;
            _ByteEnum<Bits> column2 = y;
            var expr = column1 & column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseAnd, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Byte_BitwiseOr()
        {
            TestBitwiseOr(Bits.Flag1, Bits.Flag2, Bits.Flag1 | Bits.Flag2);
            TestBitwiseOr(Bits.Flag1, null, null);
            TestBitwiseOr(null, null, null);
        }

        private void TestBitwiseOr(Bits? x, Bits? y, Bits? expectedValue)
        {
            _ByteEnum<Bits> column1 = x;
            _ByteEnum<Bits> column2 = y;
            var expr = column1 | column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseOr, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _ByteEnum_Equal()
        {
            TestEqual(WeekDay.Tue, WeekDay.Tue, true);
            TestEqual(WeekDay.Thu, WeekDay.Fri, false);
            TestEqual(WeekDay.Mon, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(WeekDay? x, WeekDay? y, bool? expectedValue)
        {
            _ByteEnum<WeekDay> column1 = x;
            _ByteEnum<WeekDay> column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _ByteEnum_NotEqual()
        {
            TestNotEqual(WeekDay.Mon, WeekDay.Mon, false);
            TestNotEqual(WeekDay.Mon, WeekDay.Tue, true);
            TestNotEqual(WeekDay.Mon, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(WeekDay? x, WeekDay? y, bool? expectedValue)
        {
            _ByteEnum<WeekDay> column1 = x;
            _ByteEnum<WeekDay> column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _ByteEnum_FromByte()
        {
            TestFromByte(null, null);
            TestFromByte(1, WeekDay.Mon);
            TestFromByte(128, (WeekDay)128);
        }

        private void TestFromByte(Byte? x, WeekDay? expectedValue)
        {
            _Byte column1 = x;
            var expr = (_ByteEnum<WeekDay>)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Byte?), typeof(WeekDay?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ToByte()
        {
            TestToByte(null, null);
            TestToByte(WeekDay.Mon, 1);
            TestToByte((WeekDay)128, 128);
        }

        private void TestToByte(WeekDay? x, Byte? expectedValue)
        {
            _ByteEnum<WeekDay> column1 = x;
            var expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(WeekDay?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }
    }
}

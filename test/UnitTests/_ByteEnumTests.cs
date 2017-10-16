using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _ByteEnumTests : ColumnConverterTestsBase
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

        //[TestMethod]
        //public void _ByteEnum_Equal_Converter()
        //{
        //    var column = _ByteEnum<WeekDay>.Const(WeekDay.Mon) == _ByteEnum<WeekDay>.Const(WeekDay.Mon);
        //    var json = column.ToJson(true);
        //    Assert.AreEqual(string.Empty, json);

        //    var columnFromJson = Column.ParseJson<_Boolean>(null, json);
        //    Assert.AreEqual(true, columnFromJson.Eval());
        //}
    }
}

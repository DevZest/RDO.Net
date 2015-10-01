using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _ByteTests
    {
        [TestMethod]
        public void ByteColumn_Param()
        {
            TestParam(5);
            TestParam(null);
        }

        private void TestParam(byte? x)
        {
            _Byte column = _Byte.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void ByteColumn_implicit_convert()
        {
            TestImplicit(5);
            TestImplicit(null);
        }

        private static void TestImplicit(byte? x)
        {
            _Byte column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void ByteColumn_Const()
        {
            TestConst(5);
            TestConst(null);
        }

        private static void TestConst(byte? x)
        {
            _Byte column = _Byte.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void ByteColumn_ones_complement()
        {
            byte x = 5;
            TestOnesComplement(x, (byte)~x);
            TestOnesComplement(null, null);
        }

        private void TestOnesComplement(Byte? x, Byte? expectedValue)
        {
            _Byte column = x;
            var expr = ~column;
            var dbExpr = (DbUnaryExpression)expr.DbExpression;
            dbExpr.Verify(DbUnaryExpressionKind.OnesComplement, column);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_add()
        {
            TestAdd(1, 2, 3);
            TestAdd(128, 128, 0);
            TestAdd(1, null, null);
            TestAdd(null, null, null);
        }

        private void TestAdd(Byte? x, Byte? y, Byte? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 + column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Add, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_substract()
        {
            TestSubstract(5, 2, 3);
            TestSubstract(null, 2, null);
            TestSubstract(2, null, null);
        }

        private void TestSubstract(Byte? x, Byte? y, Byte? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 - column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Substract, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_multipy()
        {
            TestMultiply(5, 5, 25);
            TestMultiply(5, null, null);
            TestMultiply(null, null, null);
        }

        private void TestMultiply(Byte? x, Byte? y, Byte? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 * column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Multiply, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_divide()
        {
            TestDivide(12, 4, 3);
            TestDivide(5, null, null);
            TestDivide(null, 5, null);
            TestDivide(null, null, null);
        }

        private void TestDivide(Byte? x, Byte? y, Byte? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 / column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Divide, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_modulo()
        {
            TestModulo(5, 3, 2);
            TestModulo(100, null, null);
            TestModulo(null, null, null);
        }

        private void TestModulo(Byte? x, Byte? y, Byte? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 % column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Modulo, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_bitwise_AND()
        {
            TestBitwiseAnd(102, 55, 102 & 55);
            TestBitwiseAnd(100, null, null);
            TestBitwiseAnd(null, null, null);
        }

        private void TestBitwiseAnd(Byte? x, Byte? y, Byte? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 & column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseAnd, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_Bitwise_OR()
        {
            TestBitwiseOr(5, 12, 5 | 12);
            TestBitwiseOr(5, null, null);
            TestBitwiseOr(null, null, null);
        }

        private void TestBitwiseOr(Byte? x, Byte? y, Byte? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 | column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseOr, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_bitwise_XOR()
        {
            TestBitwiseXor(64, 128, 64 ^ 128);
            TestBitwiseXor(32, null, null);
            TestBitwiseXor(null, 32, null);
            TestBitwiseXor(null, null, null);
        }

        private void TestBitwiseXor(Byte? x, Byte? y, Byte? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 ^ column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseXor, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_less_than()
        {
            TestLessThan(99, 100, true);
            TestLessThan(99, 99, false);
            TestLessThan(99, 98, false);
            TestLessThan(5, null, null);
            TestLessThan(null, 5, null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(Byte? x, Byte? y, bool? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_less_than_or_equal()
        {
            TestLessThanOrEqual(99, 98, false);
            TestLessThanOrEqual(99, 99, true);
            TestLessThanOrEqual(99, 100, true);
            TestLessThanOrEqual(5, null, null);
            TestLessThanOrEqual(null, 5, null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(Byte? x, Byte? y, bool? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_greater_than()
        {
            TestGreaterThan(100, 99, true);
            TestGreaterThan(100, 100, false);
            TestGreaterThan(99, 100, false);
            TestGreaterThan(5, null, null);
            TestGreaterThan(null, 5, null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(Byte? x, Byte? y, bool? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_greater_than_or_equal()
        {
            TestGreaterThanOrEqual(100, 99, true);
            TestGreaterThanOrEqual(100, 100, true);
            TestGreaterThanOrEqual(99, 100, false);
            TestGreaterThanOrEqual(5, null, null);
            TestGreaterThanOrEqual(null, 5, null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(Byte? x, Byte? y, bool? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_equal()
        {
            TestEqual(2, 2, true);
            TestEqual(4, 5, false);
            TestEqual(1, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(Byte? x, Byte? y, bool? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_test_not_equal()
        {
            TestNotEqual(1, 1, false);
            TestNotEqual(1, 2, true);
            TestNotEqual(1, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(Byte? x, Byte? y, bool? expectedValue)
        {
            _Byte column1 = x;
            _Byte column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_cast_from_DbBoolean()
        {
            TestDbBooleanCast(_Boolean.True, 1);
            TestDbBooleanCast(_Boolean.False, 0);
            TestDbBooleanCast(_Boolean.Null, null);
        }

        private void TestDbBooleanCast(_Boolean x, Byte? expectedValue)
        {
            _Byte expr = (_Byte)x;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(x, typeof(bool?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_cast_from_Int16Column()
        {
            TestInt16ColumnCast(null, null);
            TestInt16ColumnCast(127, 127);
            TestInt16ColumnCast(1024, 0);
        }

        private void TestInt16ColumnCast(Int16? x, Byte? expectedValue)
        {
            _Int16 column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int16?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_cast_from_Int32Column()
        {
            TestInt32ColumnCast(null, null);
            TestInt32ColumnCast(5, 5);
        }

        private void TestInt32ColumnCast(Int32? x, Byte? expectedValue)
        {
            _Int32 column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int32?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_cast_from_Int64Column()
        {
            TestInt64ColumnCast(8, 8);
            TestInt64ColumnCast(null, null);
        }

        private void TestInt64ColumnCast(Int64? x, Byte? expectedValue)
        {
            _Int64 column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int64?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_cast_from_DecimalColumn()
        {
            TestDecimalColumnCast(8, 8);
            TestDecimalColumnCast(null, null);
        }

        private void TestDecimalColumnCast(Decimal? x, Byte? expectedValue)
        {
            _Decimal column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Decimal?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_cast_from_DoubleColumn()
        {
            TestDoubleColumnCast(8, 8);
            TestDoubleColumnCast(null, null);
        }

        private void TestDoubleColumnCast(Double? x, Byte? expectedValue)
        {
            _Double column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Double?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_cast_from_SingleColumn()
        {
            TestSingleColumnCast(8, 8);
            TestSingleColumnCast(null, null);
        }

        private void TestSingleColumnCast(Single? x, Byte? expectedValue)
        {
            _Single column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Single?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void ByteColumn_cast_from_StringColumn()
        {
            TestStringColumnCast("8", 8);
            TestStringColumnCast(null, null);
        }

        private void TestStringColumnCast(String x, Byte? expectedValue)
        {
            _String column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }
    }
}

using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _Int32Tests
    {
        [TestMethod]
        public void Int32Column_Param()
        {
            TestParam(5);
            TestParam(null);
        }

        private void TestParam(Int32? x)
        {
            var column = _Int32.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void Int32Column_implicit_convert()
        {
            TestImplicit(5);
            TestImplicit(null);
        }

        private void TestImplicit(Int32? x)
        {
            _Int32 column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void Int32Column_Const()
        {
            TestConst(5);
            TestConst(null);
        }

        private void TestConst(Int32? x)
        {
            _Int32 column = _Int32.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void Int32Column_negate()
        {
            TestNegate(2, -2);
            TestNegate(null, null);
        }

        private void TestNegate(Int32? x, Int32? expectedValue)
        {
            _Int32 column = x;
            var expr = -column;
            var dbExpr = (DbUnaryExpression)expr.DbExpression;
            dbExpr.Verify(DbUnaryExpressionKind.Negate, column);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_ones_complement()
        {
            TestOnesComplement(5, ~5);
            TestOnesComplement(null, null);
        }

        private void TestOnesComplement(Int32? x, Int32? expectedValue)
        {
            _Int32 column = x;
            var expr = ~column;
            var dbExpr = (DbUnaryExpression)expr.DbExpression;
            dbExpr.Verify(DbUnaryExpressionKind.OnesComplement, column);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_add()
        {
            TestAdd(1, 2, 3);
            TestAdd(1, null, null);
            TestAdd(null, null, null);
        }

        private void TestAdd(Int32? x, Int32? y, Int32? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 + column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Add, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_substract()
        {
            TestSubstract(5, 2, 3);
            TestSubstract(null, 2, null);
            TestSubstract(2, null, null);
        }

        private void TestSubstract(Int32? x, Int32? y, Int32? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 - column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Substract, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_multipy()
        {
            TestMultiply(5, 5, 25);
            TestMultiply(5, null, null);
            TestMultiply(null, null, null);
        }

        private void TestMultiply(Int32? x, Int32? y, Int32? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 * column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Multiply, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_divide()
        {
            TestDivide(12, 4, 3);
            TestDivide(5, null, null);
            TestDivide(null, 5, null);
            TestDivide(null, null, null);
        }

        private void TestDivide(Int32? x, Int32? y, Int32? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 / column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Divide, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_modulo()
        {
            TestModulo(5, 3, 2);
            TestModulo(100, null, null);
            TestModulo(null, null, null);
        }

        private void TestModulo(Int32? x, Int32? y, Int32? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 % column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Modulo, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_bitwise_AND()
        {
            TestBitwiseAnd(102, 55, 102 & 55);
            TestBitwiseAnd(100, null, null);
            TestBitwiseAnd(null, null, null);
        }

        private void TestBitwiseAnd(Int32? x, Int32? y, Int32? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 & column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseAnd, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_bitwise_OR()
        {
            TestBitwiseOr(5, 12, 5 | 12);
            TestBitwiseOr(5, null, null);
            TestBitwiseOr(null, null, null);
        }

        private void TestBitwiseOr(Int32? x, Int32? y, Int32? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 | column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseOr, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_bitwise_XOR()
        {
            TestBitwiseXor(1024, 2048, 1024 ^ 2048);
            TestBitwiseXor(1024, null, null);
            TestBitwiseXor(null, 1024, null);
            TestBitwiseXor(null, null, null);
        }

        private void TestBitwiseXor(Int32? x, Int32? y, Int32? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 ^ column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseXor, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_less_than()
        {
            TestLessThan(99, 100, true);
            TestLessThan(99, 99, false);
            TestLessThan(99, 98, false);
            TestLessThan(5, null, null);
            TestLessThan(null, 5, null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(Int32? x, Int32? y, bool? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_less_than_or_equal()
        {
            TestLessThanOrEqual(99, 98, false);
            TestLessThanOrEqual(99, 99, true);
            TestLessThanOrEqual(99, 100, true);
            TestLessThanOrEqual(5, null, null);
            TestLessThanOrEqual(null, 5, null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(Int32? x, Int32? y, bool? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_greater_than()
        {
            TestGreaterThan(100, 99, true);
            TestGreaterThan(100, 100, false);
            TestGreaterThan(99, 100, false);
            TestGreaterThan(5, null, null);
            TestGreaterThan(null, 5, null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(Int32? x, Int32? y, bool? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_greater_than_or_equal()
        {
            TestGreaterThanOrEqual(100, 99, true);
            TestGreaterThanOrEqual(100, 100, true);
            TestGreaterThanOrEqual(99, 100, false);
            TestGreaterThanOrEqual(5, null, null);
            TestGreaterThanOrEqual(null, 5, null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(Int32? x, Int32? y, bool? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_equal()
        {
            TestEqual(2, 2, true);
            TestEqual(4, 5, false);
            TestEqual(1, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(Int32? x, Int32? y, bool? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_not_equal()
        {
            TestNotEqual(1, 1, false);
            TestNotEqual(1, 2, true);
            TestNotEqual(1, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(Int32? x, Int32? y, bool? expectedValue)
        {
            _Int32 column1 = x;
            _Int32 column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_cast_from_BooleanColumn()
        {
            TestBooleanColumnCast(_Boolean.True, 1);
            TestBooleanColumnCast(_Boolean.False, 0);
            TestBooleanColumnCast(_Boolean.Null, null);
        }

        private void TestBooleanColumnCast(_Boolean x, Int32? expectedValue)
        {
            _Int32 expr = (_Int32)x;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(x, typeof(bool?), typeof(Int32?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_cast_from_ByteColumn()
        {
            TestByteColumnCast(null, null);
            TestByteColumnCast(127, 127);
        }

        private void TestByteColumnCast(byte? x, Int32? expectedValue)
        {
            _Byte column1 = x;
            _Int32 expr = column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(byte?), typeof(Int32?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_cast_from_Int16Column()
        {
            TestInt16ColumnCast(null, null);
            TestInt16ColumnCast(5, 5);
        }

        private void TestInt16ColumnCast(Int16? x, Int32? expectedValue)
        {
            _Int16 column1 = x;
            _Int32 expr = column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int16?), typeof(Int32?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_cast_from_Int64Column()
        {
            TestInt64ColumnCast(8, 8);
            TestInt64ColumnCast(null, null);
        }

        private void TestInt64ColumnCast(Int64? x, Int32? expectedValue)
        {
            _Int64 column1 = x;
            _Int32 expr = (_Int32)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int64?), typeof(Int32?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_cast_from_DecimalColumn()
        {
            TestDecimalColumnCast(8, 8);
            TestDecimalColumnCast(null, null);
        }

        private void TestDecimalColumnCast(Decimal? x, Int32? expectedValue)
        {
            _Decimal column1 = x;
            _Int32 expr = (_Int32)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Decimal?), typeof(Int32?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_cast_from_DoubleColumn()
        {
            TestDoubleColumnCast(8, 8);
            TestDoubleColumnCast(null, null);
        }

        private void TestDoubleColumnCast(Double? x, Int32? expectedValue)
        {
            _Double column1 = x;
            _Int32 expr = (_Int32)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Double?), typeof(Int32?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_cast_from_SingleColumn()
        {
            TestSingleColumnCast(8, 8);
            TestSingleColumnCast(null, null);
        }

        private void TestSingleColumnCast(Single? x, Int32? expectedValue)
        {
            _Single column1 = x;
            _Int32 expr = (_Int32)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Single?), typeof(Int32?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int32Column_cast_from_StringColumn()
        {
            TestStringColumnCast("8", 8);
            TestStringColumnCast(null, null);
        }

        private void TestStringColumnCast(String x, Int32? expectedValue)
        {
            _String column1 = x;
            _Int32 expr = (_Int32)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(Int32?));
            expr.VerifyEval(expectedValue);
        }
    }
}

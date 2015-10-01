using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _Int16Tests
    {
        [TestMethod]
        public void Int16Column_Param()
        {
            TestParam(25);
            TestParam(null);
        }

        private void TestParam(Int16? x)
        {
            var column = _Int16.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void Int16Column_implicit_convert()
        {
            TestImplicit(25);
            TestImplicit(null);
        }

        private void TestImplicit(Int16? x)
        {
            _Int16 column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void Int16Column_Const()
        {
            TestConst(25);
            TestConst(null);
        }

        private void TestConst(Int16? x)
        {
            _Int16 column = _Int16.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void Int16Column_negate()
        {
            TestNegate(2, -2);
            TestNegate(null, null);
        }

        private void TestNegate(Int16? x, Int16? expectedValue)
        {
            _Int16 column = x;
            var expr = -column;
            var dbExpr = (DbUnaryExpression)expr.DbExpression;
            dbExpr.Verify(DbUnaryExpressionKind.Negate, column);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_ones_complement()
        {
            TestOnesComplement(5, ~5);
            TestOnesComplement(null, null);
        }

        private void TestOnesComplement(Int16? x, Int16? expectedValue)
        {
            _Int16 column = x;
            var expr = ~column;
            var dbExpr = (DbUnaryExpression)expr.DbExpression;
            dbExpr.Verify(DbUnaryExpressionKind.OnesComplement, column);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_add()
        {
            TestAdd(1, 2, 3);
            TestAdd(1, null, null);
            TestAdd(null, null, null);
        }

        private void TestAdd(Int16? x, Int16? y, Int16? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 + column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Add, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_substract()
        {
            TestSubstract(5, 2, 3);
            TestSubstract(null, 2, null);
            TestSubstract(2, null, null);
        }

        private void TestSubstract(Int16? x, Int16? y, Int16? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 - column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Substract, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_multipy()
        {
            TestMultiply(5, 5, 25);
            TestMultiply(5, null, null);
            TestMultiply(null, null, null);
        }

        private void TestMultiply(Int16? x, Int16? y, Int16? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 * column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Multiply, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_divide()
        {
            TestDivide(12, 4, 3);
            TestDivide(5, null, null);
            TestDivide(null, 5, null);
            TestDivide(null, null, null);
        }

        private void TestDivide(Int16? x, Int16? y, Int16? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 / column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Divide, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_modulo()
        {
            TestModulo(5, 3, 2);
            TestModulo(100, null, null);
            TestModulo(null, null, null);
        }

        private void TestModulo(Int16? x, Int16? y, Int16? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 % column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Modulo, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_bitwise_AND()
        {
            TestBitwiseAnd(102, 55, 102 & 55);
            TestBitwiseAnd(100, null, null);
            TestBitwiseAnd(null, null, null);
        }

        private void TestBitwiseAnd(Int16? x, Int16? y, Int16? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 & column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseAnd, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_bitwise_OR()
        {
            TestBitwiseOr(5, 12, 5 | 12);
            TestBitwiseOr(5, null, null);
            TestBitwiseOr(null, null, null);
        }

        private void TestBitwiseOr(Int16? x, Int16? y, Int16? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 | column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseOr, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_bitwise_XOR()
        {
            TestBitwiseXor(1024, 2048, 1024 ^ 2048);
            TestBitwiseXor(1024, null, null);
            TestBitwiseXor(null, 1024, null);
            TestBitwiseXor(null, null, null);
        }

        private void TestBitwiseXor(Int16? x, Int16? y, Int16? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 ^ column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.BitwiseXor, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_less_than()
        {
            TestLessThan(99, 100, true);
            TestLessThan(99, 99, false);
            TestLessThan(99, 98, false);
            TestLessThan(5, null, null);
            TestLessThan(null, 5, null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(Int16? x, Int16? y, bool? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_less_than_or_equal()
        {
            TestLessThanOrEqual(99, 98, false);
            TestLessThanOrEqual(99, 99, true);
            TestLessThanOrEqual(99, 100, true);
            TestLessThanOrEqual(5, null, null);
            TestLessThanOrEqual(null, 5, null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(Int16? x, Int16? y, bool? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_greater_than()
        {
            TestGreaterThan(100, 99, true);
            TestGreaterThan(100, 100, false);
            TestGreaterThan(99, 100, false);
            TestGreaterThan(5, null, null);
            TestGreaterThan(null, 5, null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(Int16? x, Int16? y, bool? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_greater_than_or_equal()
        {
            TestGreaterThanOrEqual(100, 99, true);
            TestGreaterThanOrEqual(100, 100, true);
            TestGreaterThanOrEqual(99, 100, false);
            TestGreaterThanOrEqual(5, null, null);
            TestGreaterThanOrEqual(null, 5, null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(Int16? x, Int16? y, bool? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_equal()
        {
            TestEqual(2, 2, true);
            TestEqual(4, 5, false);
            TestEqual(1, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(Int16? x, Int16? y, bool? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_not_equal()
        {
            TestNotEqual(1, 1, false);
            TestNotEqual(1, 2, true);
            TestNotEqual(1, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(Int16? x, Int16? y, bool? expectedValue)
        {
            _Int16 column1 = x;
            _Int16 column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_cast_from_BooleanColumn()
        {
            TestBooleanColumnCast(_Boolean.True, 1);
            TestBooleanColumnCast(_Boolean.False, 0);
            TestBooleanColumnCast(_Boolean.Null, null);
        }

        private void TestBooleanColumnCast(_Boolean x, Int16? expectedValue)
        {
            _Int16 expr = (_Int16)x;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(x, typeof(bool?), typeof(Int16?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_cast_from_ByteColumn()
        {
            TestByteColumnCast(null, null);
            TestByteColumnCast(127, 127);
        }

        private void TestByteColumnCast(byte? x, Int16? expectedValue)
        {
            _Byte column1 = x;
            _Int16 expr = column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(byte?), typeof(Int16?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_cast_from_Int32Column()
        {
            TestInt32ColumnCast(null, null);
            TestInt32ColumnCast(5, 5);
        }

        private void TestInt32ColumnCast(Int32? x, Int16? expectedValue)
        {
            _Int32 column1 = x;
            _Int16 expr = (_Int16)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int32?), typeof(Int16?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_cast_from_Int64Column()
        {
            TestInt64ColumnCast(8, 8);
            TestInt64ColumnCast(null, null);
        }

        private void TestInt64ColumnCast(Int64? x, Int16? expectedValue)
        {
            _Int64 column1 = x;
            _Int16 expr = (_Int16)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int64?), typeof(Int16?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_cast_from_DecimalColumn()
        {
            TestDecimalColumnCast(8, 8);
            TestDecimalColumnCast(null, null);
        }

        private void TestDecimalColumnCast(Decimal? x, Int16? expectedValue)
        {
            _Decimal column1 = x;
            _Int16 expr = (_Int16)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Decimal?), typeof(Int16?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_cast_from_DoubleColumn()
        {
            TestDoubleColumnCast(8, 8);
            TestDoubleColumnCast(null, null);
        }

        private void TestDoubleColumnCast(Double? x, Int16? expectedValue)
        {
            _Double column1 = x;
            _Int16 expr = (_Int16)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Double?), typeof(Int16?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_cast_from_SingleColumn()
        {
            TestSingleColumnCast(8, 8);
            TestSingleColumnCast(null, null);
        }

        private void TestSingleColumnCast(Single? x, Int16? expectedValue)
        {
            _Single column1 = x;
            _Int16 expr = (_Int16)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Single?), typeof(Int16?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void Int16Column_cast_from_StringColumn()
        {
            TestStringColumnCast("8", 8);
            TestStringColumnCast(null, null);
        }

        private void TestStringColumnCast(String x, Int16? expectedValue)
        {
            _String column1 = x;
            _Int16 expr = (_Int16)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(Int16?));
            expr.VerifyEval(expectedValue);
        }
    }
}

using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using DevZest.Data.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _ByteTests : ColumnConverterTestsBase
    {
        [TestMethod]
        public void _Byte_Param()
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
        public void _Byte_implicit_convert()
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
        public void _Byte_Const()
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
        public void _Byte_OnesComplement()
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
        public void _Byte_Add()
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
        public void _Byte_Substract()
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
        public void _Byte_Multiply()
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
        public void _Byte_Divide()
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
        public void _Byte_Modulo()
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
        public void _Byte_BitwiseAnd()
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
        public void _Byte_BitwiseOr()
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
        public void _Byte_BitwiseXor()
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
        public void _Byte_LessThan()
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
        public void _Byte_LessThanOrEqual()
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
        public void _Byte_GreaterThan()
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
        public void _Byte_GreaterThanOrEqual()
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
        public void _Byte_Equal()
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
        public void _Byte_NotEqual()
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
        public void _Byte_FromBoolean()
        {
            TestFromBoolean(_Boolean.True, 1);
            TestFromBoolean(_Boolean.False, 0);
            TestFromBoolean(_Boolean.Null, null);
        }

        private void TestFromBoolean(_Boolean x, Byte? expectedValue)
        {
            _Byte expr = (_Byte)x;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(x, typeof(bool?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Byte_FromInt16()
        {
            TestFromInt16(null, null);
            TestFromInt16(127, 127);
            TestFromInt16(1024, 0);
        }

        private void TestFromInt16(Int16? x, Byte? expectedValue)
        {
            _Int16 column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int16?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Byte_FromInt32()
        {
            TestFromInt32(null, null);
            TestFromInt32(5, 5);
        }

        private void TestFromInt32(Int32? x, Byte? expectedValue)
        {
            _Int32 column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int32?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Byte_FromInt64()
        {
            TestFromInt64(8, 8);
            TestFromInt64(null, null);
        }

        private void TestFromInt64(Int64? x, Byte? expectedValue)
        {
            _Int64 column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int64?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Byte_FromDecimal()
        {
            TestFromDecimal(8, 8);
            TestFromDecimal(null, null);
        }

        private void TestFromDecimal(Decimal? x, Byte? expectedValue)
        {
            _Decimal column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Decimal?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _ByteColumn_FromDouble()
        {
            TestFromDouble(8, 8);
            TestFromDouble(null, null);
        }

        private void TestFromDouble(Double? x, Byte? expectedValue)
        {
            _Double column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Double?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Byte_FromSingle()
        {
            TestFromSingle(8, 8);
            TestFromSingle(null, null);
        }

        private void TestFromSingle(Single? x, Byte? expectedValue)
        {
            _Single column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Single?), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Byte_FromString()
        {
            TestFromString("8", 8);
            TestFromString(null, null);
        }

        private void TestFromString(String x, Byte? expectedValue)
        {
            _String column1 = x;
            _Byte expr = (_Byte)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(Byte?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Byte_Add_Converter()
        {
            var column = _Byte.Const(1) + _Byte.Const(2);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_Add, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_BitwiseAnd_Converter()
        {
            var column = _Byte.Const(1) & _Byte.Const(0);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_BitwiseAnd, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)0, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_BitwiseOr_Converter()
        {
            var column = _Byte.Const(1) | _Byte.Const(0);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_BitwiseOr, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_BitwiseXor_Converter()
        {
            var column = _Byte.Const(1) ^ _Byte.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_BitwiseXor, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)0, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_Divide_Converter()
        {
            var column = _Byte.Const(12) / _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_Divide, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)4, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_Equal_Converter()
        {
            var column = _Byte.Const(1) == _Byte.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_FromBoolean_Converter()
        {
            var column = (_Byte)_Boolean.True;
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromBoolean, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_FromDecimal_Converter()
        {
            var column = (_Byte)_Decimal.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromDecimal, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_FromDouble_Converter()
        {
            var column = (_Byte)_Double.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromDouble, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_FromInt16_Converter()
        {
            var column = (_Byte)_Int16.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromInt16, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_FromInt32_Converter()
        {
            var column = (_Byte)_Int32.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromInt32, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_FromInt64_Converter()
        {
            var column = (_Byte)_Int64.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromInt64, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_FromSingle_Converter()
        {
            var column = (_Byte)_Single.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromSingle, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_FromString_Converter()
        {
            var column = (_Byte)_String.Const("5");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromString, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_GreaterThan_Converter()
        {
            var column = _Byte.Const(3) > _Byte.Const(2);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_GreaterThanOrEqual_Converter()
        {
            var column = _Byte.Const(3) >= _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_LessThan_Converter()
        {
            var column = _Byte.Const(3) < _Byte.Const(2);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(false, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_LessThanOrEqual_Converter()
        {
            var column = _Byte.Const(3) <= _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_Modulo_Converter()
        {
            var column = _Byte.Const(5) % _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_Modulo, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_Multiply_Converter()
        {
            var column = _Byte.Const(5) * _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_Multiply, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)15, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_NotEqual_Converter()
        {
            var column = _Byte.Const(5) != _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_OnesComplement_Converter()
        {
            var column = ~_Byte.Const(0);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_OnesComplement, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)255, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_Substract_Converter()
        {
            var column = _Byte.Const(5) - _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_Substract, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Byte_CastToString()
        {
            TestCastToString(null, null);
            TestCastToString(127, "127");
        }

        private void TestCastToString(byte? x, String expectedValue)
        {
            _Byte column1 = x;
            _String expr = column1.CastToString();
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(byte?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Byte_CastToString_Converter()
        {
            var column = _Byte.Const(1).CastToString();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_CastToString, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("1", columnFromJson.Eval());
        }
    }
}

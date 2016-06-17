using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using DevZest.Data.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _DecimalTests : ColumnConverterTestsBase
    {
        [TestMethod]
        public void _Decimal_Param()
        {
            TestParam(5.5m);
            TestParam(null);
        }

        private static void TestParam(Decimal? x)
        {
            var column = _Decimal.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Decimal_Implicit()
        {
            TestImplicit(5.5m);
            TestImplicit(null);
        }

        private static void TestImplicit(Decimal? x)
        {
            _Decimal column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Decimal_Const()
        {
            TestConst(5.5m);
            TestConst(null);
        }

        private static void TestConst(Decimal? x)
        {
            _Decimal column = _Decimal.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _Decimal_Negate()
        {
            TestNegate(2, -2);
            TestNegate(null, null);
        }

        private void TestNegate(Decimal? x, Decimal? expectedValue)
        {
            _Decimal column = x;
            var expr = -column;
            var dbExpr = (DbUnaryExpression)expr.DbExpression;
            dbExpr.Verify(DbUnaryExpressionKind.Negate, column);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_Add()
        {
            TestAdd(1, 2, 3);
            TestAdd(1, null, null);
            TestAdd(null, null, null);
        }

        private void TestAdd(Decimal? x, Decimal? y, Decimal? expectedValue)
        {
            _Decimal column1 = x;
            _Decimal column2 = y;
            var expr = column1 + column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Add, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_Substract()
        {
            TestSubstract(5, 2, 3);
            TestSubstract(null, 2, null);
            TestSubstract(2, null, null);
        }

        private void TestSubstract(Decimal? x, Decimal? y, Decimal? expectedValue)
        {
            _Decimal column1 = x;
            _Decimal column2 = y;
            var expr = column1 - column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Substract, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_Multiply()
        {
            TestMultiply(5, 5, 25);
            TestMultiply(5, null, null);
            TestMultiply(null, null, null);
        }

        private void TestMultiply(Decimal? x, Decimal? y, Decimal? expectedValue)
        {
            _Decimal column1 = x;
            _Decimal column2 = y;
            var expr = column1 * column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Multiply, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_Divide()
        {
            TestDivide(12, 4, 3);
            TestDivide(5, null, null);
            TestDivide(null, 5, null);
            TestDivide(null, null, null);
        }

        private void TestDivide(Decimal? x, Decimal? y, Decimal? expectedValue)
        {
            _Decimal column1 = x;
            _Decimal column2 = y;
            var expr = column1 / column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Divide, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_Modulo()
        {
            TestModulo(5, 3, 2);
            TestModulo(100, null, null);
            TestModulo(null, null, null);
        }

        private void TestModulo(Decimal? x, Decimal? y, Decimal? expectedValue)
        {
            _Decimal column1 = x;
            _Decimal column2 = y;
            var expr = column1 % column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Modulo, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_LessThan()
        {
            TestLessThan(99, 100, true);
            TestLessThan(99, 99, false);
            TestLessThan(99, 98, false);
            TestLessThan(5, null, null);
            TestLessThan(null, 5, null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(Decimal? x, Decimal? y, bool? expectedValue)
        {
            _Decimal column1 = x;
            _Decimal column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_LessThanOrEqual()
        {
            TestLessThanOrEqual(99, 98, false);
            TestLessThanOrEqual(99, 99, true);
            TestLessThanOrEqual(99, 100, true);
            TestLessThanOrEqual(5, null, null);
            TestLessThanOrEqual(null, 5, null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(Decimal? x, Decimal? y, bool? expectedValue)
        {
            _Decimal column1 = x;
            _Decimal column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_GreaterThan()
        {
            TestGreaterThan(100, 99, true);
            TestGreaterThan(100, 100, false);
            TestGreaterThan(99, 100, false);
            TestGreaterThan(5, null, null);
            TestGreaterThan(null, 5, null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(Decimal? x, Decimal? y, bool? expectedValue)
        {
            _Decimal column1 = x;
            _Decimal column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_GreaterThanOrEqual()
        {
            TestGreaterThanOrEqual(100, 99, true);
            TestGreaterThanOrEqual(100, 100, true);
            TestGreaterThanOrEqual(99, 100, false);
            TestGreaterThanOrEqual(5, null, null);
            TestGreaterThanOrEqual(null, 5, null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(Decimal? x, Decimal? y, bool? expectedValue)
        {
            _Decimal column1 = x;
            _Decimal column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_Equal()
        {
            TestEqual(2, 2, true);
            TestEqual(4, 5, false);
            TestEqual(1, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(Decimal? x, Decimal? y, bool? expectedValue)
        {
            _Decimal column1 = x;
            _Decimal column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_NotEqual()
        {
            TestNotEqual(1, 1, false);
            TestNotEqual(1, 2, true);
            TestNotEqual(1, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(Decimal? x, Decimal? y, bool? expectedValue)
        {
            _Decimal column1 = x;
            _Decimal column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_FromBoolean()
        {
            TestFromBoolean(_Boolean.True, 1);
            TestFromBoolean(_Boolean.False, 0);
            TestFromBoolean(_Boolean.Null, null);
        }

        private void TestFromBoolean(_Boolean x, Decimal? expectedValue)
        {
            _Decimal expr = (_Decimal)x;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(x, typeof(bool?), typeof(Decimal?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_FromByte()
        {
            TestFromByte(null, null);
            TestFromByte(127, 127);
        }

        private void TestFromByte(byte? x, Decimal? expectedValue)
        {
            _Byte column1 = x;
            _Decimal expr = (_Decimal)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(byte?), typeof(Decimal?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_FromInt16()
        {
            TestFromInt16(null, null);
            TestFromInt16(5, 5);
        }

        private void TestFromInt16(Int16? x, Decimal? expectedValue)
        {
            _Int16 column1 = x;
            _Decimal expr = (_Decimal)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int16?), typeof(Decimal?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_FromInt64()
        {
            TestFromInt64(8, 8);
            TestFromInt64(null, null);
        }

        private void TestFromInt64(Int64? x, Decimal? expectedValue)
        {
            _Int64 column1 = x;
            _Decimal expr = (_Decimal)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int64?), typeof(Decimal?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_FromInt32()
        {
            TestFromInt32(8, 8);
            TestFromInt32(null, null);
        }

        private void TestFromInt32(Int32? x, Decimal? expectedValue)
        {
            _Int32 column1 = x;
            _Decimal expr = (_Decimal)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int32?), typeof(Decimal?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_FromDouble()
        {
            TestFromDouble(8, 8);
            TestFromDouble(null, null);
        }

        private void TestFromDouble(Double? x, Decimal? expectedValue)
        {
            _Double column1 = x;
            _Decimal expr = (_Decimal)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Double?), typeof(Decimal?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_FromSingle()
        {
            TestFromSingle(8, 8);
            TestFromSingle(null, null);
        }

        private void TestFromSingle(Single? x, Decimal? expectedValue)
        {
            _Single column1 = x;
            _Decimal expr = (_Decimal)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Single?), typeof(Decimal?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _DecimalColumn_FromString()
        {
            TestFromString("8", 8);
            TestFromString(null, null);
        }

        private void TestFromString(String x, Decimal? expectedValue)
        {
            _String column1 = x;
            _Decimal expr = (_Decimal)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(Decimal?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_Add_Converter()
        {
            var column = _Decimal.Const(1) + _Decimal.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Add, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_Divide_Converter()
        {
            var column = _Decimal.Const(15) / _Decimal.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Divide, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_Equal_Converter()
        {
            var column = _Decimal.Const(5) == _Decimal.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_FromBoolean_Converter()
        {
            var column = (_Decimal)_Boolean.True;
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromBoolean, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_FromByte_Converter()
        {
            var column = (_Decimal)_Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromByte, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_FromDouble_Converter()
        {
            var column = (_Decimal)_Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromDouble, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_FromInt16_Converter()
        {
            var column = (_Decimal)_Int16.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromInt16, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_FromInt32_Converter()
        {
            var column = (_Decimal)_Int32.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromInt32, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_FromInt64_Converter()
        {
            var column = (_Decimal)_Int64.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromInt64, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_FromSingle_Converter()
        {
            var column = (_Decimal)_Single.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromSingle, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_FromString_Converter()
        {
            var column = (_Decimal)_String.Const("3");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromString, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_GreaterThan_Converter()
        {
            var column = _Decimal.Const(4) > _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_GreaterThanOrEqual_Converter()
        {
            var column = _Decimal.Const(3) >= _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_LessThan_Converter()
        {
            var column = _Decimal.Const(3) < _Decimal.Const(4);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_LessThanOrEqual_Converter()
        {
            var column = _Decimal.Const(3) <= _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_Modulo_Converter()
        {
            var column = _Decimal.Const(5) % _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Modulo, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_Multiply_Converter()
        {
            var column = _Decimal.Const(5) * _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Multiply, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)15, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_Negate_Converter()
        {
            var column = -_Decimal.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Negate, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)(-5), columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_NotEqual_Converter()
        {
            var column = _Decimal.Const(2) != _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_Substract_Converter()
        {
            var column = _Decimal.Const(5) - _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Substract, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Decimal_CastToString()
        {
            TestCastToString(8, "8");
            TestCastToString(null, null);
        }

        private void TestCastToString(Decimal? x, String expectedValue)
        {
            _Decimal column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Decimal?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Decimal_CastToString_Converter()
        {
            var column = (_String)_Decimal.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_CastToString, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("1", columnFromJson.Eval());
        }
    }
}

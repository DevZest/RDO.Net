using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using DevZest.Data.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _DoubleTests : ColumnConverterTestsBase
    {
        [TestMethod]
        public void _Double_Param()
        {
            TestParam(5.5);
            TestParam(null);
        }

        private void TestParam(Double? x)
        {
            var column = _Double.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Double_Implicit()
        {
            TestImplicit(5.5);
            TestImplicit(null);
        }

        private void TestImplicit(Double? x)
        {
            _Double column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Double_Const()
        {
            TestConst(5.5);
            TestConst(null);
        }

        private void TestConst(Double? x)
        {
            _Double column = _Double.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _Double_Negate()
        {
            TestNegate(2, -2);
            TestNegate(null, null);
        }

        private void TestNegate(Double? x, Double? expectedValue)
        {
            _Double column = x;
            var expr = -column;
            var dbExpr = (DbUnaryExpression)expr.DbExpression;
            dbExpr.Verify(DbUnaryExpressionKind.Negate, column);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_Add()
        {
            TestAdd(1, 2, 3);
            TestAdd(1, null, null);
            TestAdd(null, null, null);
        }

        private void TestAdd(Double? x, Double? y, Double? expectedValue)
        {
            _Double column1 = x;
            _Double column2 = y;
            var expr = column1 + column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Add, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_Substract()
        {
            TestSubstract(5, 2, 3);
            TestSubstract(null, 2, null);
            TestSubstract(2, null, null);
        }

        private void TestSubstract(Double? x, Double? y, Double? expectedValue)
        {
            _Double column1 = x;
            _Double column2 = y;
            var expr = column1 - column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Substract, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_Multiply()
        {
            TestMultiply(5, 5, 25);
            TestMultiply(5, null, null);
            TestMultiply(null, null, null);
        }

        private void TestMultiply(Double? x, Double? y, Double? expectedValue)
        {
            _Double column1 = x;
            _Double column2 = y;
            var expr = column1 * column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Multiply, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_Divide()
        {
            TestDivide(12, 4, 3);
            TestDivide(5, null, null);
            TestDivide(null, 5, null);
            TestDivide(null, null, null);
        }

        private void TestDivide(Double? x, Double? y, Double? expectedValue)
        {
            _Double column1 = x;
            _Double column2 = y;
            var expr = column1 / column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Divide, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_Modulo()
        {
            TestModulo(5, 3, 2);
            TestModulo(100, null, null);
            TestModulo(null, null, null);
        }

        private void TestModulo(Double? x, Double? y, Double? expectedValue)
        {
            _Double column1 = x;
            _Double column2 = y;
            var expr = column1 % column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Modulo, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_LessThan()
        {
            TestLessThan(99, 100, true);
            TestLessThan(99, 99, false);
            TestLessThan(99, 98, false);
            TestLessThan(5, null, null);
            TestLessThan(null, 5, null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(Double? x, Double? y, bool? expectedValue)
        {
            _Double column1 = x;
            _Double column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_LessThanOrEqual()
        {
            TestLessThanOrEqual(99, 98, false);
            TestLessThanOrEqual(99, 99, true);
            TestLessThanOrEqual(99, 100, true);
            TestLessThanOrEqual(5, null, null);
            TestLessThanOrEqual(null, 5, null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(Double? x, Double? y, bool? expectedValue)
        {
            _Double column1 = x;
            _Double column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_GreaterThan()
        {
            TestGreaterThan(100, 99, true);
            TestGreaterThan(100, 100, false);
            TestGreaterThan(99, 100, false);
            TestGreaterThan(5, null, null);
            TestGreaterThan(null, 5, null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(Double? x, Double? y, bool? expectedValue)
        {
            _Double column1 = x;
            _Double column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_GreaterThanOrEqual()
        {
            TestGreaterThanOrEqual(100, 99, true);
            TestGreaterThanOrEqual(100, 100, true);
            TestGreaterThanOrEqual(99, 100, false);
            TestGreaterThanOrEqual(5, null, null);
            TestGreaterThanOrEqual(null, 5, null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(Double? x, Double? y, bool? expectedValue)
        {
            _Double column1 = x;
            _Double column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_Equal()
        {
            TestEqual(2, 2, true);
            TestEqual(4, 5, false);
            TestEqual(1, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(Double? x, Double? y, bool? expectedValue)
        {
            _Double column1 = x;
            _Double column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_NotEqual()
        {
            TestNotEqual(1, 1, false);
            TestNotEqual(1, 2, true);
            TestNotEqual(1, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(Double? x, Double? y, bool? expectedValue)
        {
            _Double column1 = x;
            _Double column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_FromBoolean()
        {
            TestFromBoolean(_Boolean.True, 1);
            TestFromBoolean(_Boolean.False, 0);
            TestFromBoolean(_Boolean.Null, null);
        }

        private void TestFromBoolean(_Boolean x, Double? expectedValue)
        {
            _Double expr = (_Double)x;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(x, typeof(bool?), typeof(Double?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_FromByte()
        {
            TestFromByte(null, null);
            TestFromByte(127, 127);
        }

        private void TestFromByte(byte? x, Double? expectedValue)
        {
            _Byte column1 = x;
            _Double expr = (_Double)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(byte?), typeof(Double?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_FromInt16()
        {
            TestFromInt16(null, null);
            TestFromInt16(5, 5);
        }

        private void TestFromInt16(Int16? x, Double? expectedValue)
        {
            _Int16 column1 = x;
            _Double expr = (_Double)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int16?), typeof(Double?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_FromInt64()
        {
            TestFromInt64(8, 8);
            TestFromInt64(null, null);
        }

        private void TestFromInt64(Int64? x, Double? expectedValue)
        {
            _Int64 column1 = x;
            _Double expr = (_Double)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int64?), typeof(Double?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_FromInt32()
        {
            TestFromInt32(8, 8);
            TestFromInt32(null, null);
        }

        private void TestFromInt32(Int32? x, Double? expectedValue)
        {
            _Int32 column1 = x;
            _Double expr = (_Double)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int32?), typeof(Double?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_FromDecimal()
        {
            TestFromDecimal(8, 8);
            TestFromDecimal(null, null);
        }

        private void TestFromDecimal(Decimal? x, Double? expectedValue)
        {
            _Decimal column1 = x;
            _Double expr = (_Double)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Decimal?), typeof(Double?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_FromSingle()
        {
            TestFromSingle(8, 8);
            TestFromSingle(null, null);
        }

        private void TestFromSingle(Single? x, Double? expectedValue)
        {
            _Single column1 = x;
            _Double expr = (_Double)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Single?), typeof(Double?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_FromString()
        {
            TestFromString("8", 8);
            TestFromString(null, null);
        }

        private void TestFromString(String x, Double? expectedValue)
        {
            _String column1 = x;
            _Double expr = (_Double)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(Double?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_Add_Converter()
        {
            var column = _Double.Const(1) + _Double.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Add, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_Divide_Converter()
        {
            var column = _Double.Const(6) / _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Divide, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_Equal_Converter()
        {
            var column = _Double.Const(1) == _Double.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_FromBoolean_Converter()
        {
            var column = (_Double)_Boolean.True;
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromBoolean, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_FromByte_Converter()
        {
            var column = (_Double)_Byte.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromByte, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_FromDecimal_Converter()
        {
            var column = (_Double)_Decimal.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromDecimal, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_FromInt16_Converter()
        {
            var column = (_Double)_Int16.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromInt16, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_FromInt32_Converter()
        {
            var column = (_Double)_Int32.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromInt32, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_FromInt64_Converter()
        {
            var column = (_Double)_Int64.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromInt64, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_FromSingle_Converter()
        {
            var column = (_Double)_Single.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromSingle, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_FromString_Converter()
        {
            var column = (_Double)_String.Const("1");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromString, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_GreaterThan_Converter()
        {
            var column = _Double.Const(4) > _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_GreaterThanOrEqual_Converter()
        {
            var column = _Double.Const(3) >= _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_LessThan_Converter()
        {
            var column = _Double.Const(3) < _Double.Const(4);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_LessThanOrEqual_Converter()
        {
            var column = _Double.Const(3) <= _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_Modulo_Converter()
        {
            var column = _Double.Const(5) % _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Modulo, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_Multiply_Converter()
        {
            var column = _Double.Const(5) * _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Multiply, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)15, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_Negate_Converter()
        {
            var column = -_Double.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Negate, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)(-5), columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_NotEqual_Converter()
        {
            var column = _Double.Const(2) != _Double.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_Substract_Converter()
        {
            var column = _Double.Const(5) - _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Substract, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Double_CastToString()
        {
            TestCastToString(8, "8");
            TestCastToString(null, null);
        }

        private void TestCastToString(Double? x, String expectedValue)
        {
            _Double column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Double?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Double_CastToString_Converter()
        {
            var column = (_String)_Double.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_CastToString, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("1", columnFromJson.Eval());
        }
    }
}

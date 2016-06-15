using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using DevZest.Data.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _SingleTests : ColumnConverterTestsBase
    {
        [TestMethod]
        public void _Single_Param()
        {
            TestParam(2.5F);
            TestParam(null);
        }

        private void TestParam(Single? x)
        {
            var column = _Single.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Single_Implicit()
        {
            TestImplicit(2.5F);
            TestImplicit(null);
        }

        private void TestImplicit(Single? x)
        {
            _Single column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Single_Const()
        {
            TestConst(2.5F);
            TestConst(null);
        }

        private void TestConst(Single? x)
        {
            _Single column = _Single.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _Single_Negate()
        {
            TestNegate(2, -2);
            TestNegate(null, null);
        }

        private void TestNegate(Single? x, Single? expectedValue)
        {
            _Single column = x;
            var expr = -column;
            var dbExpr = (DbUnaryExpression)expr.DbExpression;
            dbExpr.Verify(DbUnaryExpressionKind.Negate, column);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_Add()
        {
            TestAdd(1, 2, 3);
            TestAdd(1, null, null);
            TestAdd(null, null, null);
        }

        private void TestAdd(Single? x, Single? y, Single? expectedValue)
        {
            _Single column1 = x;
            _Single column2 = y;
            var expr = column1 + column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Add, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_Substract()
        {
            TestSubstract(5, 2, 3);
            TestSubstract(null, 2, null);
            TestSubstract(2, null, null);
        }

        private void TestSubstract(Single? x, Single? y, Single? expectedValue)
        {
            _Single column1 = x;
            _Single column2 = y;
            var expr = column1 - column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Substract, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_Multiply()
        {
            TestMultiply(5, 5, 25);
            TestMultiply(5, null, null);
            TestMultiply(null, null, null);
        }

        private void TestMultiply(Single? x, Single? y, Single? expectedValue)
        {
            _Single column1 = x;
            _Single column2 = y;
            var expr = column1 * column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Multiply, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_Divide()
        {
            TestDivide(12, 4, 3);
            TestDivide(5, null, null);
            TestDivide(null, 5, null);
            TestDivide(null, null, null);
        }

        private void TestDivide(Single? x, Single? y, Single? expectedValue)
        {
            _Single column1 = x;
            _Single column2 = y;
            var expr = column1 / column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Divide, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_Modulo()
        {
            TestModulo(5, 3, 2);
            TestModulo(100, null, null);
            TestModulo(null, null, null);
        }

        private void TestModulo(Single? x, Single? y, Single? expectedValue)
        {
            _Single column1 = x;
            _Single column2 = y;
            var expr = column1 % column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Modulo, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_LessThan()
        {
            TestLessThan(99, 100, true);
            TestLessThan(99, 99, false);
            TestLessThan(99, 98, false);
            TestLessThan(5, null, null);
            TestLessThan(null, 5, null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(Single? x, Single? y, bool? expectedValue)
        {
            _Single column1 = x;
            _Single column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_LessThanOrEqual()
        {
            TestLessThanOrEqual(99, 98, false);
            TestLessThanOrEqual(99, 99, true);
            TestLessThanOrEqual(99, 100, true);
            TestLessThanOrEqual(5, null, null);
            TestLessThanOrEqual(null, 5, null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(Single? x, Single? y, bool? expectedValue)
        {
            _Single column1 = x;
            _Single column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_GreaterThan()
        {
            TestGreaterThan(100, 99, true);
            TestGreaterThan(100, 100, false);
            TestGreaterThan(99, 100, false);
            TestGreaterThan(5, null, null);
            TestGreaterThan(null, 5, null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(Single? x, Single? y, bool? expectedValue)
        {
            _Single column1 = x;
            _Single column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_GreaterThanOrEqual()
        {
            TestGreaterThanOrEqual(100, 99, true);
            TestGreaterThanOrEqual(100, 100, true);
            TestGreaterThanOrEqual(99, 100, false);
            TestGreaterThanOrEqual(5, null, null);
            TestGreaterThanOrEqual(null, 5, null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(Single? x, Single? y, bool? expectedValue)
        {
            _Single column1 = x;
            _Single column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_Equal()
        {
            TestEqual(2, 2, true);
            TestEqual(4, 5, false);
            TestEqual(1, null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(Single? x, Single? y, bool? expectedValue)
        {
            _Single column1 = x;
            _Single column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_NotEqual()
        {
            TestNotEqual(1, 1, false);
            TestNotEqual(1, 2, true);
            TestNotEqual(1, null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(Single? x, Single? y, bool? expectedValue)
        {
            _Single column1 = x;
            _Single column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_FromBoolean()
        {
            TestFromBoolean(_Boolean.True, 1);
            TestFromBoolean(_Boolean.False, 0);
            TestFromBoolean(_Boolean.Null, null);
        }

        private void TestFromBoolean(_Boolean x, Single? expectedValue)
        {
            _Single expr = (_Single)x;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(x, typeof(bool?), typeof(Single?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_FromByte()
        {
            TestFromByte(null, null);
            TestFromByte(127, 127);
        }

        private void TestFromByte(byte? x, Single? expectedValue)
        {
            _Byte column1 = x;
            _Single expr = (_Single)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(byte?), typeof(Single?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_FromInt16()
        {
            TestFromInt16(null, null);
            TestFromInt16(5, 5);
        }

        private void TestFromInt16(Int16? x, Single? expectedValue)
        {
            _Int16 column1 = x;
            _Single expr = (_Single)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int16?), typeof(Single?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_FromInt64()
        {
            TestFromInt64(8, 8);
            TestFromInt64(null, null);
        }

        private void TestFromInt64(Int64? x, Single? expectedValue)
        {
            _Int64 column1 = x;
            _Single expr = (_Single)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int64?), typeof(Single?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_FromInt32()
        {
            TestFromInt32(8, 8);
            TestFromInt32(null, null);
        }

        private void TestFromInt32(Int32? x, Single? expectedValue)
        {
            _Int32 column1 = x;
            _Single expr = (_Single)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int32?), typeof(Single?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_FromDecimal()
        {
            TestFromDecimal(8, 8);
            TestFromDecimal(null, null);
        }

        private void TestFromDecimal(Decimal? x, Single? expectedValue)
        {
            _Decimal column1 = x;
            _Single expr = (_Single)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Decimal?), typeof(Single?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_FromDouble()
        {
            TestFromDouble(8, 8);
            TestFromDouble(null, null);
        }

        private void TestFromDouble(Double? x, Single? expectedValue)
        {
            _Double column1 = x;
            _Single expr = (_Single)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Double?), typeof(Single?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_FromString()
        {
            TestFromString("8", 8);
            TestFromString(null, null);
        }

        private void TestFromString(String x, Single? expectedValue)
        {
            _String column1 = x;
            _Single expr = (_Single)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(Single?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Single_Add_Converter()
        {
            var column = _Single.Const(1) + _Single.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_Add, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_Divide_Converter()
        {
            var column = _Single.Const(6) / _Single.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_Divide, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_Equal_Converter()
        {
            var column = _Single.Const(1) == _Single.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_FromBoolean_Converter()
        {
            var column = (_Single)_Boolean.True;
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_FromBoolean, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_FromByte_Converter()
        {
            var column = (_Single)_Byte.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_FromByte, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_FromDecimal_Converter()
        {
            var column = (_Single)_Decimal.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_FromDecimal, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_FromDouble_Converter()
        {
            var column = (_Single)_Double.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_FromDouble, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_FromInt16_Converter()
        {
            var column = (_Single)_Int16.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_FromInt16, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_FromInt32_Converter()
        {
            var column = (_Single)_Int32.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_FromInt32, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_FromInt64_Converter()
        {
            var column = (_Single)_Int64.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_FromInt64, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_FromString_Converter()
        {
            var column = (_Single)_String.Const("1");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_FromString, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_GreaterThan_Converter()
        {
            var column = _Single.Const(4) > _Single.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_GreaterThanOrEqual_Converter()
        {
            var column = _Single.Const(3) >= _Single.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_LessThan_Converter()
        {
            var column = _Single.Const(3) < _Single.Const(4);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_LessThanOrEqual_Converter()
        {
            var column = _Single.Const(3) <= _Single.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_Modulo_Converter()
        {
            var column = _Single.Const(5) % _Single.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_Modulo, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_Multiply_Converter()
        {
            var column = _Single.Const(5) * _Single.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_Multiply, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)15, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_Negate_Converter()
        {
            var column = -_Single.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_Negate, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)(-5), columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_NotEqual_Converter()
        {
            var column = _Single.Const(2) != _Single.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _Single_Substract_Converter()
        {
            var column = _Single.Const(5) - _Single.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Single_Substract, json);

            var columnFromJson = (_Single)Column.FromJson(null, json);
            Assert.AreEqual((Single)2, columnFromJson.Eval());
        }
    }
}

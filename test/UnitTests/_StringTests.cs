using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using DevZest.Data.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace DevZest.Data
{
    [TestClass]
    public class _StringTests : ColumnConverterTestsBase
    {
        [TestMethod]
        public void _String_Param()
        {
            TestParam("ABC");
            TestParam(null);
        }

        private void TestParam(String x)
        {
            var column = _String.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _String_Implicit()
        {
            TestImplicit("ABC");
            TestImplicit(null);
        }

        private void TestImplicit(String x)
        {
            _String column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _String_Const()
        {
            TestConst("ABC");
            TestConst(null);
        }

        private void TestConst(String x)
        {
            _String column = _String.Const(x);
            column.VerifyConst(x);
        }
        [TestMethod]
        public void _String_Add()
        {
            TestAdd("A", "B", "AB");
            TestAdd("A", null, null);
            TestAdd(null, null, null);
        }

        private void TestAdd(String x, String y, String expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 + column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Add, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_LessThan()
        {
            TestLessThan("099", "100", true);
            TestLessThan("99", "99", false);
            TestLessThan("99", "98", false);
            TestLessThan("5", null, null);
            TestLessThan(null, "5", null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(String x, String y, bool? expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_LessThanOrEqual()
        {
            TestLessThanOrEqual("99", "98", false);
            TestLessThanOrEqual("99", "99", true);
            TestLessThanOrEqual("099", "100", true);
            TestLessThanOrEqual("5", null, null);
            TestLessThanOrEqual(null, "5", null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(String x, String y, bool? expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_GreaterThan()
        {
            TestGreaterThan("100", "099", true);
            TestGreaterThan("100", "100", false);
            TestGreaterThan("099", "100", false);
            TestGreaterThan("5", null, null);
            TestGreaterThan(null, "5", null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(String x, String y, bool? expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_GreaterThanOrEqual()
        {
            TestGreaterThanOrEqual("100", "099", true);
            TestGreaterThanOrEqual("100", "100", true);
            TestGreaterThanOrEqual("099", "100", false);
            TestGreaterThanOrEqual("5", null, null);
            TestGreaterThanOrEqual(null, "5", null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(String x, String y, bool? expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_Equal()
        {
            TestEqual("2", "2", true);
            TestEqual("4", "5", false);
            TestEqual("1", null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(String x, String y, bool? expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_NotEqual()
        {
            TestNotEqual("1", "1", false);
            TestNotEqual("1", "2", true);
            TestNotEqual("1", null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(String x, String y, bool? expectedValue)
        {
            _String column1 = x;
            _String column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_FromBoolean()
        {
            TestFromBoolean(_Boolean.True, "True");
            TestFromBoolean(_Boolean.False, "False");
            TestFromBoolean(_Boolean.Null, null);
        }

        private void TestFromBoolean(_Boolean x, String expectedValue)
        {
            _String expr = (_String)x;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(x, typeof(bool?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_FromByte()
        {
            TestFromByte(null, null);
            TestFromByte(127, "127");
        }

        private void TestFromByte(byte? x, String expectedValue)
        {
            _Byte column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(byte?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_FromChar()
        {
            TestFromChar(null, null);
            TestFromChar('A', "A");
        }

        private void TestFromChar(Char? x, String expectedValue)
        {
            _Char column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Char?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_FromDateTime()
        {
            var now = DateTime.Now;
            TestFromDateTime(null, null);
            TestFromDateTime(now, now.ToString("O", CultureInfo.InvariantCulture));
        }

        private void TestFromDateTime(DateTime? x, String expectedValue)
        {
            _DateTime column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(DateTime?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_FromGuid()
        {
            var guid = Guid.NewGuid();
            TestFromGuid(null, null);
            TestFromGuid(guid, guid.ToString());
        }

        private void TestFromGuid(Guid? x, String expectedValue)
        {
            _Guid column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Guid?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_FromInt16()
        {
            TestFromInt16(null, null);
            TestFromInt16(5, "5");
        }

        private void TestFromInt16(Int16? x, String expectedValue)
        {
            _Int16 column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int16?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_FromInt32()
        {
            TestFromInt32(1234567, "1234567");
            TestFromInt32(null, null);
        }

        private void TestFromInt32(Int32? x, String expectedValue)
        {
            _Int32 column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int32?), typeof(String));
            expr.VerifyEval(expectedValue);
        }


        [TestMethod]
        public void _String_FromInt64()
        {
            TestFromInt64(8, "8");
            TestFromInt64(null, null);
        }

        private void TestFromInt64(Int64? x, String expectedValue)
        {
            _Int64 column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Int64?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_FromDecimal()
        {
            TestFromDecimal(8, "8");
            TestFromDecimal(null, null);
        }

        private void TestFromDecimal(Decimal? x, String expectedValue)
        {
            _Decimal column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Decimal?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_FromDouble()
        {
            TestFromDouble(8, "8");
            TestFromDouble(null, null);
        }

        private void TestFromDouble(Double? x, String expectedValue)
        {
            _Double column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Double?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_FromSingle()
        {
            TestFromSingle(8, "8");
            TestFromSingle(null, null);
        }

        private void TestFromSingle(Single? x, String expectedValue)
        {
            _Single column1 = x;
            _String expr = (_String)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Single?), typeof(String));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _String_Add_Converter()
        {
            var column = _String.Const("a") + _String.Const("b");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_Add, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("ab", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_Equal_Converter()
        {
            var column = _String.Const("a") == _String.Const("a");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_FromBoolean_Converter()
        {
            var column = (_String)_Boolean.True;
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_FromBoolean, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("True", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_FromByte_Converter()
        {
            var column = (_String)_Byte.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_FromByte, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("1", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_FromChar_Converter()
        {
            var column = (_String)_Char.Const('a');
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_FromChar, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("a", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_FromDateTime_Converter()
        {
            var column = (_String)_DateTime.Const(new DateTime(2016, 6, 15));
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_FromDateTime, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("2016-06-15T00:00:00.0000000", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_FromDecimal_Converter()
        {
            var column = (_String)_Decimal.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_FromDecimal, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("1", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_FromDouble_Converter()
        {
            var column = (_String)_Double.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_FromDouble, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("1", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_FromGuid_Converter()
        {
            var column = (_String)_Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_FromGuid, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_FromInt16_Converter()
        {
            var column = (_String)_Int16.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_FromInt16, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("1", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_FromInt32_Converter()
        {
            var column = (_String)_Int32.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_FromInt32, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("1", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_FromInt64_Converter()
        {
            var column = (_String)_Int64.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_FromInt64, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("1", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_FromSingle_Converter()
        {
            var column = (_String)_Single.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_FromSingle, json);

            var columnFromJson = (_String)Column.FromJson(null, json);
            Assert.AreEqual("1", columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_GreaterThan_Converter()
        {
            var column = _String.Const("b") > _String.Const("a");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_GreaterThanOrEqual_Converter()
        {
            var column = _String.Const("a") >= _String.Const("a");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_LessThan_Converter()
        {
            var column = _String.Const("a") < _String.Const("b");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_LessThanOrEqual_Converter()
        {
            var column = _String.Const("a") <= _String.Const("a");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void _String_NotEqual_Converter()
        {
            var column = _String.Const("a") != _String.Const("b");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_String_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }
    }
}

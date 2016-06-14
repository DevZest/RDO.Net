using DevZest.Data.Resources;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Primitives
{
    [TestClass]
    public class ColumnConverterTests
    {
        [TestInitialize]
        public void Initialize()
        {
            ColumnConverter.EnsureInitialized(typeof(_Int32));
        }

        [TestMethod]
        public void Converter_Column()
        {
            var salesOrder = new SalesOrder();
            var json = salesOrder.SalesOrderID.ToJson(true);
            Assert.AreEqual(Json.Converter_Column, json);

            var columnFromJson = Column.FromJson(salesOrder, json);
            Assert.AreEqual(salesOrder.SalesOrderID, columnFromJson);
        }

        [TestMethod]
        public void Converter_ConstantExpression()
        {
            _Int32 column = _Int32.Const(5);
            var json = column.ToJson(true);

            Assert.AreEqual(Json.Converter_ConstantExpression, json);

            var columnFromJson = (_Int32)Column.FromJson(null, json);
            Assert.AreEqual(5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_ParamExpression_NullSourceColumn()
        {
            _Int32 column = _Int32.Param(5);
            var json = column.ToJson(true);

            Assert.AreEqual(Json.Converter_ParamExpression_NullSourceColumn, json);

            var columnFromJson = (_Int32)Column.FromJson(null, json);
            Assert.AreEqual(5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_ParamExpression_NotNullSourceColumn()
        {
            var salesOrder = new SalesOrder();
            var column = _Int32.Param(5, salesOrder.SalesOrderID);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_ParamExpression_NotNullSourceColumn, json);

            var columnFromJson = (_Int32)Column.FromJson(salesOrder, json);
            Assert.AreEqual(5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Boolean_And()
        {
            var column = _Boolean.Const(true) & _Boolean.Const(false);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Boolean_Add, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(false, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Boolean_FromString()
        {
            var column = (_Boolean)(_String.Const("true"));
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Boolean_FromString, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Boolean_Not()
        {
            var column = !_Boolean.Const(true);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Boolean_Not, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(false, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Boolean_Or()
        {
            var column = _Boolean.Const(true) | _Boolean.Const(false);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Boolean_Or, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_CaseExpression()
        {
            var column = Case.When(_Boolean.Const(true)).Then(_Boolean.False)
                .Else(_Boolean.True);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_CaseExpression, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(false, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_CaseOnExpression()
        {
            var column = Case.On(_Boolean.True)
                .When(_Boolean.True).Then(_Boolean.False)
                .Else(_Boolean.True);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_CaseOnExpression, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(false, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_Add()
        {
            var column = _Byte.Const(1) + _Byte.Const(2);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_Add, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_BitwiseAnd()
        {
            var column = _Byte.Const(1) & _Byte.Const(0);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_BitwiseAnd, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)0, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_BitwiseOr()
        {
            var column = _Byte.Const(1) | _Byte.Const(0);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_BitwiseOr, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_BitwiseXor()
        {
            var column = _Byte.Const(1) ^ _Byte.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_BitwiseXor, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)0, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_Divide()
        {
            var column = _Byte.Const(12) / _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_Divide, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)4, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_Equal()
        {
            var column = _Byte.Const(1) == _Byte.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_FromBoolean()
        {
            var column = (_Byte)_Boolean.True;
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromBoolean, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_FromDecimal()
        {
            var column = (_Byte)_Decimal.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromDecimal, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_FromDouble()
        {
            var column = (_Byte)_Double.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromDouble, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_FromInt16()
        {
            var column = (_Byte)_Int16.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromInt16, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_FromInt32()
        {
            var column = (_Byte)_Int32.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromInt32, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_FromInt64()
        {
            var column = (_Byte)_Int64.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromInt64, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_FromSingle()
        {
            var column = (_Byte)_Single.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromSingle, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_FromString()
        {
            var column = (_Byte)_String.Const("5");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_FromString, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_GreaterThan()
        {
            var column = _Byte.Const(3) > _Byte.Const(2);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_GreaterThanOrEqual()
        {
            var column = _Byte.Const(3) >= _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_LessThan()
        {
            var column = _Byte.Const(3) < _Byte.Const(2);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(false, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_LessThanOrEqual()
        {
            var column = _Byte.Const(3) <= _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_Modulo()
        {
            var column = _Byte.Const(5) % _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_Modulo, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_Multiply()
        {
            var column = _Byte.Const(5) * _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_Multiply, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)15, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_NotEqual()
        {
            var column = _Byte.Const(5) != _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_OnesComplement()
        {
            var column = ~_Byte.Const(0);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_OnesComplement, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)255, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Byte_Substract()
        {
            var column = _Byte.Const(5) - _Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Byte_Substract, json);

            var columnFromJson = (_Byte)Column.FromJson(null, json);
            Assert.AreEqual((byte)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Char_Equal()
        {
            var column = _Char.Const('a') == _Char.Const('a');
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Char_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Char_FromString()
        {
            var column = (_Char)_String.Const("a");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Char_FromString, json);

            var columnFromJson = (_Char)Column.FromJson(null, json);
            Assert.AreEqual('a', columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Char_GreaterThan()
        {
            var column = _Char.Const('b') > _Char.Const('a');
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Char_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Char_GreaterThanOrEqual()
        {
            var column = _Char.Const('b') >= _Char.Const('b');
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Char_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Char_LessThan()
        {
            var column = _Char.Const('a') < _Char.Const('b');
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Char_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Char_LessThanOrEqual()
        {
            var column = _Char.Const('b') <= _Char.Const('b');
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Char_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Char_NotEqual()
        {
            var column = _Char.Const('a') != _Char.Const('b');
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Char_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_DateTime_Equal()
        {
            var column = _DateTime.Const(new DateTime(2016, 6, 14)) == _DateTime.Const(new DateTime(2016, 6, 14));
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_DateTime_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_DateTime_FromString()
        {
            var column = (_DateTime)_String.Const("2016/6/14");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_DateTime_FromString, json);

            var columnFromJson = (_DateTime)Column.FromJson(null, json);
            Assert.AreEqual(new DateTime(2016, 6, 14), columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_DateTime_GreaterThan()
        {
            var column = _DateTime.Const(new DateTime(2016, 6, 15)) > _DateTime.Const(new DateTime(2016, 6, 14));
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_DateTime_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_DateTime_GreaterThanOrEqual()
        {
            var column = _DateTime.Const(new DateTime(2016, 6, 14)) >= _DateTime.Const(new DateTime(2016, 6, 14));
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_DateTime_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_DateTime_LessThan()
        {
            var column = _DateTime.Const(new DateTime(2016, 6, 13)) < _DateTime.Const(new DateTime(2016, 6, 14));
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_DateTime_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_DateTime_LessThanOrEqual()
        {
            var column = _DateTime.Const(new DateTime(2016, 6, 14)) <= _DateTime.Const(new DateTime(2016, 6, 14));
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_DateTime_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_DateTime_NotEqual()
        {
            var column = _DateTime.Const(new DateTime(2016, 6, 13)) != _DateTime.Const(new DateTime(2016, 6, 14));
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_DateTime_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_Add()
        {
            var column = _Decimal.Const(1) + _Decimal.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Add, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_Divide()
        {
            var column = _Decimal.Const(15) / _Decimal.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Divide, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_Equal()
        {
            var column = _Decimal.Const(5) == _Decimal.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_FromBoolean()
        {
            var column = (_Decimal)_Boolean.True;
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromBoolean, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_FromByte()
        {
            var column = (_Decimal)_Byte.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromByte, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_FromDouble()
        {
            var column = (_Decimal)_Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromDouble, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_FromInt16()
        {
            var column = (_Decimal)_Int16.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromInt16, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_FromInt32()
        {
            var column = (_Decimal)_Int32.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromInt32, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_FromInt64()
        {
            var column = (_Decimal)_Int64.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromInt64, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_FromSingle()
        {
            var column = (_Decimal)_Single.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromSingle, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_FromString()
        {
            var column = (_Decimal)_String.Const("3");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_FromString, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_GreaterThan()
        {
            var column = _Decimal.Const(4) > _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_GreaterThanOrEqual()
        {
            var column = _Decimal.Const(3) >= _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_LessThan()
        {
            var column = _Decimal.Const(3) < _Decimal.Const(4);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_LessThanOrEqual()
        {
            var column = _Decimal.Const(3) <= _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_Modulo()
        {
            var column = _Decimal.Const(5) % _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Modulo, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_Multiply()
        {
            var column = _Decimal.Const(5) * _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Multiply, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)15, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_Negate()
        {
            var column = -_Decimal.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Negate, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)(-5), columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_NotEqual()
        {
            var column = _Decimal.Const(2) != _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Decimal_Substract()
        {
            var column = _Decimal.Const(5) - _Decimal.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Decimal_Substract, json);

            var columnFromJson = (_Decimal)Column.FromJson(null, json);
            Assert.AreEqual((Decimal)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_Add()
        {
            var column = _Double.Const(1) + _Double.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Add, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_Divide()
        {
            var column = _Double.Const(6) / _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Divide, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_Equal()
        {
            var column = _Double.Const(1) == _Double.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_FromBoolean()
        {
            var column = (_Double)_Boolean.True;
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromBoolean, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_FromByte()
        {
            var column = (_Double)_Byte.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromByte, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_FromDecimal()
        {
            var column = (_Double)_Decimal.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromDecimal, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_FromInt16()
        {
            var column = (_Double)_Int16.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromInt16, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_FromInt32()
        {
            var column = (_Double)_Int32.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromInt32, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_FromInt64()
        {
            var column = (_Double)_Int64.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromInt64, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_FromSingle()
        {
            var column = (_Double)_Single.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromSingle, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_FromString()
        {
            var column = (_Double)_String.Const("1");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_FromString, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_GreaterThan()
        {
            var column = _Double.Const(4) > _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_GreaterThanOrEqual()
        {
            var column = _Double.Const(3) >= _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_LessThan()
        {
            var column = _Double.Const(3) < _Double.Const(4);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_LessThanOrEqual()
        {
            var column = _Double.Const(3) <= _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_Modulo()
        {
            var column = _Double.Const(5) % _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Modulo, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_Multiply()
        {
            var column = _Double.Const(5) * _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Multiply, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)15, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_Negate()
        {
            var column = -_Double.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Negate, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)(-5), columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_NotEqual()
        {
            var column = _Double.Const(2) != _Double.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Double_Substract()
        {
            var column = _Double.Const(5) - _Double.Const(3);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Double_Substract, json);

            var columnFromJson = (_Double)Column.FromJson(null, json);
            Assert.AreEqual((Double)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Guid_Equal()
        {
            var column = _Guid.Const(new Guid()) == _Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Guid_FromString()
        {
            var column = (_Guid)_String.Const("00000000-0000-0000-0000-000000000000");
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_FromString, json);

            var columnFromJson = (_Guid)Column.FromJson(null, json);
            Assert.AreEqual(new Guid(), columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Guid_GreaterThan()
        {
            var column = _Guid.Const(new Guid()) > _Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_GreaterThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(false, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Guid_GreaterThanOrEqual()
        {
            var column = _Guid.Const(new Guid()) >= _Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_GreaterThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Guid_LessThan()
        {
            var column = _Guid.Const(new Guid()) < _Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_LessThan, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(false, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Guid_LessThanOrEqual()
        {
            var column = _Guid.Const(new Guid()) <= _Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_LessThanOrEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Guid_NotEqual()
        {
            var column = _Guid.Const(new Guid()) != _Guid.Const(new Guid());
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Guid_NotEqual, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(false, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Int16_Add()
        {
            var column = _Int16.Const(1) + _Int16.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Int16_Add, json);

            var columnFromJson = (_Int16)Column.FromJson(null, json);
            Assert.AreEqual((Int16)2, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Int16_BitwiseAnd()
        {
            var column = _Int16.Const(1) & _Int16.Const(0);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Int16_BitwiseAnd, json);

            var columnFromJson = (_Int16)Column.FromJson(null, json);
            Assert.AreEqual((Int16)0, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Int16_BitwiseOr()
        {
            var column = _Int16.Const(1) | _Int16.Const(0);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Int16_BitwiseOr, json);

            var columnFromJson = (_Int16)Column.FromJson(null, json);
            Assert.AreEqual((Int16)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Int16_BitwiseXor()
        {
            var column = _Int16.Const(1) ^ _Int16.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Int16_BitwiseXor, json);

            var columnFromJson = (_Int16)Column.FromJson(null, json);
            Assert.AreEqual((Int16)0, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Int16_Divide()
        {
            var column = _Int16.Const(15) / _Int16.Const(5);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Int16_Divide, json);

            var columnFromJson = (_Int16)Column.FromJson(null, json);
            Assert.AreEqual((Int16)3, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Int16_Equal()
        {
            var column = _Int16.Const(1) == _Int16.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Int16_Equal, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Int16_FromBoolan()
        {
            var column = (_Int16)_Boolean.True;
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Int16_FromBoolean, json);

            var columnFromJson = (_Int16)Column.FromJson(null, json);
            Assert.AreEqual((Int16)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Int16_FromByte()
        {
            var column = (_Int16)_Byte.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Int16_FromByte, json);

            var columnFromJson = (_Int16)Column.FromJson(null, json);
            Assert.AreEqual((Int16)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Int16_FromDecimal()
        {
            var column = (_Int16)_Decimal.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Int16_FromDecimal, json);

            var columnFromJson = (_Int16)Column.FromJson(null, json);
            Assert.AreEqual((Int16)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Int16_FromDouble()
        {
            var column = (_Int16)_Double.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Int16_FromDouble, json);

            var columnFromJson = (_Int16)Column.FromJson(null, json);
            Assert.AreEqual((Int16)1, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_Int16_FromInt32()
        {
            var column = (_Int16)_Int32.Const(1);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Int16_FromInt32, json);

            var columnFromJson = (_Int16)Column.FromJson(null, json);
            Assert.AreEqual((Int16)1, columnFromJson.Eval());
        }
    }
}

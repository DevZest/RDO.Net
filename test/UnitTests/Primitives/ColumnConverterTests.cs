using DevZest.Data.Resources;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}

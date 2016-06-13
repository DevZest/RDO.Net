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
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DevZest.Data.Helpers;
using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Resources;

namespace DevZest.Data
{
    [TestClass]
    public class ColumnTests : ColumnConverterTestsBase
    {
        [TestMethod]
        public void Column_Nullable()
        {
            var column = new _Int32();
            column.VerifyNullable(true);

            column.Nullable(false);
            column.VerifyNullable(false);
            
            column.Nullable(true);
            column.VerifyNullable(true);
        }

        [TestMethod]
        public void Column_Default_const()
        {
            var column = new _Int32();
            column.DefaultValue(5);
            column.VerifyDefault(5);
        }

        [TestMethod]
        public void Column_Default_function()
        {
            var dateTime = new _DateTime();
            dateTime.Default(Functions.GetDate());
            var defaultValue = dateTime.GetDefault().Value;
            var span = DateTime.Now - defaultValue;
            Assert.IsTrue(span.Value.Seconds < 1);
        }

        [TestMethod]
        public void Column_Converter()
        {
            var salesOrder = new SalesOrder();
            var json = salesOrder.SalesOrderID.ToJson(true);
            Assert.AreEqual(Json.Converter_Column, json);

            var columnFromJson = Column.ParseJson<Column>(salesOrder, json);
            Assert.AreEqual(salesOrder.SalesOrderID, columnFromJson);
        }

        [TestMethod]
        public void Column_Converter_ConstantExpression()
        {
            _Int32 column = _Int32.Const(5);
            var json = column.ToJson(true);

            Assert.AreEqual(Json.Converter_ConstantExpression, json);

            var columnFromJson = Column.ParseJson<_Int32>(null, json);
            Assert.AreEqual(5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Column_Converter_ParamExpression_NullSourceColumn()
        {
            _Int32 column = _Int32.Param(5);
            var json = column.ToJson(true);

            Assert.AreEqual(Json.Converter_ParamExpression_NullSourceColumn, json);

            var columnFromJson = Column.ParseJson<_Int32>(null, json);
            Assert.AreEqual(5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Column_Converter_ParamExpression_NotNullSourceColumn()
        {
            var salesOrder = new SalesOrder();
            var column = _Int32.Param(5, salesOrder.SalesOrderID);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_ParamExpression_NotNullSourceColumn, json);

            var columnFromJson = Column.ParseJson<_Int32>(salesOrder, json);
            Assert.AreEqual(5, columnFromJson.Eval());
        }
    }
}

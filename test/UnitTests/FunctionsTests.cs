using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using DevZest.Data.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class FunctionsTests : ColumnConverterTestsBase
    {
        private class SimpleModel : Model
        {
            public static readonly Accessor<SimpleModel, _Int32> Column1Accessor = RegisterColumn((SimpleModel x) => x.Column1);

            public _Int32 Column1 { get; private set; }
        }

        [TestMethod]
        public void Functions_IsNull()
        {
            var dataSet = DataSet<SimpleModel>.New();
            var model = dataSet._;
            
            var column1 = model.Column1;
            var isNullExpr = column1.IsNull();
            ((DbFunctionExpression)isNullExpr.DbExpression).Verify(FunctionKeys.IsNull, column1);

            var dataRow = dataSet.AddRow();
            
            column1[dataRow] = null;
            Assert.AreEqual(true, isNullExpr[dataRow]);

            column1[dataRow] = 1;
            Assert.AreEqual(false, isNullExpr[dataRow]);
        }

        [TestMethod]
        public void Functions_IsNull_Converter()
        {
            var column = _Int32.Const(null).IsNull();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_IsNull, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Functions_IsNotNull()
        {
            var dataSet = DataSet<SimpleModel>.New();

            var column1 = dataSet._.Column1;
            var isNotNullExpr = column1.IsNotNull();
            ((DbFunctionExpression)isNotNullExpr.DbExpression).Verify(FunctionKeys.IsNotNull, column1);

            var dataRow = dataSet.AddRow();

            column1[dataRow] = null;
            Assert.AreEqual(false, isNotNullExpr[dataRow]);

            column1[dataRow] = 1;
            Assert.AreEqual(true, isNotNullExpr[dataRow]);
        }

        [TestMethod]
        public void Functions_IsNotNull_Converter()
        {
            var column = _Int32.Const(2).IsNotNull();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_IsNotNull, json);

            var columnFromJson = (_Boolean)Column.FromJson(null, json);
            Assert.AreEqual(true, columnFromJson.Eval());
        }

        [TestMethod]
        public void Functions_IfNull()
        {
            var dataSet = DataSet<SimpleModel>.New();

            var column1 = dataSet._.Column1;
            var replace = _Int32.Const(10);
            var expr = column1.IfNull(replace);
            ((DbFunctionExpression)expr.DbExpression).Verify(FunctionKeys.IfNull, column1, replace);

            var dataRow = dataSet.AddRow();

            column1[dataRow] = null;
            Assert.AreEqual(10, expr[dataRow]);

            column1[dataRow] = 1;
            Assert.AreEqual(1, expr[dataRow]);
        }

        [TestMethod]
        public void Functions_IfNull_Converter()
        {
            var column = _Int32.Const(null).IfNull(_Int32.Const(3));
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_IfNull, json);

            var columnFromJson = (_Int32)Column.FromJson(null, json);
            Assert.AreEqual(3, columnFromJson.Eval());
        }

        [TestMethod]
        public void Functions_GetDate()
        {
            var getDateExpr = Functions.GetDate();
            ((DbFunctionExpression)getDateExpr.DbExpression).Verify(FunctionKeys.GetDate);

            var currentDate = getDateExpr.Eval();
            var span = DateTime.Now - currentDate;
            Assert.AreEqual(true, span.Value.Seconds < 1);
        }

        [TestMethod]
        public void Functions_GetDate_Converter()
        {
            var column = Functions.GetDate();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_GetDate, json);

            var columnFromJson = (_DateTime)Column.FromJson(null, json);
            var currentDate = columnFromJson.Eval();
            var span = DateTime.Now - currentDate;
            Assert.AreEqual(true, span.Value.Seconds < 1);
        }

        [TestMethod]
        public void Functions_GetUtcDate()
        {
            var getUtcDateExpr = Functions.GetUtcDate();
            ((DbFunctionExpression)getUtcDateExpr.DbExpression).Verify(FunctionKeys.GetUtcDate);

            var currentDate = getUtcDateExpr.Eval();
            var span = DateTime.UtcNow - currentDate;
            Assert.AreEqual(true, span.Value.Seconds < 1);
        }

        [TestMethod]
        public void Functions_GetUtcDate_Converter()
        {
            var column = Functions.GetUtcDate();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_GetUtcDate, json);

            var columnFromJson = (_DateTime)Column.FromJson(null, json);
            var currentDate = columnFromJson.Eval();
            var span = DateTime.UtcNow - currentDate;
            Assert.AreEqual(true, span.Value.Seconds < 1);
        }

        [TestMethod]
        public void Functions_NewGuid()
        {
            var newGuidExpr = Functions.NewGuid();
            ((DbFunctionExpression)newGuidExpr.DbExpression).Verify(FunctionKeys.NewGuid);

            var newGuid = newGuidExpr.Eval();
            Assert.IsTrue(newGuid.HasValue);
        }

        [TestMethod]
        public void Functions_NewGuid_Converter()
        {
            var column = Functions.NewGuid();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Functions_NewGuid, json);

            var columnFromJson = (_Guid)Column.FromJson(null, json);
            var newGuid = columnFromJson.Eval();
            Assert.IsTrue(newGuid.HasValue);
        }
    }
}

using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class DbFunctionsTests
    {
        private class SimpleModel : Model
        {
            public static readonly Accessor<SimpleModel, _Int32> Column1Accessor = RegisterColumn((SimpleModel x) => x.Column1);

            public _Int32 Column1 { get; private set; }
        }

        [TestMethod]
        public void DbFunctions_IsNull()
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
        public void DbFunctions_IsNotNull()
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
        public void DbFunctions_IfNull()
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
        public void DbFunctions_GetDate()
        {
            var getDateExpr = Functions.GetDate();
            ((DbFunctionExpression)getDateExpr.DbExpression).Verify(FunctionKeys.GetDate);

            var currentDate = getDateExpr.Eval();
            var span = DateTime.Now - currentDate;
            Assert.AreEqual(true, span.Value.Seconds < 1);
        }

        [TestMethod]
        public void DbFunctions_GetUtcDate()
        {
            var getUtcDateExpr = Functions.GetUtcDate();
            ((DbFunctionExpression)getUtcDateExpr.DbExpression).Verify(FunctionKeys.GetUtcDate);

            var currentDate = getUtcDateExpr.Eval();
            var span = DateTime.UtcNow - currentDate;
            Assert.AreEqual(true, span.Value.Seconds < 1);
        }

        [TestMethod]
        public void DbFunctions_NewGuid()
        {
            var newGuidExpr = Functions.NewGuid();
            ((DbFunctionExpression)newGuidExpr.DbExpression).Verify(FunctionKeys.NewGuid);

            var newGuid = newGuidExpr.Eval();
            Assert.IsTrue(newGuid.HasValue);
        }
    }
}

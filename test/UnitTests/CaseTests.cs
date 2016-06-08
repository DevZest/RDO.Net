using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class CaseTests
    {
        private class SimpleModel : Model
        {
            public static readonly Accessor<SimpleModel, _Int32> Column1Accessor = RegisterColumn((SimpleModel x) => x.Column1);

            public _Int32 Column1 { get; private set; }
        }

        [TestMethod]
        public void CaseOn_verify_expression()
        {
            var dataSet = DataSet<SimpleModel>.New();

            var column1 = dataSet._.Column1;
            _Int32 c1 = 1;
            _Int32 c0 = 0;
            var expr = column1.When(c1, _Boolean.True)
                .When(c0, _Boolean.False)
                .Else(_Boolean.Null);
            var dbExpr = (DbCaseExpression)expr.DbExpression;
            dbExpr.Verify(column1, c1, _Boolean.True, c0, _Boolean.False, _Boolean.Null);

            var dataRow = dataSet.AddRow();
            column1[dataRow] = 1;
            Assert.AreEqual(true, expr[dataRow]);

            column1[dataRow] = 0;
            Assert.AreEqual(false, expr[dataRow]);

            column1[dataRow] = null;
            Assert.AreEqual(null, expr[dataRow]);
        }

        [TestMethod]
        public void Case_verify_expression()
        {
            var dataSet = DataSet<SimpleModel>.New();
            var dataRow = dataSet.AddRow();

            var column1 = dataSet._.Column1;
            _Boolean c1 = column1 == 1;
            _Boolean c0 = column1 == 0;
            var expr = Functions.Then(c1, _Boolean.True)
                .When(c0, _Boolean.False)
                .Else(_Boolean.Null);
            var dbExpr = (DbCaseExpression)expr.DbExpression;
            dbExpr.Verify(null, c1, _Boolean.True, c0, _Boolean.False, _Boolean.Null);

            column1[dataRow] = 1;
            Assert.AreEqual(true, expr[dataRow]);

            column1[dataRow] = 0;
            Assert.AreEqual(false, expr[dataRow]);

            column1[dataRow] = null;
            Assert.AreEqual(null, expr[dataRow]);
        }
    }
}

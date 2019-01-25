using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class FunctionsTests
    {
        private class SimpleModel : Model
        {
            public static readonly Mounter<_Int32> _Column1 = RegisterColumn((SimpleModel _) => _.Column1);

            public static readonly Mounter<_String> _Column2 = RegisterColumn((SimpleModel _) => _.Column2);

            public _Int32 Column1 { get; private set; }

            public _String Column2 { get; private set; }
        }

        [TestMethod]
        public void Functions_IsNull()
        {
            var dataSet = DataSet<SimpleModel>.Create();
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
        public void Functions_IsNotNull()
        {
            var dataSet = DataSet<SimpleModel>.Create();

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
        public void Functions_IfNull()
        {
            var dataSet = DataSet<SimpleModel>.Create();

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
        public void Functions_Contains()
        {
            var dataSet = DataSet<SimpleModel>.Create();
            var model = dataSet._;

            var column = model.Column2;
            var value = _String.Const("abc");
            var containsExpr = column.Contains(value);
            ((DbFunctionExpression)containsExpr.DbExpression).Verify(FunctionKeys.Contains, column, value);

            var dataRow = dataSet.AddRow();

            column[dataRow] = null;
            Assert.AreEqual(null, containsExpr[dataRow]);

            column[dataRow] = "abcdefg";
            Assert.AreEqual(true, containsExpr[dataRow]);

            column[dataRow] = "cdefg";
            Assert.AreEqual(false, containsExpr[dataRow]);
        }
    }
}

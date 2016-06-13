using DevZest.Data.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Primitives
{
    [TestClass]
    public class ConstantExpressionTests : ColumnExpressionTestsBase
    {
        [TestMethod]
        public void ConstantExpression_Converter()
        {
            _Int32 column = _Int32.Const(5);
            var json = column.ToJson(true);

            Assert.AreEqual(Json.Converter_ValueExpression, json);

            var fromJson = (_Int32)Column.FromJson(null, json);
            Assert.AreEqual(5, fromJson.Eval());
        }
    }
}

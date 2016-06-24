using DevZest.Data.Helpers;
using DevZest.Data.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _BinaryTests : ColumnConverterTestsBase
    {
        [TestMethod]
        public void _Binary_Implicit()
        {
            TestParam(new byte[0]);
            TestParam(null);
        }

        private void TestParam(Binary x)
        {
            _Binary column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Binary_Const()
        {
            TestConstant(new byte[0]);
            TestConstant(null);
        }

        private void TestConstant(Binary x)
        {
            _Binary column = _Binary.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _Binary_CastToString_Converter()
        {
            var bytes = new byte[] { 1, 2, 3 };
            var column = _Binary.Const(new Binary(bytes)).CastToString();
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_Binary_CastToString, json);

            var fromJsonColumn = Column.ParseJson<_String>(null, json);
            Assert.AreEqual(Convert.ToBase64String(bytes), fromJsonColumn.Eval());
        }
    }
}

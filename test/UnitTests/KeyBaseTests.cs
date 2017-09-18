using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class KeyBaseTests
    {
        private class SimpleKey : KeyBase
        {
            public SimpleKey(_Int32 column1, _Int32 column2)
            {
                Column1 = column1;
                Column2 = column2;
            }

            public _Int32 Column1 { get; private set; }

            [Sort(SortDirection.Descending)]
            public _Int32 Column2 { get; private set; }
        }

        [TestMethod]
        public void KeyBase_column_sort_list_successfully_constructed()
        {
            var simpleKey = new SimpleKey(new _Int32(), new _Int32());
            Assert.AreEqual(2, simpleKey.Count);
            Assert.AreEqual(simpleKey.Column1, simpleKey[0].Column);
            Assert.AreEqual(simpleKey.Column2, simpleKey[1].Column);
            Assert.AreEqual(SortDirection.Unspecified, simpleKey[0].Direction);
            Assert.AreEqual(SortDirection.Descending, simpleKey[1].Direction);
        }
    }
}

using DevZest.Data.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class KeyBaseTests
    {
        private sealed class SimpleKey : PrimaryKey
        {
            public SimpleKey(_Int32 column1, [Desc]_Int32 column2)
                : base(column1, column2.Desc())
            {
            }
        }

        [TestMethod]
        public void KeyBase_column_sort_list_successfully_constructed()
        {
            var column1 = new _Int32();
            var column2 = new _Int32();
            var simpleKey = new SimpleKey(column1, column2);
            Assert.AreEqual(2, simpleKey.Count);
            Assert.AreEqual(column1, simpleKey[0].Column);
            Assert.AreEqual(column2, simpleKey[1].Column);
            Assert.AreEqual(SortDirection.Unspecified, simpleKey[0].Direction);
            Assert.AreEqual(SortDirection.Descending, simpleKey[1].Direction);
        }
    }
}

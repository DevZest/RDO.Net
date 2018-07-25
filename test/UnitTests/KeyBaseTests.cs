using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class KeyBaseTests
    {
        private class SimpleKey : PrimaryKey
        {
            public static IDataValues ValueOf(int column1, int column2)
            {
                return DataValues.Create(_Int32.Const(column1), _Int32.Const(column2));
            }

            public SimpleKey(_Int32 column1, _Int32 column2)
                : base(column1, column2.Desc())
            {
            }

            public _Int32 Column1
            {
                get { return GetColumn<_Int32>(0); }
            }

            public _Int32 Column2
            {
                get { return GetColumn<_Int32>(1); }
            }
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

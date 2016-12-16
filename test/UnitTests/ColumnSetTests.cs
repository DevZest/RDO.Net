using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class ColumnSetTests
    {
        [TestMethod]
        public void ColumnSet_New()
        {
            {
                var column1 = new _Int32();
                Assert.AreEqual(column1, ColumnSet.New(column1));
            }

            {
                var column1 = new _Int32();
                var column2 = new _Int32();
                var columnSet = ColumnSet.New(column1, column2);
                Assert.AreEqual(2, columnSet.Count);
                Assert.IsTrue(columnSet.Contains(column1));
                Assert.IsTrue(columnSet.Contains(column2));
            }
        }

        [TestMethod]
        public void ColumnSet_Union()
        {
            {
                Assert.AreEqual(ColumnSet.Empty, ColumnSet.Empty.Union(ColumnSet.Empty));
            }

            {
                var column1 = new _Int32();
                Assert.AreEqual(column1, ColumnSet.Empty.Union(column1));
                Assert.AreEqual(column1, column1.Union(ColumnSet.Empty));
            }

            {
                var column1 = new _Int32();
                Assert.AreEqual(column1, column1.Union(column1));
            }

            {
                var column1 = new _Int32();
                var column2 = new _Int32();
                var columnSet = column1.Union(column2);
                Assert.AreEqual(2, columnSet.Count);
                Assert.IsTrue(columnSet.Contains(column1));
                Assert.IsTrue(columnSet.Contains(column2));
            }
        }
    }
}

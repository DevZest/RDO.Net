using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Primitives
{
    [TestClass]
    public class ColumnSetTests
    {
        [TestMethod]
        public void ColumnSet_New()
        {
            {
                Column[] columns = null;
                Assert.AreEqual(ColumnSet.Empty, ColumnSet.New(columns));
            }

            {
                Column[] columns = new Column[] { null };
                Assert.AreEqual(ColumnSet.Empty, ColumnSet.New(columns));
            }

            {
                Column[] columns = new Column[] { null, null };
                Assert.AreEqual(ColumnSet.Empty, ColumnSet.New(columns));
            }

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
        public void ColumnSet_Merge()
        {
            {
                Assert.AreEqual(ColumnSet.Empty, ColumnSet.Empty.Merge(ColumnSet.Empty));
            }

            {
                var column1 = new _Int32();
                Assert.AreEqual(column1, ColumnSet.Empty.Merge(column1));
                Assert.AreEqual(column1, column1.Merge(ColumnSet.Empty));
            }

            {
                var column1 = new _Int32();
                Assert.AreEqual(column1, column1.Merge(column1));
            }

            {
                var column1 = new _Int32();
                var column2 = new _Int32();
                var columnSet = column1.Merge(column2);
                Assert.AreEqual(2, columnSet.Count);
                Assert.IsTrue(columnSet.Contains(column1));
                Assert.IsTrue(columnSet.Contains(column2));
            }
        }
    }
}

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
                Assert.AreEqual(column1, Columns.New(column1));
            }

            {
                var column1 = new _Int32();
                var column2 = new _Int32();
                var columnSet = Columns.New(column1, column2);
                Assert.AreEqual(2, columnSet.Count);
                Assert.IsTrue(columnSet.Contains(column1));
                Assert.IsTrue(columnSet.Contains(column2));
            }
        }

        [TestMethod]
        public void ColumnSet_Union()
        {
            {
                Assert.AreEqual(Columns.Empty, Columns.Empty.Union(Columns.Empty));
            }

            {
                var column1 = new _Int32();
                Assert.AreEqual(column1, Columns.Empty.Union(column1));
                Assert.AreEqual(column1, column1.Union(Columns.Empty));
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

        [TestMethod]
        public void ColumnSet_IsSubsetOf()
        {
            Assert.IsTrue(Columns.Empty.IsSubsetOf(Columns.Empty));

            var column1 = new _Int32();
            var column2 = new _Int32();
            var column1And2 = Columns.New(column1, column2);

            Assert.IsTrue(column1.IsSubsetOf(column1And2));
            Assert.IsTrue(column2.IsSubsetOf(column1And2));
            Assert.IsTrue(column1And2.IsSubsetOf(column1And2));
        }

        [TestMethod]
        public void ColumnSet_IsProperSubsetOf()
        {
            Assert.IsTrue(Columns.Empty.IsSubsetOf(Columns.Empty));

            var column1 = new _Int32();
            var column2 = new _Int32();
            var column1And2 = Columns.New(column1, column2);

            Assert.IsTrue(column1.IsProperSubsetOf(column1And2));
            Assert.IsTrue(column2.IsProperSubsetOf(column1And2));
            Assert.IsFalse(column1And2.IsProperSubsetOf(column1And2));
        }

        [TestMethod]
        public void ColumnSet_IsSupersetOf()
        {
            Assert.IsTrue(Columns.Empty.IsSubsetOf(Columns.Empty));

            var column1 = new _Int32();
            var column2 = new _Int32();
            var column1And2 = Columns.New(column1, column2);

            Assert.IsTrue(column1And2.IsSupersetOf(column1));
            Assert.IsTrue(column1And2.IsSupersetOf(column2));
            Assert.IsTrue(column1And2.IsSupersetOf(column1And2));
        }

        [TestMethod]
        public void ColumnSet_IsProperSupersetOf()
        {
            Assert.IsTrue(Columns.Empty.IsSubsetOf(Columns.Empty));

            var column1 = new _Int32();
            var column2 = new _Int32();
            var column1And2 = Columns.New(column1, column2);

            Assert.IsTrue(column1And2.IsProperSupersetOf(column1));
            Assert.IsTrue(column1And2.IsProperSupersetOf(column2));
            Assert.IsFalse(column1And2.IsProperSupersetOf(column1And2));
        }
    }
}

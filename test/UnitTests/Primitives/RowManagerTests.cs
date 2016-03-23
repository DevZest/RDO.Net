using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class RowManagerTests : RowManagerTestsBase
    {
        [TestMethod]
        public void RowManager_EofRowMapping_Never()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, EofRowMapping.Never);

            Assert.AreEqual(0, rowManager.Rows.Count);
            Assert.AreEqual(null, rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsEof);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_EofRowMapping_Always()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, EofRowMapping.Always);

            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.AreEqual(true, rowManager.Rows[0].IsEof);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(2, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsEof);
            Assert.IsTrue(rowManager.Rows[1].IsEof);
            Assert.AreEqual(rowManager.Rows[1], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_EofRowMapping_NoData()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, EofRowMapping.NoData);

            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsTrue(rowManager.Rows[0].IsEof);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsEof);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_HierarchicalDataSet()
        {
            var productCategories = MockProductCategories(3);
            var rowManager = CreateRowManager(productCategories);
            VerifyHierarchicalLevel(rowManager, 0, 0, 0);

            rowManager.Rows[0].Expand();
            VerifyHierarchicalLevel(rowManager, 0, 1, 1, 1, 0, 0);

            rowManager.Rows[1].Expand();
            VerifyHierarchicalLevel(rowManager, 0, 1, 2, 2, 2, 1, 1, 0, 0);

            rowManager.Rows[0].Collapse();
            VerifyHierarchicalLevel(rowManager, 0, 0, 0);

            rowManager.Rows[0].Expand();
            VerifyHierarchicalLevel(rowManager, 0, 1, 2, 2, 2, 1, 1, 0, 0);
        }

        private static void VerifyHierarchicalLevel(RowManager rowManager, params int[] hiearchicalLevels)
        {
            var rows = rowManager.Rows;
            Assert.AreEqual(rows.Count, hiearchicalLevels.Length);

            for (int i = 0; i < rows.Count; i++)
                Assert.AreEqual(hiearchicalLevels[i], rows[i].HierarchicalLevel);
        }
    }
}

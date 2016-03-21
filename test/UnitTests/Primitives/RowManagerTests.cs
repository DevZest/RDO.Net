using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class RowManagerTests
    {
        private sealed class ConcreteRowManager : RowManager
        {
            public ConcreteRowManager(DataSet dataSet)
                : base(dataSet)
            {
            }

            internal override void InvalidateView()
            {
            }
        }

        private static RowManager CreateRowManager(DataSet<Adhoc> dataSet, EofRowStrategy eofRowStrategy)
        {
            RowManager result = new ConcreteRowManager(dataSet);
            result.Template.EofRowStrategy = eofRowStrategy;
            result.Initialize();
            return result;
        }

        [TestMethod]
        public void RowManager_EofRowStrategy_Never()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, EofRowStrategy.Never);

            Assert.AreEqual(0, rowManager.MappedRows.Count);
            Assert.AreEqual(null, rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.MappedRows.Count);
            Assert.IsFalse(rowManager.MappedRows[0].IsEof);
            Assert.AreEqual(rowManager.MappedRows[0], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_EofRowStrategy_Always()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, EofRowStrategy.Always);

            Assert.AreEqual(1, rowManager.MappedRows.Count);
            Assert.AreEqual(true, rowManager.MappedRows[0].IsEof);
            Assert.AreEqual(rowManager.MappedRows[0], rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(2, rowManager.MappedRows.Count);
            Assert.IsFalse(rowManager.MappedRows[0].IsEof);
            Assert.IsTrue(rowManager.MappedRows[1].IsEof);
            Assert.AreEqual(rowManager.MappedRows[1], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_EofRowStrategy_NoData()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, EofRowStrategy.NoData);

            Assert.AreEqual(1, rowManager.MappedRows.Count);
            Assert.IsTrue(rowManager.MappedRows[0].IsEof);
            Assert.AreEqual(rowManager.MappedRows[0], rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.MappedRows.Count);
            Assert.IsFalse(rowManager.MappedRows[0].IsEof);
            Assert.AreEqual(rowManager.MappedRows[0], rowManager.CurrentRow);
        }
    }
}

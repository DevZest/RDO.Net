using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class RowManagerTests : RowManagerTestsBase
    {
        [TestMethod]
        public void RowManager_EofRowMapping_Never()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, EofVisibility.Never);

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
            var rowManager = CreateRowManager(dataSet, EofVisibility.Always);

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
            var rowManager = CreateRowManager(dataSet, EofVisibility.NoData);

            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsTrue(rowManager.Rows[0].IsEof);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsEof);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_InsertRow()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var rowManager = CreateRowManager(dataSet, EofVisibility.Always);
            var rows = rowManager.Rows;

            var row = rowManager.InsertRow(0);
            Assert.AreEqual(3, rows.Count);
            Assert.IsTrue(row.IsEditing);

            row.CancelEdit();
            Assert.AreEqual(2, rows.Count);
        }

        [TestMethod]
        public void RowManager_InsertRow_Hierarchical()
        {
            var dataSet = MockProductCategories(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;

            var row = rowManager.InsertRow(1);
            Assert.IsTrue(row.IsEditing);
            VerifyHierarchicalLevel(rows, 0, 0, 0, 0);

            row.CancelEdit();
            VerifyHierarchicalLevel(rows, 0, 0, 0);

            rows[0].Expand();
            try
            {
                rowManager.InsertRow(1);
                Assert.Fail("An exception should be thrown because the ordinal is a child row.");
            }
            catch (ArgumentException)
            {
            }
        }
    }
}

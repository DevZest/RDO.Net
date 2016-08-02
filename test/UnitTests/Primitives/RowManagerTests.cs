﻿using DevZest.Samples.AdventureWorksLT;
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
            var rowManager = CreateRowManager(dataSet, EmptyRowPosition.None);

            Assert.AreEqual(0, rowManager.Rows.Count);
            Assert.AreEqual(null, rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsEmpty);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_EofRowMapping_Always()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, EmptyRowPosition.Bottom);

            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.AreEqual(true, rowManager.Rows[0].IsEmpty);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(2, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsEmpty);
            Assert.IsTrue(rowManager.Rows[1].IsEmpty);
            Assert.AreEqual(rowManager.Rows[1], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_EofRowMapping_NoData()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, EmptyRowPosition.NoData);

            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsTrue(rowManager.Rows[0].IsEmpty);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsEmpty);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_InsertRow()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var rowManager = CreateRowManager(dataSet, EmptyRowPosition.Bottom);
            var rows = rowManager.Rows;

            var row = rowManager.InsertRow(0);
            Assert.AreEqual(3, rows.Count);
        }
    }
}

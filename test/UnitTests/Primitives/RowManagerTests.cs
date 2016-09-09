using DevZest.Data.Windows.Helpers;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class RowManagerTests
    {
        private sealed class ConcreteRowManager : RowManager
        {
            public ConcreteRowManager(Template template, DataSet dataSet, _Boolean where = null, ColumnSort[] orderBy = null)
                : base(template, dataSet, where, orderBy)
            {
            }
        }

        private static RowManager CreateRowManager<T>(DataSet<T> dataSet, RowPlaceholderMode rowPlaceholderPosition)
            where T : Model, new()
        {
            var template = new Template();
            template.RowPlaceholderMode = rowPlaceholderPosition;
            RowManager result = new ConcreteRowManager(template, dataSet);
            return result;
        }

        private static RowManager CreateRowManager<T>(DataSet<T> dataSet, int hierarchicalModelOrdinal = 0)
            where T : Model, new()
        {
            var template = new Template();
            template.RecursiveModelOrdinal = hierarchicalModelOrdinal;
            RowManager result = new ConcreteRowManager(template, dataSet);
            return result;
        }

        [TestMethod]
        public void RowManager_RowPlaceholderMode_Explicit()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, RowPlaceholderMode.Explicit);

            Assert.AreEqual(0, rowManager.Rows.Count);
            Assert.AreEqual(null, rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsPlaceholder);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_RowPlaceholderMode_Tail()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, RowPlaceholderMode.Tail);

            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.AreEqual(true, rowManager.Rows[0].IsPlaceholder);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(2, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsPlaceholder);
            Assert.IsTrue(rowManager.Rows[1].IsPlaceholder);
            Assert.AreEqual(rowManager.Rows[1], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_RowPlaceholderMode_EmptyView()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, RowPlaceholderMode.EmptyView);

            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsTrue(rowManager.Rows[0].IsPlaceholder);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsPlaceholder);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_CommitEdit()
        {
            var dataSet = ProductCategoryDataSet.Mock(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;
            rows[0].Expand();

            Assert.AreEqual("Name-1", rows[0].GetValue(dataSet._.Name));
            Assert.AreEqual("Name-1-1", rows[1].GetValue(dataSet._.Name));

            rows[0].EditValue(dataSet._.Name, "NewName-1");
            rowManager.CommitEdit();
            rows[1].EditValue(dataSet._.Name, "NewName-1-1");
            rowManager.CommitEdit();
            Assert.AreEqual("NewName-1", rows[0].GetValue(dataSet._.Name));
            Assert.AreEqual("NewName-1-1", rows[1].GetValue(dataSet._.Name));
        }

        [TestMethod]
        public void RowManager_CommitEdit_by_row_presenter_indexer()
        {
            var dataSet = ProductCategoryDataSet.Mock(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;
            rows[0].Expand();

            Assert.AreEqual("Name-1", rows[0][dataSet._.Name]);
            Assert.AreEqual("Name-1-1", rows[1][dataSet._.Name]);

            rows[0][dataSet._.Name] = "NewName-1";
            rowManager.CommitEdit();
            rows[1][dataSet._.Name] = "NewName-1-1";
            rowManager.CommitEdit();
            Assert.AreEqual("NewName-1", rows[0][dataSet._.Name]);
            Assert.AreEqual("NewName-1-1", rows[1][dataSet._.Name]);
        }

        [TestMethod]
        public void RowManager_CancelEdit()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var rowManager = CreateRowManager(dataSet, RowPlaceholderMode.Tail);

            var row = rowManager.Rows[0];
            Assert.AreEqual(SalesOrderStatus.Shipped, row.GetValue(dataSet._.Status));

            row.EditValue(dataSet._.Status, SalesOrderStatus.InProcess);
            Assert.AreEqual(SalesOrderStatus.InProcess, row.GetValue(dataSet._.Status));
            Assert.IsTrue(row.IsEditing);
            rowManager.RollbackEdit();
            Assert.IsFalse(row.IsEditing);
            Assert.AreEqual(SalesOrderStatus.Shipped, row.GetValue(dataSet._.Status));
        }

        [TestMethod]
        public void RowManager_CancelEdit_TailPlaceholder()
        {
            var dataSet = DataSet<SalesOrder>.New();
            var rowManager = CreateRowManager(dataSet, RowPlaceholderMode.Tail);

            var rows = rowManager.Rows;
            var row = rows[0];
            Assert.AreEqual(1, rows.Count);
            Assert.IsTrue(row.IsPlaceholder);

            row.EditValue(dataSet._.Status, SalesOrderStatus.InProcess);
            Assert.IsTrue(row.IsPlaceholder);
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual(SalesOrderStatus.InProcess, row.GetValue(dataSet._.Status));
            Assert.IsTrue(row.IsEditing);
            rowManager.RollbackEdit();
            Assert.IsFalse(row.IsEditing);
            Assert.IsTrue(row.IsPlaceholder);
            Assert.AreEqual(1, rows.Count);
        }

        [TestMethod]
        public void RowManager_InsertRow()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var rowManager = CreateRowManager(dataSet, RowPlaceholderMode.Tail);
            var rows = rowManager.Rows;

            //var row = rowManager.InsertRow(0);
            Assert.AreEqual(3, rows.Count);
        }
    }
}

using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        private static RowManager CreateRowManager<T>(DataSet<T> dataSet, VirtualRowPlacement virtualRowPlacement)
            where T : Model, new()
        {
            var template = new Template();
            template.VirtualRowPlacement = virtualRowPlacement;
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
        public void RowManager_VirtualRowPlacement_Explicit()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, VirtualRowPlacement.Explicit);

            Assert.AreEqual(0, rowManager.Rows.Count);
            Assert.AreEqual(null, rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsVirtual);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_VirtualRowPlacement_Tail()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, VirtualRowPlacement.Tail);

            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.AreEqual(true, rowManager.Rows[0].IsVirtual);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(2, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsVirtual);
            Assert.IsTrue(rowManager.Rows[1].IsVirtual);
            Assert.AreEqual(rowManager.Rows[1], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_VirtualRowPlacement_EmptyView()
        {
            var dataSet = DataSet<Adhoc>.New();
            var rowManager = CreateRowManager(dataSet, VirtualRowPlacement.EmptyView);

            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsTrue(rowManager.Rows[0].IsVirtual);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, rowManager.Rows.Count);
            Assert.IsFalse(rowManager.Rows[0].IsVirtual);
            Assert.AreEqual(rowManager.Rows[0], rowManager.CurrentRow);
        }

        [TestMethod]
        public void RowManager_CommitEdit()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;
            rows[0].Expand();

            Assert.AreEqual("Name-1", rows[0].GetValue(dataSet._.Name));
            Assert.AreEqual("Name-1-1", rows[1].GetValue(dataSet._.Name));

            rows[0].EditValue(dataSet._.Name, "NewName-1");
            rowManager.CommitEdit();
            rowManager.CurrentRow = rows[1];
            rows[1].EditValue(dataSet._.Name, "NewName-1-1");
            rowManager.CommitEdit();
            Assert.AreEqual("NewName-1", rows[0].GetValue(dataSet._.Name));
            Assert.AreEqual("NewName-1-1", rows[1].GetValue(dataSet._.Name));
        }

        [TestMethod]
        public void RowManager_CommitEdit_by_row_presenter_indexer()
        {
            var dataSet = DataSetMock.ProductCategories(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;
            rows[0].Expand();

            Assert.AreEqual("Name-1", rows[0][dataSet._.Name]);
            Assert.AreEqual("Name-1-1", rows[1][dataSet._.Name]);

            rows[0][dataSet._.Name] = "NewName-1";
            rowManager.CommitEdit();
            rowManager.CurrentRow = rows[1];
            rows[1][dataSet._.Name] = "NewName-1-1";
            rowManager.CommitEdit();
            Assert.AreEqual("NewName-1", rows[0][dataSet._.Name]);
            Assert.AreEqual("NewName-1-1", rows[1][dataSet._.Name]);
        }

        [TestMethod]
        public void RowManager_CancelEdit()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var rowManager = CreateRowManager(dataSet, VirtualRowPlacement.Tail);

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
        public void RowManager_CancelEdit_TailVirtualRowPlacement()
        {
            var dataSet = DataSet<SalesOrder>.New();
            var rowManager = CreateRowManager(dataSet, VirtualRowPlacement.Tail);

            var rows = rowManager.Rows;
            var row = rows[0];
            Assert.AreEqual(1, rows.Count);
            Assert.IsTrue(row.IsVirtual);

            row.EditValue(dataSet._.Status, SalesOrderStatus.InProcess);
            Assert.IsTrue(row.IsVirtual);
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual(SalesOrderStatus.InProcess, row.GetValue(dataSet._.Status));
            Assert.IsTrue(row.IsEditing);
            rowManager.RollbackEdit();
            Assert.IsFalse(row.IsEditing);
            Assert.IsTrue(row.IsVirtual);
            Assert.AreEqual(1, rows.Count);
        }

        [TestMethod]
        public void RowManager_InsertBefore()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var rowManager = CreateRowManager(dataSet, VirtualRowPlacement.Tail);
            var rows = rowManager.Rows;

            rowManager.BeginInsertBefore(null, rows[0]);
            Assert.AreEqual(2, rows.Count);
            Assert.IsTrue(rows[0].IsVirtual);
            rowManager.RollbackEdit();
            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual(dataSet[0], rows[0].DataRow);
            Assert.IsTrue(rows[1].IsVirtual);

            rowManager.BeginInsertBefore(null, rows[0]);
            Assert.AreEqual(2, rows.Count);
            Assert.IsTrue(rows[0].IsVirtual);
            rowManager.CommitEdit();
            Assert.AreEqual(3, rows.Count);
            Assert.AreEqual(dataSet[0], rows[0].DataRow);
            Assert.AreEqual(dataSet[1], rows[1].DataRow);
            Assert.IsTrue(rows[2].IsVirtual);
        }

        [TestMethod]
        public void RowManager_InsertAfter()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var rowManager = CreateRowManager(dataSet, VirtualRowPlacement.Tail);
            var rows = rowManager.Rows;

            rowManager.BeginInsertAfter(null, rows[0]);
            Assert.AreEqual(2, rows.Count);
            Assert.IsTrue(rows[1].IsVirtual);
            rowManager.RollbackEdit();
            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual(dataSet[0], rows[0].DataRow);
            Assert.IsTrue(rows[1].IsVirtual);

            rowManager.BeginInsertAfter(null, rows[0]);
            Assert.AreEqual(2, rows.Count);
            Assert.IsTrue(rows[1].IsVirtual);
            rowManager.CommitEdit();
            Assert.AreEqual(3, rows.Count);
            Assert.AreEqual(dataSet[0], rows[0].DataRow);
            Assert.AreEqual(dataSet[1], rows[1].DataRow);
            Assert.IsTrue(rows[2].IsVirtual);
        }
    }
}

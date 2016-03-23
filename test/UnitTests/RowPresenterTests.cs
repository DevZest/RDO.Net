using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class RowPresenterTests : RowManagerTestsBase
    {
        [TestMethod]
        public void RowPresenter_GetSetValue()
        {
            var dataSet = MockProductCategories(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;
            rows[0].Expand();
            VerifyHierarchicalLevel(rows, 0, 1, 1, 1, 0, 0);

            Assert.AreEqual("Name-1", rows[0].GetValue(dataSet._.Name));
            Assert.AreEqual("Name-1-1", rows[1].GetValue(dataSet._.Name));

            rows[0].SetValue(dataSet._.Name, "NewName-1");
            rows[1].SetValue(dataSet._.Name, "NewName-1-1");
            Assert.AreEqual("NewName-1", rows[0].GetValue(dataSet._.Name));
            Assert.AreEqual("NewName-1-1", rows[1].GetValue(dataSet._.Name));
        }

        [TestMethod]
        public void RowPresenter_GetSetValue_Object()
        {
            var dataSet = MockProductCategories(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;
            rows[0].Expand();
            VerifyHierarchicalLevel(rows, 0, 1, 1, 1, 0, 0);

            Assert.AreEqual("Name-1", rows[0][dataSet._.Name]);
            Assert.AreEqual("Name-1-1", rows[1][dataSet._.Name]);

            rows[0][dataSet._.Name] = "NewName-1";
            rows[1][dataSet._.Name] = "NewName-1-1";
            Assert.AreEqual("NewName-1", rows[0][dataSet._.Name]);
            Assert.AreEqual("NewName-1-1", rows[1][dataSet._.Name]);
        }

        [TestMethod]
        public void RowPresenter_CancelEdit()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var rowManager = CreateRowManager(dataSet, EofRowMapping.Always);

            var row = rowManager.Rows[0];
            Assert.AreEqual(SalesOrderStatus.Shipped, row.GetValue(dataSet._.Status));

            row.SetValue(dataSet._.Status, SalesOrderStatus.InProcess);
            Assert.AreEqual(SalesOrderStatus.InProcess, row.GetValue(dataSet._.Status));
            Assert.IsTrue(row.IsEditing);
            row.CancelEdit();
            Assert.IsFalse(row.IsEditing);
            Assert.AreEqual(SalesOrderStatus.Shipped, row.GetValue(dataSet._.Status));
        }

        [TestMethod]
        public void RowPresenter_CancelEdit_Eof()
        {
            var dataSet = DataSet<SalesOrder>.New();
            var rowManager = CreateRowManager(dataSet, EofRowMapping.Always);

            var rows = rowManager.Rows;
            var row = rows[0];
            Assert.AreEqual(1, rows.Count);
            Assert.IsTrue(row.IsEof);

            row.SetValue(dataSet._.Status, SalesOrderStatus.InProcess);
            Assert.IsFalse(row.IsEof);
            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual(SalesOrderStatus.InProcess, row.GetValue(dataSet._.Status));
            Assert.IsTrue(row.IsEditing);
            row.CancelEdit();
            Assert.IsFalse(row.IsEditing);
            Assert.IsTrue(row.IsEof);
            Assert.AreEqual(1, rows.Count);
        }

        [TestMethod]
        public void RowPresenter_Expand_Collapse()
        {
            var productCategories = MockProductCategories(3);
            var rowManager = CreateRowManager(productCategories);
            var rows = rowManager.Rows;
            VerifyHierarchicalLevel(rows, 0, 0, 0);

            rowManager.Rows[0].Expand();
            VerifyHierarchicalLevel(rows, 0, 1, 1, 1, 0, 0);

            rowManager.Rows[1].Expand();
            VerifyHierarchicalLevel(rows, 0, 1, 2, 2, 2, 1, 1, 0, 0);

            rowManager.Rows[0].Collapse();
            VerifyHierarchicalLevel(rows, 0, 0, 0);

            rowManager.Rows[0].Expand();
            VerifyHierarchicalLevel(rows, 0, 1, 2, 2, 2, 1, 1, 0, 0);
        }

        [TestMethod]
        public void RowPresenter_Delete()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var rowManager = CreateRowManager(dataSet, EofRowMapping.Never);
            var rows = rowManager.Rows;
            Assert.AreEqual(1, rows.Count);
            rows[0].Delete();
            Assert.AreEqual(0, rows.Count);
        }

        [TestMethod]
        public void RowPresenter_Delete_Hierarchical()
        {
            var dataSet = MockProductCategories(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;
            VerifyRowOrdinal(rows);
            VerifyHierarchicalLevel(rows, 0, 0, 0);
            rows[0].Delete();
            VerifyRowOrdinal(rows);
            VerifyHierarchicalLevel(rows, 0, 0);
        }

        [TestMethod]
        public void RowPresenter_InsertChildRow()
        {
            var dataSet = MockProductCategories(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;
            rows[0].Expand();
            VerifyHierarchicalLevel(rows, 0, 1, 1, 1, 0, 0);

            var newChildRow = rows[0].InsertChildRow(1);
            VerifyHierarchicalLevel(rows, 0, 1, 1, 1, 1, 0, 0);

            newChildRow.CancelEdit();
            VerifyHierarchicalLevel(rows, 0, 1, 1, 1, 0, 0);
        }
    }
}

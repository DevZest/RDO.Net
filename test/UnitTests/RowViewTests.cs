using DevZest.Data.Windows.Factories;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class RowViewTests
    {
        [TestMethod]
        public void RowView_CancelEdit()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var dataView = DataPresenter.Create(dataSet, (builder, model) =>
            {
                builder.WithEofVisible(true);
            });

            var row = dataView[0];
            Assert.AreEqual(SalesOrderStatus.Shipped, row.GetValue(dataSet._.Status));

            row.SetValue(dataSet._.Status, SalesOrderStatus.InProcess);
            Assert.AreEqual(SalesOrderStatus.InProcess, row.GetValue(dataSet._.Status));
            Assert.IsTrue(row.IsEditing);
            row.CancelEdit();
            Assert.IsFalse(row.IsEditing);
            Assert.AreEqual(SalesOrderStatus.Shipped, row.GetValue(dataSet._.Status));
        }

        [TestMethod]
        public void RowView_CancelEdit_Eof()
        {
            var dataSet = DataSet<SalesOrder>.New();
            var dataView = DataPresenter.Create(dataSet, (builder, model) =>
            {
                builder.WithEofVisible(true);
            });

            var row = dataView[0];
            Assert.AreEqual(RowKind.Eof, row.Kind);

            row.SetValue(dataSet._.Status, SalesOrderStatus.InProcess);
            Assert.AreEqual(RowKind.DataRow, row.Kind);
            Assert.AreEqual(2, dataView.Count);
            Assert.AreEqual(SalesOrderStatus.InProcess, row.GetValue(dataSet._.Status));
            Assert.IsTrue(row.IsEditing);
            row.CancelEdit();
            Assert.IsFalse(row.IsEditing);
            Assert.AreEqual(RowKind.Eof, row.Kind);
            Assert.AreEqual(1, dataView.Count);
        }
    }
}

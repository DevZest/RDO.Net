using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class DataSetPresenterTests
    {
        [TestMethod]
        public void DataSetPresenter_RowType_DataRow()
        {
            var dataSet = DataSet<Adhoc>.New();

            var dataSetPresenter = DataSetPresenter.Create(dataSet, (c, m) => c.WithEofVisible(false).WithEmptySetVisible(false));

            Assert.AreEqual(0, dataSetPresenter.Count);
            Assert.AreEqual(-1, dataSetPresenter.Current);
            Assert.AreEqual(0, dataSetPresenter.Selection.Count);

            dataSet.AddRow();
            Assert.AreEqual(1, dataSetPresenter.Count);
            Assert.AreEqual(RowType.DataRow, dataSetPresenter[0].RowType);
            Assert.AreEqual(0, dataSetPresenter.Current);
            Assert.AreEqual(1, dataSetPresenter.Selection.Count);
        }

        [TestMethod]
        public void DataSetPresenter_RowType_Eof()
        {
            var dataSet = DataSet<Adhoc>.New();

            var dataSetPresenter = DataSetPresenter.Create(dataSet, (c, m) => c.WithEofVisible(true));

            Assert.AreEqual(1, dataSetPresenter.Count);
            Assert.AreEqual(RowType.Eof, dataSetPresenter[0].RowType);
            Assert.AreEqual(0, dataSetPresenter.Current);
            Assert.AreEqual(0, dataSetPresenter.Selection.Count);

            dataSet.AddRow();
            Assert.AreEqual(2, dataSetPresenter.Count);
            Assert.AreEqual(RowType.DataRow, dataSetPresenter[0].RowType);
            Assert.AreEqual(RowType.Eof, dataSetPresenter[1].RowType);
            Assert.AreEqual(0, dataSetPresenter.Current);
            Assert.AreEqual(1, dataSetPresenter.Selection.Count);
        }

        [TestMethod]
        public void DataSetPresenter_RowType_EmptySet()
        {
            var dataSet = DataSet<Adhoc>.New();

            var dataSetPresenter = DataSetPresenter.Create(dataSet, (c, m) => c.WithEofVisible(false).WithEmptySetVisible(true));

            Assert.AreEqual(1, dataSetPresenter.Count);
            Assert.AreEqual(RowType.EmptySet, dataSetPresenter[0].RowType);
            Assert.AreEqual(0, dataSetPresenter.Current);
            Assert.AreEqual(0, dataSetPresenter.Selection.Count);

            dataSet.AddRow();
            Assert.AreEqual(1, dataSetPresenter.Count);
            Assert.AreEqual(RowType.DataRow, dataSetPresenter[0].RowType);
            Assert.AreEqual(0, dataSetPresenter.Current);
            Assert.AreEqual(1, dataSetPresenter.Selection.Count);
        }
    }
}

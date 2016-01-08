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
            Assert.AreEqual(null, dataSetPresenter.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, dataSetPresenter.Count);
            Assert.AreEqual(RowType.DataRow, dataSetPresenter[0].RowType);
            Assert.AreEqual(dataSetPresenter[0], dataSetPresenter.CurrentRow);
        }

        [TestMethod]
        public void DataSetPresenter_RowType_Eof()
        {
            var dataSet = DataSet<Adhoc>.New();

            var dataSetPresenter = DataSetPresenter.Create(dataSet, (c, m) => c.WithEofVisible(true));

            Assert.AreEqual(1, dataSetPresenter.Count);
            Assert.AreEqual(RowType.Eof, dataSetPresenter[0].RowType);
            Assert.AreEqual(dataSetPresenter[0], dataSetPresenter.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(2, dataSetPresenter.Count);
            Assert.AreEqual(RowType.DataRow, dataSetPresenter[0].RowType);
            Assert.AreEqual(RowType.Eof, dataSetPresenter[1].RowType);
            Assert.AreEqual(dataSetPresenter[1], dataSetPresenter.CurrentRow);
        }

        [TestMethod]
        public void DataSetPresenter_RowType_EmptySet()
        {
            var dataSet = DataSet<Adhoc>.New();

            var dataSetPresenter = DataSetPresenter.Create(dataSet, (c, m) => c.WithEofVisible(false).WithEmptySetVisible(true));

            Assert.AreEqual(1, dataSetPresenter.Count);
            Assert.AreEqual(RowType.EmptySet, dataSetPresenter[0].RowType);
            Assert.AreEqual(dataSetPresenter[0], dataSetPresenter.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, dataSetPresenter.Count);
            Assert.AreEqual(RowType.DataRow, dataSetPresenter[0].RowType);
            Assert.AreEqual(dataSetPresenter[0], dataSetPresenter.CurrentRow);
        }
    }
}

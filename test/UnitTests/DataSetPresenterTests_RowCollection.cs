using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class DataSetPresenterTests_RowCollection
    {
        [TestMethod]
        public void DataSetPresenter_RowCollection_data_row_only()
        {
            var dataSet = DataSet<Adhoc>.New();

            var dataSetPresenter = DataSetPresenter.Create(dataSet, (c, m) => c.WithEofVisible(false).WithEmptySetVisible(false));

            Assert.AreEqual(0, dataSetPresenter.Count);
            Assert.AreEqual(-1, dataSetPresenter.Current);
            Assert.AreEqual(0, dataSetPresenter.Selection.Count);

            dataSet.AddRow();
            Assert.AreEqual(1, dataSetPresenter.Count);
            Assert.AreEqual(DataViewRowType.DataRow, dataSetPresenter[0].RowType);
            Assert.AreEqual(0, dataSetPresenter.Current);
            Assert.AreEqual(1, dataSetPresenter.Selection.Count);
        }

        [TestMethod]
        public void DataSetPresenter_RowCollection_eof()
        {
            var dataSet = DataSet<Adhoc>.New();

            var dataSetPresenter = DataSetPresenter.Create(dataSet, (c, m) => c.WithEofVisible(true));

            Assert.AreEqual(1, dataSetPresenter.Count);
            Assert.AreEqual(DataViewRowType.Eof, dataSetPresenter[0].RowType);
            Assert.AreEqual(0, dataSetPresenter.Current);
            Assert.AreEqual(0, dataSetPresenter.Selection.Count);

            dataSet.AddRow();
            Assert.AreEqual(2, dataSetPresenter.Count);
            Assert.AreEqual(DataViewRowType.DataRow, dataSetPresenter[0].RowType);
            Assert.AreEqual(DataViewRowType.Eof, dataSetPresenter[1].RowType);
            Assert.AreEqual(0, dataSetPresenter.Current);
            Assert.AreEqual(1, dataSetPresenter.Selection.Count);
        }

        [TestMethod]
        public void DataSetPresenter_RowCollection_empty_data_row()
        {
            var dataSet = DataSet<Adhoc>.New();

            var dataSetPresenter = DataSetPresenter.Create(dataSet, (c, m) => c.WithEofVisible(false).WithEmptySetVisible(true));

            Assert.AreEqual(1, dataSetPresenter.Count);
            Assert.AreEqual(DataViewRowType.EmptySet, dataSetPresenter[0].RowType);
            Assert.AreEqual(0, dataSetPresenter.Current);
            Assert.AreEqual(0, dataSetPresenter.Selection.Count);

            dataSet.AddRow();
            Assert.AreEqual(1, dataSetPresenter.Count);
            Assert.AreEqual(DataViewRowType.DataRow, dataSetPresenter[0].RowType);
            Assert.AreEqual(0, dataSetPresenter.Current);
            Assert.AreEqual(1, dataSetPresenter.Selection.Count);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class DataRowPresenterCollectionTests
    {
        private static DataRowPresenterCollection Create<T>(DataSet<T> dataSet, Action<DataSetPresenterBuilder, T> builder = null)
            where T : Model, new()
        {
            var dataSetPresenter = DataSetPresenter.Create(dataSet, builder);
            return new DataRowPresenterCollection(dataSetPresenter);
        }

        [TestMethod]
        public void DataRowPresenterCollection_data_row_only()
        {
            var dataSet = DataSet<Adhoc>.New();

            var dataRowPresenters = Create(dataSet, (c, m) => c.WithEofVisible(false).WithEmptySetVisible(false));

            Assert.AreEqual(0, dataRowPresenters.Count);
            Assert.AreEqual(-1, dataRowPresenters.Current);
            Assert.AreEqual(0, dataRowPresenters.Selection.Count);

            dataSet.AddRow();
            Assert.AreEqual(1, dataRowPresenters.Count);
            Assert.AreEqual(DataViewRowType.DataRow, dataRowPresenters[0].RowType);
            Assert.AreEqual(0, dataRowPresenters.Current);
            Assert.AreEqual(1, dataRowPresenters.Selection.Count);
        }

        [TestMethod]
        public void DataRowPresenterCollection_shows_eof()
        {
            var dataSet = DataSet<Adhoc>.New();

            var dataRowPresenters = Create(dataSet, (c, m) => c.WithEofVisible(true));

            Assert.AreEqual(1, dataRowPresenters.Count);
            Assert.AreEqual(DataViewRowType.Eof, dataRowPresenters[0].RowType);
            Assert.AreEqual(0, dataRowPresenters.Current);
            Assert.AreEqual(0, dataRowPresenters.Selection.Count);

            dataSet.AddRow();
            Assert.AreEqual(2, dataRowPresenters.Count);
            Assert.AreEqual(DataViewRowType.DataRow, dataRowPresenters[0].RowType);
            Assert.AreEqual(DataViewRowType.Eof, dataRowPresenters[1].RowType);
            Assert.AreEqual(0, dataRowPresenters.Current);
            Assert.AreEqual(1, dataRowPresenters.Selection.Count);
        }

        [TestMethod]
        public void DataRowPresenterCollection_shows_empty_data_row()
        {
            var dataSet = DataSet<Adhoc>.New();

            var dataRowPresenters = Create(dataSet, (c, m) => c.WithEofVisible(false).WithEmptySetVisible(true));

            Assert.AreEqual(1, dataRowPresenters.Count);
            Assert.AreEqual(DataViewRowType.EmptySet, dataRowPresenters[0].RowType);
            Assert.AreEqual(0, dataRowPresenters.Current);
            Assert.AreEqual(0, dataRowPresenters.Selection.Count);

            dataSet.AddRow();
            Assert.AreEqual(1, dataRowPresenters.Count);
            Assert.AreEqual(DataViewRowType.DataRow, dataRowPresenters[0].RowType);
            Assert.AreEqual(0, dataRowPresenters.Current);
            Assert.AreEqual(1, dataRowPresenters.Selection.Count);
        }
    }
}

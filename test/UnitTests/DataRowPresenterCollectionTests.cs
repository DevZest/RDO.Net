using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class DataRowPresenterCollectionTests
    {
        private static DataRowPresenterCollection Create<T>(DataSet<T> dataSet, Action<GridTemplate> templateInitializer = null)
            where T : Model, new()
        {
            var dataSetPresenter = Util.CreateDataSetPresenter(dataSet, templateInitializer);
            return new DataRowPresenterCollection(dataSetPresenter);
        }

        [TestMethod]
        public void DataRowPresenterCollection_data_row_only()
        {
            var dataSet = DataSet<Adhoc>.New();

            var dataRowPresenters = Create(dataSet, t => t.WithShowsEof(false).WithShowsEmptyDataRow(false));

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

            var dataRowPresenters = Create(dataSet, t => t.WithShowsEof(true));

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

            var dataRowPresenters = Create(dataSet, t => t.WithShowsEof(false).WithShowsEmptyDataRow(true));

            Assert.AreEqual(1, dataRowPresenters.Count);
            Assert.AreEqual(DataViewRowType.EmptyDataRow, dataRowPresenters[0].RowType);
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

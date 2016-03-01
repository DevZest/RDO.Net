using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class DataPresenterRowCollectionTests
    {
        private static DataPresenter CreateDataPresenter(DataSet<Adhoc> dataSet, bool isEofVisible, bool isEmptySetVisible = false)
        {
            return DataPresenter.Create(dataSet, (builder, model) => builder.WithEofVisible(isEofVisible).WithEmptySetVisible(isEmptySetVisible));
        }

        [TestMethod]
        public void DataPresenter_RowCollection_RowType_DataRow()
        {
            var dataSet = DataSet<Adhoc>.New();
            var dataPresenter = CreateDataPresenter(dataSet, false, false);

            Assert.AreEqual(0, dataPresenter.Count);
            Assert.AreEqual(null, dataPresenter.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, dataPresenter.Count);
            Assert.AreEqual(RowKind.DataRow, dataPresenter[0].Kind);
            Assert.AreEqual(dataPresenter[0], dataPresenter.CurrentRow);
        }

        [TestMethod]
        public void DataPresenter_RowCollection_RowType_Eof()
        {
            var dataSet = DataSet<Adhoc>.New();
            var dataPresenter = CreateDataPresenter(dataSet, true);

            Assert.AreEqual(1, dataPresenter.Count);
            Assert.AreEqual(RowKind.Eof, dataPresenter[0].Kind);
            Assert.AreEqual(dataPresenter[0], dataPresenter.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(2, dataPresenter.Count);
            Assert.AreEqual(RowKind.DataRow, dataPresenter[0].Kind);
            Assert.AreEqual(RowKind.Eof, dataPresenter[1].Kind);
            Assert.AreEqual(dataPresenter[1], dataPresenter.CurrentRow);
        }

        [TestMethod]
        public void DataPresenter_RowCollection_RowType_EmptySet()
        {
            var dataSet = DataSet<Adhoc>.New();
            var dataPresenter = CreateDataPresenter(dataSet, false, true);

            Assert.AreEqual(1, dataPresenter.Count);
            Assert.AreEqual(RowKind.EmptySet, dataPresenter[0].Kind);
            Assert.AreEqual(dataPresenter[0], dataPresenter.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, dataPresenter.Count);
            Assert.AreEqual(RowKind.DataRow, dataPresenter[0].Kind);
            Assert.AreEqual(dataPresenter[0], dataPresenter.CurrentRow);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.Windows
{
    [TestClass]
    public class DataViewRowCollectionTests
    {
        private static DataView CreateDataView(DataSet<Adhoc> dataSet, bool isEofVisible, bool isEmptySetVisible = false)
        {
            return DataView.Create(dataSet, (builder, model) => builder.WithEofVisible(isEofVisible).WithEmptySetVisible(isEmptySetVisible));
        }

        [TestMethod]
        public void DataView_RowCollection_RowType_DataRow()
        {
            var dataSet = DataSet<Adhoc>.New();
            var dataView = CreateDataView(dataSet, false, false);

            Assert.AreEqual(0, dataView.Count);
            Assert.AreEqual(null, dataView.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, dataView.Count);
            Assert.AreEqual(RowType.DataRow, dataView[0].RowType);
            Assert.AreEqual(dataView[0], dataView.CurrentRow);
        }

        [TestMethod]
        public void DataView_RowCollection_RowType_Eof()
        {
            var dataSet = DataSet<Adhoc>.New();
            var dataView = CreateDataView(dataSet, true);

            Assert.AreEqual(1, dataView.Count);
            Assert.AreEqual(RowType.Eof, dataView[0].RowType);
            Assert.AreEqual(dataView[0], dataView.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(2, dataView.Count);
            Assert.AreEqual(RowType.DataRow, dataView[0].RowType);
            Assert.AreEqual(RowType.Eof, dataView[1].RowType);
            Assert.AreEqual(dataView[1], dataView.CurrentRow);
        }

        [TestMethod]
        public void DataView_RowCollection_RowType_EmptySet()
        {
            var dataSet = DataSet<Adhoc>.New();
            var dataView = CreateDataView(dataSet, false, true);

            Assert.AreEqual(1, dataView.Count);
            Assert.AreEqual(RowType.EmptySet, dataView[0].RowType);
            Assert.AreEqual(dataView[0], dataView.CurrentRow);

            dataSet.AddRow();
            Assert.AreEqual(1, dataView.Count);
            Assert.AreEqual(RowType.DataRow, dataView[0].RowType);
            Assert.AreEqual(dataView[0], dataView.CurrentRow);
        }
    }
}

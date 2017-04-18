using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace DevZest.Data
{
    [TestClass]
    public class DataRowTests : SimpleModelDataSetHelper
    {
        [TestMethod]
        public void DataRow_ToString()
        {
            var dataSet = GetDataSet(3);

            var dataRow = dataSet[1];
            Assert.AreEqual("/[1]", dataRow.ToString());

            var childDataRow = dataRow[0][1];
            Assert.AreEqual("/[1]/Child[1]", childDataRow.ToString());

            var grandChildDataRow = childDataRow[0][1];
            Assert.AreEqual("/[1]/Child[1]/Child[1]", grandChildDataRow.ToString());
        }

        [TestMethod]
        public void DataRow_FromString()
        {
            var dataSet = GetDataSet(3);

            var dataRow = dataSet[1];
            Assert.IsTrue(DataRow.FromString(dataSet, "/[1]") == dataRow);

            var childDataRow = dataRow[0][1];
            Assert.IsTrue(DataRow.FromString(dataSet, "/[1]/Child[1]") == childDataRow);

            var grandChildDataRow = childDataRow[0][1];
            Assert.IsTrue(DataRow.FromString(dataSet, "/[1]/Child[1]/Child[1]") == grandChildDataRow);
        }

        [TestMethod]
        public void DataRow_CopyValuesFrom()
        {
            var dataSet1 = GetDataSet(3);
            var dataSet2 = dataSet1.Clone();

            dataSet2.AddRow(x => x.CopyValuesFrom(dataSet1[1]));
            dataSet1.RemoveAt(2);
            dataSet1.RemoveAt(0);
            Assert.AreEqual(dataSet1.ToJsonString(true), dataSet2.ToJsonString(true));
        }

        [TestMethod]
        public void DataRow_Move()
        {
            {
                // Move forward with children
                var dataSet = GetDataSet(3);
                var log = dataSet._.StartLog(3);
                dataSet[0].Move(2);
                Assert.AreEqual(1, dataSet._.Id[0]);
                Assert.AreEqual(2, dataSet._.Id[1]);
                Assert.AreEqual(0, dataSet._.Id[2]);
                Assert.AreEqual(3, dataSet[2].Children(dataSet._.Child).Count);
                var expectedLog =
@"DataRowRemoving: DataSet-0[0].
DataRowRemoving: DataSet-1[2].
DataRowRemoving: DataSet-2[8].
DataRowRemoved: DataSet-2[8].
DataRowRemoving: DataSet-2[7].
DataRowRemoved: DataSet-2[7].
DataRowRemoving: DataSet-2[6].
DataRowRemoved: DataSet-2[6].
DataRowRemoved: DataSet-1[2].
DataRowRemoving: DataSet-1[1].
DataRowRemoving: DataSet-2[5].
DataRowRemoved: DataSet-2[5].
DataRowRemoving: DataSet-2[4].
DataRowRemoved: DataSet-2[4].
DataRowRemoving: DataSet-2[3].
DataRowRemoved: DataSet-2[3].
DataRowRemoved: DataSet-1[1].
DataRowRemoving: DataSet-1[0].
DataRowRemoving: DataSet-2[2].
DataRowRemoved: DataSet-2[2].
DataRowRemoving: DataSet-2[1].
DataRowRemoved: DataSet-2[1].
DataRowRemoving: DataSet-2[0].
DataRowRemoved: DataSet-2[0].
DataRowRemoved: DataSet-1[0].
DataRowRemoved: DataSet-0[0].
DataRowInserting: DataSet-0[2].
DataRowInserting: DataSet-1[6].
DataRowInserting: DataSet-2[18].
BeforeDataRowInserted: DataSet-2[18].
AfterDataRowInserted: DataSet-2[18].
DataRowInserting: DataSet-2[19].
BeforeDataRowInserted: DataSet-2[19].
AfterDataRowInserted: DataSet-2[19].
DataRowInserting: DataSet-2[20].
BeforeDataRowInserted: DataSet-2[20].
AfterDataRowInserted: DataSet-2[20].
BeforeDataRowInserted: DataSet-1[6].
AfterDataRowInserted: DataSet-1[6].
DataRowInserting: DataSet-1[7].
DataRowInserting: DataSet-2[21].
BeforeDataRowInserted: DataSet-2[21].
AfterDataRowInserted: DataSet-2[21].
DataRowInserting: DataSet-2[22].
BeforeDataRowInserted: DataSet-2[22].
AfterDataRowInserted: DataSet-2[22].
DataRowInserting: DataSet-2[23].
BeforeDataRowInserted: DataSet-2[23].
AfterDataRowInserted: DataSet-2[23].
BeforeDataRowInserted: DataSet-1[7].
AfterDataRowInserted: DataSet-1[7].
DataRowInserting: DataSet-1[8].
DataRowInserting: DataSet-2[24].
BeforeDataRowInserted: DataSet-2[24].
AfterDataRowInserted: DataSet-2[24].
DataRowInserting: DataSet-2[25].
BeforeDataRowInserted: DataSet-2[25].
AfterDataRowInserted: DataSet-2[25].
DataRowInserting: DataSet-2[26].
BeforeDataRowInserted: DataSet-2[26].
AfterDataRowInserted: DataSet-2[26].
BeforeDataRowInserted: DataSet-1[8].
AfterDataRowInserted: DataSet-1[8].
BeforeDataRowInserted: DataSet-0[2].
AfterDataRowInserted: DataSet-0[2].
";
                Assert.AreEqual(expectedLog, log.ToString());
            }

            {
                // Move backward with children
                var dataSet = GetDataSet(3);
                var log = dataSet._.StartLog(3);
                dataSet[1].Move(-1);
                Assert.AreEqual(1, dataSet._.Id[0]);
                Assert.AreEqual(3, dataSet[0].Children(dataSet._.Child).Count);
                Assert.AreEqual(0, dataSet._.Id[1]);
                Assert.AreEqual(2, dataSet._.Id[2]);
                var expectedLog =
@"DataRowRemoving: DataSet-0[1].
DataRowRemoving: DataSet-1[5].
DataRowRemoving: DataSet-2[17].
DataRowRemoved: DataSet-2[17].
DataRowRemoving: DataSet-2[16].
DataRowRemoved: DataSet-2[16].
DataRowRemoving: DataSet-2[15].
DataRowRemoved: DataSet-2[15].
DataRowRemoved: DataSet-1[5].
DataRowRemoving: DataSet-1[4].
DataRowRemoving: DataSet-2[14].
DataRowRemoved: DataSet-2[14].
DataRowRemoving: DataSet-2[13].
DataRowRemoved: DataSet-2[13].
DataRowRemoving: DataSet-2[12].
DataRowRemoved: DataSet-2[12].
DataRowRemoved: DataSet-1[4].
DataRowRemoving: DataSet-1[3].
DataRowRemoving: DataSet-2[11].
DataRowRemoved: DataSet-2[11].
DataRowRemoving: DataSet-2[10].
DataRowRemoved: DataSet-2[10].
DataRowRemoving: DataSet-2[9].
DataRowRemoved: DataSet-2[9].
DataRowRemoved: DataSet-1[3].
DataRowRemoved: DataSet-0[1].
DataRowInserting: DataSet-0[0].
DataRowInserting: DataSet-1[0].
DataRowInserting: DataSet-2[0].
BeforeDataRowInserted: DataSet-2[0].
AfterDataRowInserted: DataSet-2[0].
DataRowInserting: DataSet-2[1].
BeforeDataRowInserted: DataSet-2[1].
AfterDataRowInserted: DataSet-2[1].
DataRowInserting: DataSet-2[2].
BeforeDataRowInserted: DataSet-2[2].
AfterDataRowInserted: DataSet-2[2].
BeforeDataRowInserted: DataSet-1[0].
AfterDataRowInserted: DataSet-1[0].
DataRowInserting: DataSet-1[1].
DataRowInserting: DataSet-2[3].
BeforeDataRowInserted: DataSet-2[3].
AfterDataRowInserted: DataSet-2[3].
DataRowInserting: DataSet-2[4].
BeforeDataRowInserted: DataSet-2[4].
AfterDataRowInserted: DataSet-2[4].
DataRowInserting: DataSet-2[5].
BeforeDataRowInserted: DataSet-2[5].
AfterDataRowInserted: DataSet-2[5].
BeforeDataRowInserted: DataSet-1[1].
AfterDataRowInserted: DataSet-1[1].
DataRowInserting: DataSet-1[2].
DataRowInserting: DataSet-2[6].
BeforeDataRowInserted: DataSet-2[6].
AfterDataRowInserted: DataSet-2[6].
DataRowInserting: DataSet-2[7].
BeforeDataRowInserted: DataSet-2[7].
AfterDataRowInserted: DataSet-2[7].
DataRowInserting: DataSet-2[8].
BeforeDataRowInserted: DataSet-2[8].
AfterDataRowInserted: DataSet-2[8].
BeforeDataRowInserted: DataSet-1[2].
AfterDataRowInserted: DataSet-1[2].
BeforeDataRowInserted: DataSet-0[0].
AfterDataRowInserted: DataSet-0[0].
";
                Assert.AreEqual(expectedLog, log.ToString());
            }

            {
                // Move forward without children
                var dataSet = GetDataSet(3, false);
                var log = dataSet._.StartLog(1);
                dataSet[0].Move(2);
                Assert.AreEqual(1, dataSet._.Id[0]);
                Assert.AreEqual(2, dataSet._.Id[1]);
                Assert.AreEqual(0, dataSet._.Id[2]);
                var expectedLog =
@"DataRowRemoving: DataSet-0[0].
DataRowRemoved: DataSet-0[0].
DataRowInserting: DataSet-0[2].
BeforeDataRowInserted: DataSet-0[2].
AfterDataRowInserted: DataSet-0[2].
";
                Assert.AreEqual(expectedLog, log.ToString());
            }

            {
                // Move backward without children
                var dataSet = GetDataSet(3, false);
                var log = dataSet._.StartLog(1);
                dataSet[1].Move(-1);
                Assert.AreEqual(1, dataSet._.Id[0]);
                Assert.AreEqual(0, dataSet._.Id[1]);
                Assert.AreEqual(2, dataSet._.Id[2]);
                var expectedLog =
@"DataRowRemoving: DataSet-0[1].
DataRowRemoved: DataSet-0[1].
DataRowInserting: DataSet-0[0].
BeforeDataRowInserted: DataSet-0[0].
AfterDataRowInserted: DataSet-0[0].
";
                Assert.AreEqual(expectedLog, log.ToString());
            }
        }
    }
}

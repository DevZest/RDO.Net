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
@"DataSet-2[8] removed.
DataSet-2[7] removed.
DataSet-2[6] removed.
DataSet-1[2] removed.
DataSet-2[5] removed.
DataSet-2[4] removed.
DataSet-2[3] removed.
DataSet-1[1] removed.
DataSet-2[2] removed.
DataSet-2[1] removed.
DataSet-2[0] removed.
DataSet-1[0] removed.
DataSet-0[0] removed.
DataSet-0[2] adding.
DataSet-1[6] adding.
DataSet-2[18] adding.
DataSet-2[18] added.
DataSet-2[19] adding.
DataSet-2[19] added.
DataSet-2[20] adding.
DataSet-2[20] added.
DataSet-1[6] added.
DataSet-1[7] adding.
DataSet-2[21] adding.
DataSet-2[21] added.
DataSet-2[22] adding.
DataSet-2[22] added.
DataSet-2[23] adding.
DataSet-2[23] added.
DataSet-1[7] added.
DataSet-1[8] adding.
DataSet-2[24] adding.
DataSet-2[24] added.
DataSet-2[25] adding.
DataSet-2[25] added.
DataSet-2[26] adding.
DataSet-2[26] added.
DataSet-1[8] added.
DataSet-0[2] added.
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
@"DataSet-2[17] removed.
DataSet-2[16] removed.
DataSet-2[15] removed.
DataSet-1[5] removed.
DataSet-2[14] removed.
DataSet-2[13] removed.
DataSet-2[12] removed.
DataSet-1[4] removed.
DataSet-2[11] removed.
DataSet-2[10] removed.
DataSet-2[9] removed.
DataSet-1[3] removed.
DataSet-0[1] removed.
DataSet-0[0] adding.
DataSet-1[0] adding.
DataSet-2[0] adding.
DataSet-2[0] added.
DataSet-2[1] adding.
DataSet-2[1] added.
DataSet-2[2] adding.
DataSet-2[2] added.
DataSet-1[0] added.
DataSet-1[1] adding.
DataSet-2[3] adding.
DataSet-2[3] added.
DataSet-2[4] adding.
DataSet-2[4] added.
DataSet-2[5] adding.
DataSet-2[5] added.
DataSet-1[1] added.
DataSet-1[2] adding.
DataSet-2[6] adding.
DataSet-2[6] added.
DataSet-2[7] adding.
DataSet-2[7] added.
DataSet-2[8] adding.
DataSet-2[8] added.
DataSet-1[2] added.
DataSet-0[0] added.
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
@"DataSet-0[0] removed.
DataSet-0[2] adding.
DataSet-0[2] added.
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
@"DataSet-0[1] removed.
DataSet-0[0] adding.
DataSet-0[0] added.
";
                Assert.AreEqual(expectedLog, log.ToString());
            }
        }
    }
}

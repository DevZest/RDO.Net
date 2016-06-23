using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                dataSet[0].Move(2);
                Assert.AreEqual(1, dataSet._.Id[0]);
                Assert.AreEqual(2, dataSet._.Id[1]);
                Assert.AreEqual(0, dataSet._.Id[2]);
                Assert.AreEqual(3, dataSet[2].Children(dataSet._.Child).Count);
            }

            {
                // Move backward with children
                var dataSet = GetDataSet(3);
                dataSet[1].Move(-1);
                Assert.AreEqual(1, dataSet._.Id[0]);
                Assert.AreEqual(3, dataSet[0].Children(dataSet._.Child).Count);
                Assert.AreEqual(0, dataSet._.Id[1]);
                Assert.AreEqual(2, dataSet._.Id[2]);
            }

            {
                // Move forward without children
                var dataSet = GetDataSet(3, false);
                dataSet[0].Move(2);
                Assert.AreEqual(1, dataSet._.Id[0]);
                Assert.AreEqual(2, dataSet._.Id[1]);
                Assert.AreEqual(0, dataSet._.Id[2]);
            }

            {
                // Move backward without children
                var dataSet = GetDataSet(3, false);
                dataSet[1].Move(-1);
                Assert.AreEqual(1, dataSet._.Id[0]);
                Assert.AreEqual(0, dataSet._.Id[1]);
                Assert.AreEqual(2, dataSet._.Id[2]);
            }
        }
    }
}

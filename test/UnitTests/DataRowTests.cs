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
    }
}

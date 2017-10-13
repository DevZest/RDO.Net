using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace DevZest.Data
{
    [TestClass]
    public class DataSetContainerTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public void DataSetContainer_column_computation()
        {
            var dataSet = GetSalesOrder(71774);
            var _ = dataSet._;

            var subTotal = _.SubTotal[0].Value;
            dataSet[0].BeginEdit();
            var editingRow = dataSet.EditingRow;
            Assert.AreEqual(subTotal, _.SubTotal[editingRow].Value);
            dataSet[0].EndEdit();
        }
    }
}

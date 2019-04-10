using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class DataSetContainerTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public async Task DataSetContainer_column_computation()
        {
            var dataSet = await GetSalesOrderInfoAsync(71774);
            var _ = dataSet._;

            Assert.AreNotEqual(0, _.TaxAmt[0]);

            var subTotal = _.SubTotal[0].Value;
            var totalDue = _.TotalDue[0].Value;
            dataSet[0].BeginEdit();
            Assert.AreEqual(subTotal, _.SubTotal[0].Value);
            _.TaxAmt[0] = 0;
            Assert.AreEqual(_.SubTotal[0] + _.Freight[0] + _.TaxAmt[0], _.TotalDue[0].Value);
            Assert.AreNotEqual(totalDue, _.TotalDue[0]);
            dataSet[0].EndEdit();
        }
    }
}

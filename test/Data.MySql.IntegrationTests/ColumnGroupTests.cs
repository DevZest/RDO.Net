using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class ColumnGroupTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public async Task Model_ColumnGroup_sales_order_with_details()
        {
            var json = (await GetSalesOrderInfoAsync(71774)).ToJsonString(true);
            var expectedJson = Strings.ExpectedJSON_SalesOrderInfo_71774;
            Assert.AreEqual(expectedJson, json);

            var dataSet = DataSet<SalesOrderInfo>.ParseJson(json);
            Assert.AreEqual(expectedJson, dataSet.ToJsonString(true));
        }
    }
}

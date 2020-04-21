using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class ProjectionTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public async Task Model_Projections_sales_order_with_details()
        {
            var expectedJson = Strings.ExpectedJSON_SalesOrderInfo_71774;
            string json;

            {
                var dataSet = await GetSalesOrderInfoAsync(71774);
                json = dataSet.ToJsonString(true);
                Assert.AreEqual(expectedJson, json);
            }

            {
                var dataSet = DataSet<SalesOrderInfo>.ParseJson(json);
                Assert.AreEqual(expectedJson, dataSet.ToJsonString(true));
            }
        }
    }
}

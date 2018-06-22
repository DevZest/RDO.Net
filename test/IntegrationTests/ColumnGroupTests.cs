using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class ColumnGroupTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public void Model_ColumnGroup_sales_order_with_details()
        {
            var json = GetSalesOrderInfo(71774).ToJsonString(true);
            var expectedJson = Strings.ExpectedJSON_SalesOrderInfo_71774;
            Assert.AreEqual(expectedJson, json);

            var dataSet = DataSet<SalesOrderInfo>.ParseJson(json);
            Assert.AreEqual(expectedJson, dataSet.ToJsonString(true));
        }
    }
}

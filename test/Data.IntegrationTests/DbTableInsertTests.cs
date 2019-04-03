using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class DbTableInsertTests : AdventureWorksTestsBase
    {
        private static DataSet<SalesOrder> NewSalesOrdersTestData(int count = 2)
        {
            var result = DataSet<SalesOrder>.Create();
            result.AddTestDataRows(count);
            return result;
        }

        [TestMethod]
        public async Task DbTable_InsertScalarAsync_update_identity()
        {
            var salesOrder = NewSalesOrdersTestData(1);
            var log = new StringBuilder();
            using (var db = await new EmptySalesOrderMockDb().InitializeAsync(await OpenDbAsync(log)))
            {
                await db.SalesOrderHeader.InsertAsync(salesOrder, true);
            }
            Assert.AreEqual(1, salesOrder._.SalesOrderID[0]);
        }

        [TestMethod]
        public async Task DbTable_InsertAsync_DataSet_update_identity()
        {
            var salesOrders = NewSalesOrdersTestData();
            var log = new StringBuilder();
            using (var db = await new EmptySalesOrderMockDb().InitializeAsync(await OpenDbAsync(log, LogCategory.All)))
            {
                var result = await db.SalesOrderHeader.InsertAsync(salesOrders, true);
                Assert.AreEqual(2, result);
            }
            Assert.AreEqual(1, salesOrders._.SalesOrderID[0]);
            Assert.AreEqual(2, salesOrders._.SalesOrderID[1]);
        }
    }
}
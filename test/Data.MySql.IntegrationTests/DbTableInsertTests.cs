using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data.MySql
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
            using (var db = await MockEmptySalesOrder.CreateAsync(CreateDb(log)))
            {
                await db.SalesOrderHeader.InsertAsync(salesOrder, updateIdentity: true);
            }
            Assert.AreEqual(1, salesOrder._.SalesOrderID[0]);
        }

        [TestMethod]
        public async Task DbTable_InsertAsync_DataSet_update_identity()
        {
            var salesOrders = NewSalesOrdersTestData();
            var log = new StringBuilder();
            using (var db = await MockEmptySalesOrder.CreateAsync(CreateDb(log, LogCategory.All)))
            {
                var result = await db.SalesOrderHeader.InsertAsync(salesOrders, updateIdentity: true);
                Assert.AreEqual(2, result);
            }
            Assert.AreEqual(1, salesOrders._.SalesOrderID[0]);
            Assert.AreEqual(2, salesOrders._.SalesOrderID[1]);
        }
    }
}
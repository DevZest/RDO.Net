using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class DbTableInsertTests : AdventureWorksTestsBase
    {
        private static DataSet<SalesOrder> NewSalesOrdersTestData(int count = 2)
        {
            var result = DataSet<SalesOrder>.New();
            result.AddTestDataRows(count);
            return result;
        }

        [TestMethod]
        public void DbTable_InsertScalar_update_identity()
        {
            var salesOrder = NewSalesOrdersTestData(1);
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(OpenDb(log)))
            {
                db.SalesOrders.Insert(salesOrder, updateIdentity: true).Execute();
            }
            Assert.AreEqual(1, salesOrder._.SalesOrderID[0]);
        }

        [TestMethod]
        public async Task DbTable_InsertScalarAsync_update_identity()
        {
            var salesOrder = NewSalesOrdersTestData(1);
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(await OpenDbAsync(log)))
            {
                await db.SalesOrders.Insert(salesOrder, updateIdentity: true).ExecuteAsync();
            }
            Assert.AreEqual(1, salesOrder._.SalesOrderID[0]);
        }

        [TestMethod]
        public void DbTable_Insert_DataSet_update_identity()
        {
            var salesOrders = NewSalesOrdersTestData();
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(OpenDb(log, LogCategory.All)))
            {
                var result = db.SalesOrders.Insert(salesOrders, updateIdentity: true).Execute();
                Assert.AreEqual(2, result);
            }
            Assert.AreEqual(1, salesOrders._.SalesOrderID[0]);
            Assert.AreEqual(2, salesOrders._.SalesOrderID[1]);
        }

        [TestMethod]
        public async Task DbTable_InsertAsync_DataSet_update_identity()
        {
            var salesOrders = NewSalesOrdersTestData();
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(await OpenDbAsync(log, LogCategory.All)))
            {
                var result = await db.SalesOrders.Insert(salesOrders, updateIdentity: true).ExecuteAsync();
                Assert.AreEqual(2, result);
            }
            Assert.AreEqual(1, salesOrders._.SalesOrderID[0]);
            Assert.AreEqual(2, salesOrders._.SalesOrderID[1]);
        }

        [TestMethod]
        public void DbTable_Insert_temp_table_update_identity()
        {
            var salesOrders = NewSalesOrdersTestData();
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(OpenDb(log, LogCategory.All)))
            {
                var tempSalesOrders = db.CreateTempTable<SalesOrder>();
                tempSalesOrders.Insert(salesOrders).Execute();
                tempSalesOrders.GetSalesOrderIds().Verify(0, -1);

                var result = db.SalesOrders.Insert(tempSalesOrders, updateIdentity: true).Execute();
                Assert.AreEqual(2, result);
                db.SalesOrders.GetSalesOrderIds().Verify(1, 2);
                tempSalesOrders.GetSalesOrderIds().Verify(1, 2);
            }
        }

        [TestMethod]
        public async Task DbTable_InsertAsync_temp_table_update_identity()
        {
            var salesOrders = NewSalesOrdersTestData();
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(OpenDb(log, LogCategory.All)))
            {
                var tempSalesOrders = await db.CreateTempTableAsync<SalesOrder>();
                await tempSalesOrders.Insert(salesOrders).ExecuteAsync();
                tempSalesOrders.GetSalesOrderIds().Verify(0, -1);

                var result = await db.SalesOrders.Insert(tempSalesOrders, updateIdentity: true).ExecuteAsync();
                Assert.AreEqual(2, result);
                db.SalesOrders.GetSalesOrderIds().Verify(1, 2);
                tempSalesOrders.GetSalesOrderIds().Verify(1, 2);
            }
        }
    }
}
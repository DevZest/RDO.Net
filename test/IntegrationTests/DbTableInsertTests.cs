using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class DbTableInsertTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public void DbTable_InsertScalar_update_identity()
        {
            var salesOrder = GetSalesOrder_Insert_Scalar_update_identity();
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(OpenDb(log)))
            {
                db.SalesOrders.Insert(salesOrder, updateIdentity: true);
            }
            Assert.AreEqual(1, salesOrder._.SalesOrderID[0]);
        }

        private static DataSet<SalesOrder> GetSalesOrder_Insert_Scalar_update_identity()
        {
            var salesOrder = DataSet<SalesOrder>.New();
            salesOrder.AddRow();
            salesOrder._.DueDate[0] = new DateTime(2015, 9, 30);
            salesOrder._.SalesOrderNumber[0] = "SalesOrderNumber";
            salesOrder._.CustomerID[0] = 1;
            salesOrder._.ShipMethod[0] = "TRUCK";
            salesOrder._.TotalDue[0] = 0;
            return salesOrder;
        }

        [TestMethod]
        public async Task DbTable_InsertScalarAsync_update_identity()
        {
            var salesOrder = GetSalesOrder_Insert_Scalar_update_identity();
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(await OpenDbAsync(log)))
            {
                await db.SalesOrders.InsertAsync(salesOrder, updateIdentity: true);
            }
            Assert.AreEqual(1, salesOrder._.SalesOrderID[0]);
        }

        private static DataSet<SalesOrder> GetSalesOrders_Insert_DataSet_update_identity()
        {
            var salesOrder = DataSet<SalesOrder>.New();
            salesOrder.AddRow();
            salesOrder.AddRow();

            for (int i = 0; i < salesOrder.Count; i++)
            {
                salesOrder._.DueDate[i] = new DateTime(2015, 9, 30);
                salesOrder._.SalesOrderNumber[i] = "SalesOrderNumber" + (i + 1).ToString();
                salesOrder._.CustomerID[i] = i + 1;
                salesOrder._.ShipMethod[i] = "TRUCK" + (i + 1).ToString();
                salesOrder._.TotalDue[0] = 0;
            }

            return salesOrder;
        }

        [TestMethod]
        public void DbTable_Insert_DataSet_update_identity()
        {
            var salesOrders = GetSalesOrders_Insert_DataSet_update_identity();
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(OpenDb(log, LogCategory.All)))
            {
                var result = db.SalesOrders.Insert(salesOrders, updateIdentity: true);
                Assert.AreEqual(2, result);
            }
            Assert.AreEqual(1, salesOrders._.SalesOrderID[0]);
            Assert.AreEqual(2, salesOrders._.SalesOrderID[1]);
        }

        [TestMethod]
        public async Task DbTable_InsertAsync_DataSet_update_identity()
        {
            var salesOrders = GetSalesOrders_Insert_DataSet_update_identity();
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(await OpenDbAsync(log, LogCategory.All)))
            {
                var result = await db.SalesOrders.InsertAsync(salesOrders, updateIdentity: true);
                Assert.AreEqual(2, result);
            }
            Assert.AreEqual(1, salesOrders._.SalesOrderID[0]);
            Assert.AreEqual(2, salesOrders._.SalesOrderID[1]);
        }

        [TestMethod]
        public void DbTable_Insert_temp_table_update_identity()
        {
            var salesOrders = GetSalesOrders_Insert_DataSet_update_identity();
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(OpenDb(log, LogCategory.All)))
            {
                var tempSalesOrders = salesOrders.ToDbSet(db).ToTempTable();
                tempSalesOrders.GetSalesOrderIds().Verify(0, -1);

                var result = db.SalesOrders.Insert(tempSalesOrders, updateIdentity: true);
                Assert.AreEqual(2, result);
                db.SalesOrders.GetSalesOrderIds().Verify(1, 2);
                tempSalesOrders.GetSalesOrderIds().Verify(1, 2);
            }
        }

        [TestMethod]
        public async Task DbTable_InsertAsync_temp_table_update_identity()
        {
            var salesOrders = GetSalesOrders_Insert_DataSet_update_identity();
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(OpenDb(log, LogCategory.All)))
            {
                var tempSalesOrders = salesOrders.ToDbSet(db).ToTempTable();
                tempSalesOrders.GetSalesOrderIds().Verify(0, -1);

                var result = await db.SalesOrders.InsertAsync(tempSalesOrders, updateIdentity: true);
                Assert.AreEqual(2, result);
                db.SalesOrders.GetSalesOrderIds().Verify(1, 2);
                tempSalesOrders.GetSalesOrderIds().Verify(1, 2);
            }
        }
    }
}
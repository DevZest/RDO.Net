using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class DbSessionTests : AdventureWorksTestsBase
    {
        private static DbQuery<SalesOrder> CreateSalesOrdersQuery(Db db)
        {
            return db.CreateQuery((DbQueryBuilder builder, SalesOrder model) =>
            {
                builder.From(db.SalesOrderHeader, out var h)
                    .AutoSelect()
                    .Where(h.SalesOrderID == _Int32.Const(71774) | h.SalesOrderID == _Int32.Const(71776))
                    .OrderBy(h.SalesOrderID);
            });
        }

        private static void GetSalesOrderDetails(Db db, DbQueryBuilder queryBuilder, SalesOrderDetail model)
        {
            SalesOrderDetail d;
            queryBuilder.From(db.SalesOrderDetail, out d)
                .AutoSelect()
                .OrderBy(d.SalesOrderDetailID);
        }

        [TestMethod]
        public void DbSession_CreateQuery()
        {
            var log = new StringBuilder();
            using (var db = CreateDb(log))
            {
                var salesOrders = CreateSalesOrdersQuery(db);
                var salesOrderDetails = salesOrders.CreateChildAsync(x => x.SalesOrderDetails, (DbQueryBuilder builder, SalesOrderDetail model) => GetSalesOrderDetails(db, builder, model)).Result;
            }
            var expectedSql =
@"SET @@sql_notes = 0;
DROP TEMPORARY TABLE IF EXISTS `#sys_sequential_SalesOrder`;
SET @@sql_notes = 1;

CREATE TEMPORARY TABLE `#sys_sequential_SalesOrder` (
    `SalesOrderID` INT NOT NULL COMMENT 'Primary key.',
    `sys_row_id` INT NOT NULL AUTO_INCREMENT,

    UNIQUE (`SalesOrderID`),
    PRIMARY KEY (`sys_row_id` ASC)
);

INSERT INTO `#sys_sequential_SalesOrder`
(`SalesOrderID`)
SELECT `SalesOrderHeader`.`SalesOrderID` AS `SalesOrderID`
FROM `SalesOrderHeader`
WHERE ((`SalesOrderHeader`.`SalesOrderID` = 71774) OR (`SalesOrderHeader`.`SalesOrderID` = 71776))
ORDER BY `SalesOrderHeader`.`SalesOrderID`;
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        [TestMethod]
        public async Task DbSession_CreateQuery_async_child()
        {
            var log = new StringBuilder();
            using (var db = CreateDb(log))
            {
                var salesOrders = CreateSalesOrdersQuery(db);
                var salesOrderDetails = await salesOrders.CreateChildAsync(x => x.SalesOrderDetails,
                    (DbQueryBuilder builder, SalesOrderDetail model) => GetSalesOrderDetails(db, builder, model));
            }
            var expectedSql =
@"SET @@sql_notes = 0;
DROP TEMPORARY TABLE IF EXISTS `#sys_sequential_SalesOrder`;
SET @@sql_notes = 1;

CREATE TEMPORARY TABLE `#sys_sequential_SalesOrder` (
    `SalesOrderID` INT NOT NULL COMMENT 'Primary key.',
    `sys_row_id` INT NOT NULL AUTO_INCREMENT,

    UNIQUE (`SalesOrderID`),
    PRIMARY KEY (`sys_row_id` ASC)
);

INSERT INTO `#sys_sequential_SalesOrder`
(`SalesOrderID`)
SELECT `SalesOrderHeader`.`SalesOrderID` AS `SalesOrderID`
FROM `SalesOrderHeader`
WHERE ((`SalesOrderHeader`.`SalesOrderID` = 71774) OR (`SalesOrderHeader`.`SalesOrderID` = 71776))
ORDER BY `SalesOrderHeader`.`SalesOrderID`;
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        private static void GetDistinctSalesOrderDetails(Db db, DbAggregateQueryBuilder queryBuilder, SalesOrderDetail model)
        {
            SalesOrderDetail d;
            queryBuilder.From(db.SalesOrderDetail, out d)
                .AutoSelect()
                .OrderBy(d.SalesOrderDetailID);
        }

        [TestMethod]
        public async Task DbSession_ExecuteReaderAsync()
        {
            var log = new StringBuilder();
            using (var db = CreateDb(log))
            {
                var customers = db.Customer.OrderBy(x => x.CustomerID);
                var c = customers._;
                using (var reader = await db.ExecuteReaderAsync(customers))
                {
                    await reader.ReadAsync();
                    var id = c.CustomerID[reader];
                    Assert.AreEqual(1, id);
                }
            }
            var expectedSql =
@"SELECT
    `Customer`.`CustomerID` AS `CustomerID`,
    `Customer`.`NameStyle` AS `NameStyle`,
    `Customer`.`Title` AS `Title`,
    `Customer`.`FirstName` AS `FirstName`,
    `Customer`.`MiddleName` AS `MiddleName`,
    `Customer`.`LastName` AS `LastName`,
    `Customer`.`Suffix` AS `Suffix`,
    `Customer`.`CompanyName` AS `CompanyName`,
    `Customer`.`SalesPerson` AS `SalesPerson`,
    `Customer`.`EmailAddress` AS `EmailAddress`,
    `Customer`.`Phone` AS `Phone`,
    `Customer`.`PasswordHash` AS `PasswordHash`,
    `Customer`.`PasswordSalt` AS `PasswordSalt`,
    `Customer`.`RowGuid` AS `RowGuid`,
    `Customer`.`ModifiedDate` AS `ModifiedDate`
FROM `Customer`
ORDER BY `Customer`.`CustomerID`;
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        [TestMethod]
        public async Task DbSession_ExecuteTransactionAsync_update_sales_order()
        {
            var log = new StringBuilder();
            using (var db = await MockSalesOrder.CreateAsync(CreateDb(log)))
            {
                var salesOrder = await db.SalesOrderHeader.ToDbQuery<SalesOrder>().Where(_ => _.SalesOrderID == 1).ToDataSetAsync(CancellationToken.None);
                await salesOrder.FillAsync(0, _ => _.SalesOrderDetails, db.SalesOrderDetail);
                await db.UpdateAsync(salesOrder, CancellationToken.None);

                var dataSet = await db.SalesOrderDetail.Where(_ => _.SalesOrderID == 1).ToDataSetAsync(CancellationToken.None);
                Assert.IsTrue(dataSet._.ModifiedDate[0].Value.AddSeconds(1) > DateTime.Now);
            }
        }

        [TestMethod]
        public async Task DbSession_ExecuteTransactionAsync_insert_sales_order()
        {
            var log = new StringBuilder();
            using (var db = await MockSalesOrder.CreateAsync(CreateDb(log)))
            {
                var salesOrder = DataSet<SalesOrder>.ParseJson(Strings.ExpectedJSON_SalesOrder_71774);
                await db.InsertAsync(salesOrder, CancellationToken.None);

                int salesOrderID = salesOrder._.SalesOrderID[0].Value;
                Assert.AreEqual(5, salesOrderID);
                var dataSet = await db.SalesOrderDetail.Where(_ => _.SalesOrderID == salesOrderID).ToDataSetAsync(CancellationToken.None);
                Assert.IsTrue(dataSet._.ModifiedDate[0].Value.AddSeconds(1) > DateTime.Now);
            }
        }

        [TestMethod]
        public async Task DbSession_LookupAsync()
        {
            var productIds = DataSet<Product.Ref>.ParseJson(Strings.JSON_ProductIds);
            var log = new StringBuilder();
            using (var db = CreateDb(log))
            {
                var lookup = await db.LookupAsync(productIds, CancellationToken.None);
                Assert.AreEqual(Strings.ExpectedJSON_Product_Lookup, lookup.ToJsonString(true));
            }
        }
    }
}

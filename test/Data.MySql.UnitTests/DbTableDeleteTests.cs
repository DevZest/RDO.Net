using DevZest.Data.MySql.Helpers;
using DevZest.Data.MySql.Resources;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class DbTableDeleteTests
    {
        [TestMethod]
        public void DbTable_Delete_without_from()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var command = db.ProductCategory.MockDelete(0, x => x.ModifiedDate.IsNull());
                var expectedSql =
@"DELETE `ProductCategory`
FROM `ProductCategory`
WHERE (`ProductCategory`.`ModifiedDate` IS NULL);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Delete_from_temp_table()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var tempTable = db.MockTempTable<ProductCategory>();
                var command = db.ProductCategory.MockDelete(0, tempTable, (s, _) => s.Match(_));
                var expectedSql =
@"DELETE `ProductCategory`
FROM
    (`#ProductCategory`
    INNER JOIN
    `ProductCategory`
    ON `#ProductCategory`.`ProductCategoryID` = `ProductCategory`.`ProductCategoryID`);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Delete_from_simple_query()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.MockTempTable<ProductCategory>().Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategory.MockDelete(0, query, (s, _) => s.Match(_));
                var expectedSql =
@"DELETE `ProductCategory`
FROM
    (`#ProductCategory`
    INNER JOIN
    `ProductCategory`
    ON `#ProductCategory`.`ProductCategoryID` = `ProductCategory`.`ProductCategoryID`)
WHERE (`#ProductCategory`.`ModifiedDate` IS NULL);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Delete_scalar()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var dataSet = DataSet<ProductCategory>.Create();
                var index = dataSet.AddRow().Ordinal;
                dataSet._.Name[index] = "Name";
                dataSet._.RowGuid[index] = new Guid("EC359D7D-AE3A-4A9D-BDCB-03F0A7799514");
                dataSet._.ModifiedDate[index] = new DateTime(2015, 9, 23);
                var command = db.ProductCategory.MockDelete(false, dataSet, 0, (s, _) => s.Match(_));
                var expectedSql =
@"SET @p1 = 0;

DELETE `ProductCategory`
FROM
    ((SELECT @p1 AS `ProductCategoryID`) `@ProductCategory`
    INNER JOIN
    `ProductCategory`
    ON `@ProductCategory`.`ProductCategoryID` = `ProductCategory`.`ProductCategoryID`);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Delete_from_DataSet()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var salesOrder = DataSet<SalesOrder>.ParseJson(Json.SalesOrder_71774);
                var salesOrderDetails = salesOrder.Children(x => x.SalesOrderDetails);
                var command = db.SalesOrderDetail.MockDelete(0, salesOrderDetails, (s, _) => s.Match(_));
                var expectedSql =
@"SET @p1 = '[{""SalesOrderID"":71774,""SalesOrderDetailID"":110562,""OrderQty"":1,""ProductID"":836,""UnitPrice"":356.8980,""UnitPriceDiscount"":0,""LineTotal"":356.8980,""RowGuid"":""e3a1994c-7a68-4ce8-96a3-77fdd3bbd730"",""ModifiedDate"":""2008-06-01T00:00:00""},{""SalesOrderID"":71774,""SalesOrderDetailID"":110563,""OrderQty"":1,""ProductID"":822,""UnitPrice"":356.8980,""UnitPriceDiscount"":0,""LineTotal"":356.8980,""RowGuid"":""5c77f557-fdb6-43ba-90b9-9a7aec55ca32"",""ModifiedDate"":""2008-06-01T00:00:00""}]';

DELETE `SalesOrderDetail`
FROM
    (JSON_TABLE(@p1, '$[*]' COLUMNS (
        `SalesOrderID` INT PATH '$.SalesOrderID',
        `SalesOrderDetailID` INT PATH '$.SalesOrderDetailID',
        `OrderQty` SMALLINT PATH '$.OrderQty',
        `ProductID` INT PATH '$.ProductID',
        `UnitPrice` DECIMAL(19, 4) PATH '$.UnitPrice',
        `UnitPriceDiscount` DECIMAL(19, 4) PATH '$.UnitPriceDiscount',
        `LineTotal` DECIMAL(19, 4) PATH '$.LineTotal',
        `RowGuid` CHAR(36) PATH '$.RowGuid',
        `ModifiedDate` TIMESTAMP PATH '$.ModifiedDate',
        `sys_dataset_ordinal` FOR ORDINALITY)) AS `@SalesOrderDetail`
    INNER JOIN
    `SalesOrderDetail`
    ON `@SalesOrderDetail`.`SalesOrderID` = `SalesOrderDetail`.`SalesOrderID` AND `@SalesOrderDetail`.`SalesOrderDetailID` = `SalesOrderDetail`.`SalesOrderDetailID`);
";
                command.Verify(expectedSql);
            }
        }
    }
}

using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.MySql.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DevZest.Data.MySql.Resources;

namespace DevZest.Data.MySql
{
    [TestClass]
    public partial class DbTableInsertTests
    {
        [TestMethod]
        public void DbTable_Insert_from_DbTable()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var table = db.MockTempTable<Product>();
                var commands = table.MockInsert(0, db.Product);
                var expectedSql =
@"INSERT INTO `#Product`
(`ProductID`, `Name`, `ProductNumber`, `Color`, `StandardCost`, `ListPrice`, `Size`, `Weight`, `ProductCategoryID`, `ProductModelID`, `SellStartDate`, `SellEndDate`, `DiscontinuedDate`, `ThumbNailPhoto`, `ThumbnailPhotoFileName`, `RowGuid`, `ModifiedDate`)
SELECT
    `Product`.`ProductID` AS `ProductID`,
    `Product`.`Name` AS `Name`,
    `Product`.`ProductNumber` AS `ProductNumber`,
    `Product`.`Color` AS `Color`,
    `Product`.`StandardCost` AS `StandardCost`,
    `Product`.`ListPrice` AS `ListPrice`,
    `Product`.`Size` AS `Size`,
    `Product`.`Weight` AS `Weight`,
    `Product`.`ProductCategoryID` AS `ProductCategoryID`,
    `Product`.`ProductModelID` AS `ProductModelID`,
    `Product`.`SellStartDate` AS `SellStartDate`,
    `Product`.`SellEndDate` AS `SellEndDate`,
    `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
    `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
    `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
    `Product`.`RowGuid` AS `RowGuid`,
    `Product`.`ModifiedDate` AS `ModifiedDate`
FROM `Product`;
";
                commands.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_from_DbQuery()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var table = db.MockTempTable<ProductCategory>();
                var command = table.MockInsert(0, db.ProductCategory.Where(x => x.ParentProductCategoryID.IsNull()));
                var expectedSql =
@"INSERT INTO `#ProductCategory`
(`ProductCategoryID`, `ParentProductCategoryID`, `Name`, `RowGuid`, `ModifiedDate`)
SELECT
    `ProductCategory`.`ProductCategoryID` AS `ProductCategoryID`,
    `ProductCategory`.`ParentProductCategoryID` AS `ParentProductCategoryID`,
    `ProductCategory`.`Name` AS `Name`,
    `ProductCategory`.`RowGuid` AS `RowGuid`,
    `ProductCategory`.`ModifiedDate` AS `ModifiedDate`
FROM `ProductCategory`
WHERE (`ProductCategory`.`ParentProductCategoryID` IS NULL);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_from_child_DbQuery()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var salesOrders = db.SalesOrderHeader.ToDbQuery<SalesOrder>().Where(x => x.SalesOrderID == 71774 | x.SalesOrderID == 71776).OrderBy(x => x.SalesOrderID);
                salesOrders.MockSequentialKeyTempTable();
                var childQuery = salesOrders.CreateChildAsync(x => x.SalesOrderDetails, db.SalesOrderDetail.OrderBy(x => x.SalesOrderDetailID)).Result;
                var tempTable = db.MockTempTable<SalesOrderDetail>();
                var command = tempTable.MockInsert(0, childQuery);

                var expectedSql =
@"INSERT INTO `#SalesOrderDetail`
(`SalesOrderID`, `SalesOrderDetailID`, `OrderQty`, `ProductID`, `UnitPrice`, `UnitPriceDiscount`, `LineTotal`, `RowGuid`, `ModifiedDate`)
SELECT
    `SalesOrderDetail`.`SalesOrderID` AS `SalesOrderID`,
    `SalesOrderDetail`.`SalesOrderDetailID` AS `SalesOrderDetailID`,
    `SalesOrderDetail`.`OrderQty` AS `OrderQty`,
    `SalesOrderDetail`.`ProductID` AS `ProductID`,
    `SalesOrderDetail`.`UnitPrice` AS `UnitPrice`,
    `SalesOrderDetail`.`UnitPriceDiscount` AS `UnitPriceDiscount`,
    `SalesOrderDetail`.`LineTotal` AS `LineTotal`,
    `SalesOrderDetail`.`RowGuid` AS `RowGuid`,
    `SalesOrderDetail`.`ModifiedDate` AS `ModifiedDate`
FROM
    (`SalesOrderDetail`
    INNER JOIN
    `#sys_sequential_SalesOrder`
    ON `SalesOrderDetail`.`SalesOrderID` = `#sys_sequential_SalesOrder`.`SalesOrderID`)
ORDER BY `#sys_sequential_SalesOrder`.`sys_row_id` ASC, `SalesOrderDetail`.`SalesOrderDetailID`;
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_Scalar()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var table = db.MockTempTable<ProductCategory>();
                var dataSet = DataSet<ProductCategory>.Create();
                var dataRow = dataSet.AddRow();
                dataSet._.Name[dataRow] = "Name";
                dataSet._.ParentProductCategoryID[dataRow] = null;
                dataSet._.RowGuid[dataRow] = new Guid("040D9B64-05FD-4464-B398-74679C427980");
                dataSet._.ModifiedDate[dataRow] = new DateTime(2015, 9, 8);
                var command = table.MockInsert(true, dataSet, 0);
                var expectedSql =
@"SET @p1 = 0;
SET @p2 = NULL;
SET @p3 = 'Name';
SET @p4 = '040d9b64-05fd-4464-b398-74679c427980';
SET @p5 = (TIMESTAMP '2015-09-08 00:00:00');

INSERT INTO `#ProductCategory`
(`ProductCategoryID`, `ParentProductCategoryID`, `Name`, `RowGuid`, `ModifiedDate`)
SELECT
    @p1 AS `ProductCategoryID`,
    @p2 AS `ParentProductCategoryID`,
    @p3 AS `Name`,
    @p4 AS `RowGuid`,
    @p5 AS `ModifiedDate`;
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_union_query()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var tempTable = db.MockTempTable<Product>();
                var unionQuery = db.Product.Where(x => x.ProductID < _Int32.Const(720)).UnionAll(db.Product.Where(x => x.ProductID > _Int32.Const(800)));
                var command = tempTable.MockInsert(0, unionQuery);
                var expectedSql =
@"INSERT INTO `#Product`
(`ProductID`, `Name`, `ProductNumber`, `Color`, `StandardCost`, `ListPrice`, `Size`, `Weight`, `ProductCategoryID`, `ProductModelID`, `SellStartDate`, `SellEndDate`, `DiscontinuedDate`, `ThumbNailPhoto`, `ThumbnailPhotoFileName`, `RowGuid`, `ModifiedDate`)
(SELECT
    `Product`.`ProductID` AS `ProductID`,
    `Product`.`Name` AS `Name`,
    `Product`.`ProductNumber` AS `ProductNumber`,
    `Product`.`Color` AS `Color`,
    `Product`.`StandardCost` AS `StandardCost`,
    `Product`.`ListPrice` AS `ListPrice`,
    `Product`.`Size` AS `Size`,
    `Product`.`Weight` AS `Weight`,
    `Product`.`ProductCategoryID` AS `ProductCategoryID`,
    `Product`.`ProductModelID` AS `ProductModelID`,
    `Product`.`SellStartDate` AS `SellStartDate`,
    `Product`.`SellEndDate` AS `SellEndDate`,
    `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
    `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
    `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
    `Product`.`RowGuid` AS `RowGuid`,
    `Product`.`ModifiedDate` AS `ModifiedDate`
FROM `Product`
WHERE (`Product`.`ProductID` < 720))
UNION ALL
(SELECT
    `Product`.`ProductID` AS `ProductID`,
    `Product`.`Name` AS `Name`,
    `Product`.`ProductNumber` AS `ProductNumber`,
    `Product`.`Color` AS `Color`,
    `Product`.`StandardCost` AS `StandardCost`,
    `Product`.`ListPrice` AS `ListPrice`,
    `Product`.`Size` AS `Size`,
    `Product`.`Weight` AS `Weight`,
    `Product`.`ProductCategoryID` AS `ProductCategoryID`,
    `Product`.`ProductModelID` AS `ProductModelID`,
    `Product`.`SellStartDate` AS `SellStartDate`,
    `Product`.`SellEndDate` AS `SellEndDate`,
    `Product`.`DiscontinuedDate` AS `DiscontinuedDate`,
    `Product`.`ThumbNailPhoto` AS `ThumbNailPhoto`,
    `Product`.`ThumbnailPhotoFileName` AS `ThumbnailPhotoFileName`,
    `Product`.`RowGuid` AS `RowGuid`,
    `Product`.`ModifiedDate` AS `ModifiedDate`
FROM `Product`
WHERE (`Product`.`ProductID` > 800));
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_from_DataSet()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategories);
                var tempTable = db.MockTempTable<ProductCategory>();
                var command = tempTable.MockInsert(4, dataSet);

                var expectedSql =
@"SET @p1 = '[{""ProductCategoryID"":1,""Name"":""Bikes"",""RowGuid"":""cfbda25c-df71-47a7-b81b-64ee161aa37c"",""ModifiedDate"":""2002-06-01T00:00:00""},{""ProductCategoryID"":2,""Name"":""Components"",""RowGuid"":""c657828d-d808-4aba-91a3-af2ce02300e9"",""ModifiedDate"":""2002-06-01T00:00:00""},{""ProductCategoryID"":3,""Name"":""Clothing"",""RowGuid"":""10a7c342-ca82-48d4-8a38-46a2eb089b74"",""ModifiedDate"":""2002-06-01T00:00:00""},{""ProductCategoryID"":4,""Name"":""Accessories"",""RowGuid"":""2be3be36-d9a2-4eee-b593-ed895d97c2a6"",""ModifiedDate"":""2002-06-01T00:00:00""}]';

INSERT INTO `#ProductCategory`
(`ProductCategoryID`, `ParentProductCategoryID`, `Name`, `RowGuid`, `ModifiedDate`)
SELECT
    `@ProductCategory`.`ProductCategoryID` AS `ProductCategoryID`,
    `@ProductCategory`.`ParentProductCategoryID` AS `ParentProductCategoryID`,
    `@ProductCategory`.`Name` AS `Name`,
    `@ProductCategory`.`RowGuid` AS `RowGuid`,
    `@ProductCategory`.`ModifiedDate` AS `ModifiedDate`
FROM JSON_TABLE(@p1, '$[*]' COLUMNS (
    `ProductCategoryID` INT PATH '$.ProductCategoryID',
    `ParentProductCategoryID` INT PATH '$.ParentProductCategoryID',
    `Name` VARCHAR(50) PATH '$.Name',
    `RowGuid` CHAR(36) PATH '$.RowGuid',
    `ModifiedDate` TIMESTAMP PATH '$.ModifiedDate',
    `sys_dataset_ordinal` FOR ORDINALITY)) AS `@ProductCategory`
ORDER BY `@ProductCategory`.`sys_dataset_ordinal` ASC;
";

                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_into_child_temp_table_optimized()
        {
            var salesOrders = DataSet<SalesOrder>.ParseJson(Json.SalesOrder_71774);
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var tempSalesOrders = db.MockTempTable<SalesOrder>();
                var tempSalesOrderDetails = tempSalesOrders.MockCreateChild(x => x.SalesOrderDetails);
                tempSalesOrders.MockInsert(true, salesOrders, 0);
                var salesOrderDetails = salesOrders.Children(x => x.SalesOrderDetails, 0);
                var command = tempSalesOrderDetails.MockInsert(salesOrderDetails.Count, salesOrderDetails);
                var expectedSql =
@"SET @p1 = '[{""SalesOrderID"":71774,""SalesOrderDetailID"":110562,""OrderQty"":1,""ProductID"":836,""UnitPrice"":356.8980,""UnitPriceDiscount"":0,""LineTotal"":356.8980,""RowGuid"":""e3a1994c-7a68-4ce8-96a3-77fdd3bbd730"",""ModifiedDate"":""2008-06-01T00:00:00""},{""SalesOrderID"":71774,""SalesOrderDetailID"":110563,""OrderQty"":1,""ProductID"":822,""UnitPrice"":356.8980,""UnitPriceDiscount"":0,""LineTotal"":356.8980,""RowGuid"":""5c77f557-fdb6-43ba-90b9-9a7aec55ca32"",""ModifiedDate"":""2008-06-01T00:00:00""}]';

INSERT INTO `#SalesOrderDetail`
(`SalesOrderID`, `SalesOrderDetailID`, `OrderQty`, `ProductID`, `UnitPrice`, `UnitPriceDiscount`, `LineTotal`, `RowGuid`, `ModifiedDate`)
SELECT
    `@SalesOrderDetail`.`SalesOrderID` AS `SalesOrderID`,
    `@SalesOrderDetail`.`SalesOrderDetailID` AS `SalesOrderDetailID`,
    `@SalesOrderDetail`.`OrderQty` AS `OrderQty`,
    `@SalesOrderDetail`.`ProductID` AS `ProductID`,
    `@SalesOrderDetail`.`UnitPrice` AS `UnitPrice`,
    `@SalesOrderDetail`.`UnitPriceDiscount` AS `UnitPriceDiscount`,
    `@SalesOrderDetail`.`LineTotal` AS `LineTotal`,
    `@SalesOrderDetail`.`RowGuid` AS `RowGuid`,
    `@SalesOrderDetail`.`ModifiedDate` AS `ModifiedDate`
FROM JSON_TABLE(@p1, '$[*]' COLUMNS (
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
ORDER BY `@SalesOrderDetail`.`sys_dataset_ordinal` ASC;
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_into_child_temp_table_do_not_optimize()
        {
            var salesOrders = DataSet<SalesOrder>.ParseJson(Json.SalesOrder_71774);
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var tempSalesOrders = db.MockTempTable<SalesOrder>();
                var tempSalesOrderDetails = tempSalesOrders.MockCreateChild(x => x.SalesOrderDetails);
                tempSalesOrders.MockInsert(true, salesOrders, 0);
                salesOrders._.RowGuid[0] = Guid.NewGuid();  // make some change on the dataset
                var salesOrderDetails = salesOrders.Children(x => x.SalesOrderDetails, 0);
                var command = tempSalesOrderDetails.MockInsert(salesOrderDetails.Count, salesOrderDetails);
                var expectedSql =
@"SET @p1 = '[{""SalesOrderID"":71774,""SalesOrderDetailID"":110562,""OrderQty"":1,""ProductID"":836,""UnitPrice"":356.8980,""UnitPriceDiscount"":0,""LineTotal"":356.8980,""RowGuid"":""e3a1994c-7a68-4ce8-96a3-77fdd3bbd730"",""ModifiedDate"":""2008-06-01T00:00:00""},{""SalesOrderID"":71774,""SalesOrderDetailID"":110563,""OrderQty"":1,""ProductID"":822,""UnitPrice"":356.8980,""UnitPriceDiscount"":0,""LineTotal"":356.8980,""RowGuid"":""5c77f557-fdb6-43ba-90b9-9a7aec55ca32"",""ModifiedDate"":""2008-06-01T00:00:00""}]';

INSERT INTO `#SalesOrderDetail`
(`SalesOrderID`, `SalesOrderDetailID`, `OrderQty`, `ProductID`, `UnitPrice`, `UnitPriceDiscount`, `LineTotal`, `RowGuid`, `ModifiedDate`)
SELECT
    `@SalesOrderDetail`.`SalesOrderID` AS `SalesOrderID`,
    `@SalesOrderDetail`.`SalesOrderDetailID` AS `SalesOrderDetailID`,
    `@SalesOrderDetail`.`OrderQty` AS `OrderQty`,
    `@SalesOrderDetail`.`ProductID` AS `ProductID`,
    `@SalesOrderDetail`.`UnitPrice` AS `UnitPrice`,
    `@SalesOrderDetail`.`UnitPriceDiscount` AS `UnitPriceDiscount`,
    `@SalesOrderDetail`.`LineTotal` AS `LineTotal`,
    `@SalesOrderDetail`.`RowGuid` AS `RowGuid`,
    `@SalesOrderDetail`.`ModifiedDate` AS `ModifiedDate`
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
    `#SalesOrder`
    ON `@SalesOrderDetail`.`SalesOrderID` = `#SalesOrder`.`SalesOrderID`)
ORDER BY `@SalesOrderDetail`.`sys_dataset_ordinal` ASC;
";
                command.Verify(expectedSql);
            }
        }
    }
}

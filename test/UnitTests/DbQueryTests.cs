using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Data.Helpers;
using System;
using DevZest.Data.Primitives;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class DbQueryTests
    {
        [TestMethod]
        public void DbQuery_auto_select_all()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.CreateQuery((DbQueryBuilder builder, ProductDescription model) =>
                {
                    ProductDescription d;
                    builder.From(db.ProductDescription, out d)
                    .AutoSelect()
                    .Where(d.ProductDescriptionID > 5)
                    .OrderBy(d.ProductDescriptionID.Desc());
                });
                var expectedSql =
@"SET @p1 = 5;

SELECT
    `ProductDescription`.`ProductDescriptionID` AS `ProductDescriptionID`,
    `ProductDescription`.`Description` AS `Description`,
    `ProductDescription`.`RowGuid` AS `RowGuid`,
    `ProductDescription`.`ModifiedDate` AS `ModifiedDate`
FROM `ProductDescription`
WHERE (`ProductDescription`.`ProductDescriptionID` > @p1)
ORDER BY `ProductDescription`.`ProductDescriptionID` DESC;
";
                query.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbQuery_select_single_column()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.CreateQuery((DbQueryBuilder builder, Adhoc adhoc) =>
                {
                    ProductDescription d;
                    builder.From(db.ProductDescription, out d)
                    .Select(d.ProductDescriptionID, adhoc, "Id");
                });
                var expectedSql =
@"SELECT `ProductDescription`.`ProductDescriptionID` AS `Id`
FROM `ProductDescription`;
";
                query.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbQuery_select_multi_column()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.CreateQuery((DbQueryBuilder builder, Adhoc adhoc) =>
                {
                    ProductDescription d;
                    builder.From(db.ProductDescription, out d)
                        .Select(d.ProductDescriptionID, adhoc, "Id")
                        .Select(d.Description, adhoc)
                        .OrderBy(d.ProductDescriptionID);
                });
                var expectedSql =
@"SELECT
    `ProductDescription`.`ProductDescriptionID` AS `Id`,
    `ProductDescription`.`Description` AS `Description`
FROM `ProductDescription`
ORDER BY `ProductDescription`.`ProductDescriptionID`;
";
                query.Verify(expectedSql);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DbQuery_select_aggregate_function_throws_exception()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.CreateQuery((DbQueryBuilder builder, Adhoc adhoc) =>
                {
                    ProductDescription d;
                    builder.From(db.ProductDescription, out d)
                        .Select(d.ProductDescriptionID.Count(), adhoc);
                });
            }
        }

        [TestMethod]
        public void DbQuery_auto_group_by()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.CreateAggregateQuery<Adhoc>((DbAggregateQueryBuilder builder, Adhoc adhoc) =>
                {
                    _Int32 count;
                    builder.From(db.SalesOrderHeader, out var h)
                        .InnerJoin(db.SalesOrderDetail, h.PrimaryKey, x => x.FK_SalesOrderHeader, out var d)
                        .Select(h.SalesOrderID, adhoc)
                        .Select(count = d.SalesOrderID.Count(), adhoc, "LineCount")
                        .Having(count > _Int32.Const(1))
                        .OrderBy(count.Desc(), h.SalesOrderID);
                });
                var expectedSql =
@"SELECT
    `SalesOrderHeader`.`SalesOrderID` AS `SalesOrderID`,
    COUNT(`SalesOrderDetail`.`SalesOrderID`) AS `LineCount`
FROM
    (`SalesOrderHeader`
    INNER JOIN
    `SalesOrderDetail`
    ON `SalesOrderHeader`.`SalesOrderID` = `SalesOrderDetail`.`SalesOrderID`)
GROUP BY `SalesOrderHeader`.`SalesOrderID`
HAVING (COUNT(`SalesOrderDetail`.`SalesOrderID`) > 1)
ORDER BY COUNT(`SalesOrderDetail`.`SalesOrderID`) DESC, `SalesOrderHeader`.`SalesOrderID`;
";
                query.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbQuery_inner_join()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.CreateQuery((DbQueryBuilder builder, Adhoc adhoc) =>
                {
                    builder.From(db.SalesOrderDetail, out var d)
                        .InnerJoin(db.SalesOrderHeader, d.FK_SalesOrderHeader, out var h)
                        .InnerJoin(db.Product, d.FK_Product, out var p)
                        .Select(d.SalesOrderID, adhoc)
                        .Select(d.SalesOrderDetailID, adhoc)
                        .Select(p.Name, adhoc)
                        .Select(h.OrderDate, adhoc)
                        .Select(h.Status, adhoc)
                        .Select(h.TotalDue, adhoc)
                        .OrderBy(d.SalesOrderID, d.SalesOrderDetailID);
                });
                var expectedSql =
@"SELECT
    `SalesOrderDetail`.`SalesOrderID` AS `SalesOrderID`,
    `SalesOrderDetail`.`SalesOrderDetailID` AS `SalesOrderDetailID`,
    `Product`.`Name` AS `Name`,
    `SalesOrderHeader`.`OrderDate` AS `OrderDate`,
    `SalesOrderHeader`.`Status` AS `Status`,
    `SalesOrderHeader`.`TotalDue` AS `TotalDue`
FROM
    ((`SalesOrderDetail`
    INNER JOIN
    `SalesOrderHeader`
    ON `SalesOrderDetail`.`SalesOrderID` = `SalesOrderHeader`.`SalesOrderID`)
    INNER JOIN
    `Product`
    ON `SalesOrderDetail`.`ProductID` = `Product`.`ProductID`)
ORDER BY `SalesOrderDetail`.`SalesOrderID`, `SalesOrderDetail`.`SalesOrderDetailID`;
";
                query.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbQuery_derived_query_simplified()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.Product.Where(x => x.ProductID > _Int32.Const(500));
                var query2 = db.CreateQuery((DbQueryBuilder builder, Product model) =>
                {
                    Product p;
                    builder.From(query, out p)
                        .Select(p.ProductID, model.ProductID);
                });
                var expectedSql =
@"SELECT
    `Product`.`ProductID` AS `ProductID`,
    NULL AS `Name`,
    NULL AS `ProductNumber`,
    NULL AS `Color`,
    NULL AS `StandardCost`,
    NULL AS `ListPrice`,
    NULL AS `Size`,
    NULL AS `Weight`,
    NULL AS `ProductCategoryID`,
    NULL AS `ProductModelID`,
    NULL AS `SellStartDate`,
    NULL AS `SellEndDate`,
    NULL AS `DiscontinuedDate`,
    NULL AS `ThumbNailPhoto`,
    NULL AS `ThumbnailPhotoFileName`,
    NULL AS `RowGuid`,
    NULL AS `ModifiedDate`
FROM `Product`
WHERE (`Product`.`ProductID` > 500);
";
                query2.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbQuery_CreateChild()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var salesOrders = db.SalesOrderHeader.ToDbQuery<SalesOrder>().Where(x => x.SalesOrderID == 71774 | x.SalesOrderID == 71776).OrderBy(x => x.SalesOrderID);
                salesOrders.MockSequentialKeyTempTable();
                var childQuery = salesOrders.CreateChildAsync(x => x.SalesOrderDetails, db.SalesOrderDetail.OrderBy(x => x.SalesOrderDetailID)).Result;
                var expectedSql =
@"SELECT
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
    `#sys_sequential_SalesOrder` `sys_sequential_SalesOrder`
    ON `SalesOrderDetail`.`SalesOrderID` = `sys_sequential_SalesOrder`.`SalesOrderID`)
ORDER BY `sys_sequential_SalesOrder`.`sys_row_id` ASC, `SalesOrderDetail`.`SalesOrderDetailID`;
";
                Assert.AreEqual(expectedSql, childQuery.ToString());
            }
        }

        [TestMethod]
        public void DbQuery_CreateChild_aggregate()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var salesOrders = db.SalesOrderHeader.ToDbQuery<SalesOrder>().Where(x => x.SalesOrderID == 71774 | x.SalesOrderID == 71776).OrderBy(x => x.SalesOrderID);
                salesOrders.MockSequentialKeyTempTable();
                var salesOrderDetails = salesOrders.CreateChildAsync(x => x.SalesOrderDetails, (DbAggregateQueryBuilder builder, SalesOrderDetail model) =>
                {
                    SalesOrderDetail d;
                    builder.From(db.SalesOrderDetail, out d)
                        .AutoSelect()
                        .OrderBy(d.SalesOrderDetailID);
                }).Result;
                var expectedSql =
@"SELECT
    `SalesOrderDetail`.`SalesOrderID` AS `SalesOrderID`,
    `SalesOrderDetail`.`SalesOrderDetailID` AS `SalesOrderDetailID`,
    `SalesOrderDetail`.`OrderQty` AS `OrderQty`,
    `SalesOrderDetail`.`ProductID` AS `ProductID`,
    `SalesOrderDetail`.`UnitPrice` AS `UnitPrice`,
    `SalesOrderDetail`.`UnitPriceDiscount` AS `UnitPriceDiscount`,
    `SalesOrderDetail`.`LineTotal` AS `LineTotal`,
    `SalesOrderDetail`.`RowGuid` AS `RowGuid`,
    `SalesOrderDetail`.`ModifiedDate` AS `ModifiedDate`,
    `sys_sequential_SalesOrder`.`sys_row_id` AS `sys_parent_row_id`
FROM
    (`SalesOrderDetail`
    INNER JOIN
    `#sys_sequential_SalesOrder` `sys_sequential_SalesOrder`
    ON `SalesOrderDetail`.`SalesOrderID` = `sys_sequential_SalesOrder`.`SalesOrderID`)
GROUP BY
    `SalesOrderDetail`.`SalesOrderID`,
    `SalesOrderDetail`.`SalesOrderDetailID`,
    `SalesOrderDetail`.`OrderQty`,
    `SalesOrderDetail`.`ProductID`,
    `SalesOrderDetail`.`UnitPrice`,
    `SalesOrderDetail`.`UnitPriceDiscount`,
    `SalesOrderDetail`.`LineTotal`,
    `SalesOrderDetail`.`RowGuid`,
    `SalesOrderDetail`.`ModifiedDate`,
    `sys_sequential_SalesOrder`.`sys_row_id`
ORDER BY `sys_sequential_SalesOrder`.`sys_row_id` ASC, `SalesOrderDetail`.`SalesOrderDetailID`;
";
                Assert.AreEqual(expectedSql, salesOrderDetails.ToString());
            }
        }

        [TestMethod]
        public void DbQuery_SequentialSelectStatement_union_query()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var unionQuery = db.Product.Where(x => x.ProductID < _Int32.Const(720)).UnionAll(db.Product.Where(x => x.ProductID > _Int32.Const(800)));
                unionQuery.MockSequentialKeyTempTable();
                var expectedSql =
@"SELECT
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
    `Product`.`ModifiedDate` AS `ModifiedDate`,
    `sys_sequential_Product`.`sys_row_id` AS `sys_row_id`
FROM
    (((SELECT
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
    WHERE (`Product`.`ProductID` > 800))) `Product`
    INNER JOIN
    `#sys_sequential_Product` `sys_sequential_Product`
    ON `Product`.`ProductID` = `sys_sequential_Product`.`ProductID`)
ORDER BY `sys_sequential_Product`.`sys_row_id` ASC;
";
                Assert.AreEqual(expectedSql, db.InternalGetSqlString(unionQuery.GetSequentialQueryStatement()));
            }
        }

        [TestMethod]
        public void DbQuery_SequentialSelectStatement()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var salesOrderHeaders = db.SalesOrderHeader.Where(x => x.SalesOrderID == _Int32.Const(71774) | x.SalesOrderID == _Int32.Const(71776)).OrderBy(x => x.SalesOrderID);
                salesOrderHeaders.MockSequentialKeyTempTable();
                var expectedSql =
@"SELECT
    `SalesOrderHeader`.`SalesOrderID` AS `SalesOrderID`,
    `SalesOrderHeader`.`RevisionNumber` AS `RevisionNumber`,
    `SalesOrderHeader`.`OrderDate` AS `OrderDate`,
    `SalesOrderHeader`.`DueDate` AS `DueDate`,
    `SalesOrderHeader`.`ShipDate` AS `ShipDate`,
    `SalesOrderHeader`.`Status` AS `Status`,
    `SalesOrderHeader`.`OnlineOrderFlag` AS `OnlineOrderFlag`,
    `SalesOrderHeader`.`SalesOrderNumber` AS `SalesOrderNumber`,
    `SalesOrderHeader`.`PurchaseOrderNumber` AS `PurchaseOrderNumber`,
    `SalesOrderHeader`.`AccountNumber` AS `AccountNumber`,
    `SalesOrderHeader`.`CustomerID` AS `CustomerID`,
    `SalesOrderHeader`.`ShipToAddressID` AS `ShipToAddressID`,
    `SalesOrderHeader`.`BillToAddressID` AS `BillToAddressID`,
    `SalesOrderHeader`.`ShipMethod` AS `ShipMethod`,
    `SalesOrderHeader`.`CreditCardApprovalCode` AS `CreditCardApprovalCode`,
    `SalesOrderHeader`.`SubTotal` AS `SubTotal`,
    `SalesOrderHeader`.`TaxAmt` AS `TaxAmt`,
    `SalesOrderHeader`.`Freight` AS `Freight`,
    `SalesOrderHeader`.`TotalDue` AS `TotalDue`,
    `SalesOrderHeader`.`Comment` AS `Comment`,
    `SalesOrderHeader`.`RowGuid` AS `RowGuid`,
    `SalesOrderHeader`.`ModifiedDate` AS `ModifiedDate`,
    `sys_sequential_SalesOrderHeader`.`sys_row_id` AS `sys_row_id`
FROM
    (`SalesOrderHeader`
    INNER JOIN
    `#sys_sequential_SalesOrderHeader` `sys_sequential_SalesOrderHeader`
    ON `SalesOrderHeader`.`SalesOrderID` = `sys_sequential_SalesOrderHeader`.`SalesOrderID`)
WHERE ((`SalesOrderHeader`.`SalesOrderID` = 71774) OR (`SalesOrderHeader`.`SalesOrderID` = 71776))
ORDER BY `sys_sequential_SalesOrderHeader`.`sys_row_id` ASC;
";
                Assert.AreEqual(expectedSql, db.InternalGetSqlString(salesOrderHeaders.GetSequentialQueryStatement()));
            }
        }

        //        [TestMethod]
        //        public void DbQuery_SequentialSelectStatement_child_model()
        //        {
        //            using (var db = new Db(SqlVersion.Sql11))
        //            {
        //                var salesOrders = db.SalesOrderHeader.ToDbQuery<SalesOrder>().Where(x => x.SalesOrderID == 71774 | x.SalesOrderID == 71776).OrderBy(x => x.SalesOrderID);
        //                salesOrders.MockSequentialKeyTempTable();
        //                var salesOrderDetails = salesOrders.CreateChildAsync(x => x.SalesOrderDetails, db.SalesOrderDetail.OrderBy(x => x.SalesOrderDetailID)).Result;
        //                salesOrderDetails.MockSequentialKeyTempTable();
        //                var expectedSql =
        //@"SELECT
        //    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
        //    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID],
        //    [SalesOrderDetail].[OrderQty] AS [OrderQty],
        //    [SalesOrderDetail].[ProductID] AS [ProductID],
        //    [SalesOrderDetail].[UnitPrice] AS [UnitPrice],
        //    [SalesOrderDetail].[UnitPriceDiscount] AS [UnitPriceDiscount],
        //    [SalesOrderDetail].[LineTotal] AS [LineTotal],
        //    [SalesOrderDetail].[RowGuid] AS [RowGuid],
        //    [SalesOrderDetail].[ModifiedDate] AS [ModifiedDate],
        //    [sys_sequential_SalesOrder].[sys_row_id] AS [sys_parent_row_id],
        //    [sys_sequential_SalesOrderDetail].[sys_row_id] AS [sys_row_id]
        //FROM
        //    (([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
        //    INNER JOIN
        //    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
        //    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
        //    INNER JOIN
        //    [#sys_sequential_SalesOrderDetail] [sys_sequential_SalesOrderDetail]
        //    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrderDetail].[SalesOrderID] AND [SalesOrderDetail].[SalesOrderDetailID] = [sys_sequential_SalesOrderDetail].[SalesOrderDetailID])
        //ORDER BY [sys_sequential_SalesOrderDetail].[sys_row_id] ASC;
        //";
        //                Assert.AreEqual(expectedSql, db.GetSqlString(salesOrderDetails.SequentialQueryStatement));
        //            }
        //        }

        //[TestMethod]
        //public void DbQuery_SequentialKeyTempTable()
        //{
        //    using (var db = new Db(SqlVersion.Sql11))
        //    {
        //        var salesOrders = db.SalesOrderHeader.Where(x => x.SalesOrderID == _Int32.Const(71774) | x.SalesOrderID == _Int32.Const(71776)).OrderBy(x => x.SalesOrderID);
        //        var commands = salesOrders.GetCreateSequentialKeyTempTableCommands();
        //        var expectedSql = new string[]
        //        {
        //@"CREATE TABLE [#sys_sequential_SalesOrderHeader] (
        //    [SalesOrderID] INT NOT NULL,
        //    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

        //    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
        //    UNIQUE CLUSTERED ([sys_row_id] ASC)
        //);",

        //@"INSERT INTO [#sys_sequential_SalesOrderHeader]
        //([SalesOrderID])
        //SELECT [SalesOrderHeader].[SalesOrderID] AS [SalesOrderID]
        //FROM [SalesLT].[SalesOrderHeader] [SalesOrderHeader]
        //WHERE (([SalesOrderHeader].[SalesOrderID] = 71774) OR ([SalesOrderHeader].[SalesOrderID] = 71776))
        //ORDER BY [SalesOrderHeader].[SalesOrderID];"
        //        };
        //        commands.Verify(expectedSql);
        //    }
        //}

        //        [TestMethod]
        //        public void DbQuery_SequentialKeyTempTable_aggregate_query()
        //        {
        //            using (var db = new Db(SqlVersion.Sql11))
        //            {
        //                var salesOrders = db.CreateAggregateQuery((DbAggregateQueryBuilder queryBuilder, SalesOrder model) =>
        //                {
        //                    queryBuilder.From(db.SalesOrderHeader, out var h)
        //                        .AutoSelect()
        //                        .Where(h.SalesOrderID == _Int32.Const(71774) | h.SalesOrderID == _Int32.Const(71776))
        //                        .OrderBy(h.SalesOrderNumber.Desc());
        //                });

        //                var commands = salesOrders.GetCreateSequentialKeyTempTableCommands();

        //                var expectedSql = new string[]
        //                {
        //@"CREATE TABLE [#sys_sequential_SalesOrder] (
        //    [SalesOrderID] INT NOT NULL,
        //    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

        //    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
        //    UNIQUE CLUSTERED ([sys_row_id] ASC)
        //);",

        //@"INSERT INTO [#sys_sequential_SalesOrder]
        //([SalesOrderID])
        //SELECT [SalesOrderHeader].[SalesOrderID] AS [SalesOrderID]
        //FROM [SalesLT].[SalesOrderHeader] [SalesOrderHeader]
        //WHERE (([SalesOrderHeader].[SalesOrderID] = 71774) OR ([SalesOrderHeader].[SalesOrderID] = 71776))
        //GROUP BY
        //    [SalesOrderHeader].[SalesOrderID],
        //    [SalesOrderHeader].[RevisionNumber],
        //    [SalesOrderHeader].[OrderDate],
        //    [SalesOrderHeader].[DueDate],
        //    [SalesOrderHeader].[ShipDate],
        //    [SalesOrderHeader].[Status],
        //    [SalesOrderHeader].[OnlineOrderFlag],
        //    [SalesOrderHeader].[SalesOrderNumber],
        //    [SalesOrderHeader].[PurchaseOrderNumber],
        //    [SalesOrderHeader].[AccountNumber],
        //    [SalesOrderHeader].[CustomerID],
        //    [SalesOrderHeader].[ShipToAddressID],
        //    [SalesOrderHeader].[BillToAddressID],
        //    [SalesOrderHeader].[ShipMethod],
        //    [SalesOrderHeader].[CreditCardApprovalCode],
        //    [SalesOrderHeader].[SubTotal],
        //    [SalesOrderHeader].[TaxAmt],
        //    [SalesOrderHeader].[Freight],
        //    [SalesOrderHeader].[TotalDue],
        //    [SalesOrderHeader].[Comment],
        //    [SalesOrderHeader].[RowGuid],
        //    [SalesOrderHeader].[ModifiedDate]
        //ORDER BY [SalesOrderHeader].[SalesOrderNumber] DESC;"
        //                };

        //                commands.Verify(expectedSql);
        //            }
        //        }

        //        [TestMethod]
        //        public void DbQuery_SequentialKeyTempTable_union_query()
        //        {
        //            using (var db = new Db(SqlVersion.Sql11))
        //            {
        //                var unionQuery = db.Product.Where(x => x.ProductID < _Int32.Const(720)).UnionAll(db.Product.Where(x => x.ProductID > _Int32.Const(800)));
        //                var commands = unionQuery.GetCreateSequentialKeyTempTableCommands();

        //                var expectedSql = new string[]
        //                {
        //@"CREATE TABLE [#sys_sequential_Product] (
        //    [ProductID] INT NOT NULL,
        //    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

        //    PRIMARY KEY NONCLUSTERED ([ProductID]),
        //    UNIQUE CLUSTERED ([sys_row_id] ASC)
        //);",

        //@"INSERT INTO [#sys_sequential_Product]
        //([ProductID])
        //SELECT [Product].[ProductID] AS [ProductID]
        //FROM
        //    ((SELECT
        //        [Product].[ProductID] AS [ProductID],
        //        [Product].[Name] AS [Name],
        //        [Product].[ProductNumber] AS [ProductNumber],
        //        [Product].[Color] AS [Color],
        //        [Product].[StandardCost] AS [StandardCost],
        //        [Product].[ListPrice] AS [ListPrice],
        //        [Product].[Size] AS [Size],
        //        [Product].[Weight] AS [Weight],
        //        [Product].[ProductCategoryID] AS [ProductCategoryID],
        //        [Product].[ProductModelID] AS [ProductModelID],
        //        [Product].[SellStartDate] AS [SellStartDate],
        //        [Product].[SellEndDate] AS [SellEndDate],
        //        [Product].[DiscontinuedDate] AS [DiscontinuedDate],
        //        [Product].[ThumbNailPhoto] AS [ThumbNailPhoto],
        //        [Product].[ThumbnailPhotoFileName] AS [ThumbnailPhotoFileName],
        //        [Product].[RowGuid] AS [RowGuid],
        //        [Product].[ModifiedDate] AS [ModifiedDate]
        //    FROM [SalesLT].[Product] [Product]
        //    WHERE ([Product].[ProductID] < 720))
        //    UNION ALL
        //    (SELECT
        //        [Product].[ProductID] AS [ProductID],
        //        [Product].[Name] AS [Name],
        //        [Product].[ProductNumber] AS [ProductNumber],
        //        [Product].[Color] AS [Color],
        //        [Product].[StandardCost] AS [StandardCost],
        //        [Product].[ListPrice] AS [ListPrice],
        //        [Product].[Size] AS [Size],
        //        [Product].[Weight] AS [Weight],
        //        [Product].[ProductCategoryID] AS [ProductCategoryID],
        //        [Product].[ProductModelID] AS [ProductModelID],
        //        [Product].[SellStartDate] AS [SellStartDate],
        //        [Product].[SellEndDate] AS [SellEndDate],
        //        [Product].[DiscontinuedDate] AS [DiscontinuedDate],
        //        [Product].[ThumbNailPhoto] AS [ThumbNailPhoto],
        //        [Product].[ThumbnailPhotoFileName] AS [ThumbnailPhotoFileName],
        //        [Product].[RowGuid] AS [RowGuid],
        //        [Product].[ModifiedDate] AS [ModifiedDate]
        //    FROM [SalesLT].[Product] [Product]
        //    WHERE ([Product].[ProductID] > 800))) [Product];"
        //                };

        //                commands.Verify(expectedSql);
        //            }
        //        }
    }
}

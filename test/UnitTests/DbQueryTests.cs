using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Data.Helpers;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class DbQueryTests
    {
        [TestMethod]
        public void DbQuery_auto_select_all()
        {
            using (var db = Db.Open(SqlVersion.Sql11))
            {
                var query = db.CreateQuery((DbQueryBuilder builder, ProductDescription model) =>
                {
                    ProductDescription d;
                    builder.From(db.ProductDescriptions, out d)
                    .AutoSelect()
                    .Where(d.ProductDescriptionID > 5)
                    .OrderBy(d.ProductDescriptionID.Desc());
                });
                var expectedSql =
@"DECLARE @p1 INT = 5;

SELECT
    [ProductDescription].[ProductDescriptionID] AS [ProductDescriptionID],
    [ProductDescription].[Description] AS [Description],
    [ProductDescription].[RowGuid] AS [RowGuid],
    [ProductDescription].[ModifiedDate] AS [ModifiedDate]
FROM [SalesLT].[ProductDescription] [ProductDescription]
WHERE ([ProductDescription].[ProductDescriptionID] > @p1)
ORDER BY [ProductDescription].[ProductDescriptionID] DESC;
";
                query.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbQuery_select_single_column()
        {
            using (var db = Db.Open(SqlVersion.Sql11))
            {
                var query = db.CreateQuery((DbQueryBuilder builder, Adhoc adhoc) =>
                {
                    ProductDescription d;
                    builder.From(db.ProductDescriptions, out d)
                    .Select(d.ProductDescriptionID, adhoc, "Id");
                });
                var expectedSql =
@"SELECT [ProductDescription].[ProductDescriptionID] AS [Id]
FROM [SalesLT].[ProductDescription] [ProductDescription];
";
                query.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbQuery_select_multi_column()
        {
            using (var db = Db.Open(SqlVersion.Sql11))
            {
                var query = db.CreateQuery((DbQueryBuilder builder, Adhoc adhoc) =>
                {
                    ProductDescription d;
                    builder.From(db.ProductDescriptions, out d)
                        .Select(d.ProductDescriptionID, adhoc, "Id")
                        .Select(d.Description, adhoc)
                        .OrderBy(d.ProductDescriptionID);
                });
                var expectedSql =
@"SELECT
    [ProductDescription].[ProductDescriptionID] AS [Id],
    [ProductDescription].[Description] AS [Description]
FROM [SalesLT].[ProductDescription] [ProductDescription]
ORDER BY [ProductDescription].[ProductDescriptionID];
";
                query.Verify(expectedSql);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DbQuery_select_aggregate_function_throws_exception()
        {
            using (var db = Db.Open(SqlVersion.Sql11))
            {
                var query = db.CreateQuery((DbQueryBuilder builder, Adhoc adhoc) =>
                {
                    ProductDescription d;
                    builder.From(db.ProductDescriptions, out d)
                        .Select(d.ProductDescriptionID.Count(), adhoc);
                });
            }
        }

        [TestMethod]
        public void DbQuery_auto_group_by()
        {
            using (var db = Db.Open(SqlVersion.Sql11))
            {
                var query = db.CreateQuery<Adhoc>((DbAggregateQueryBuilder builder, Adhoc adhoc) =>
                {
                    SalesOrder h;
                    SalesOrderDetail d;
                    _Int32 count;
                    builder.From(db.SalesOrders, out h)
                        .InnerJoin(db.SalesOrderDetails, h.PrimaryKey, x => x.SalesOrderKey, out d)
                        .Select(h.SalesOrderID, adhoc)
                        .Select(count = d.SalesOrderID.Count(), adhoc, "LineCount")
                        .Having(count > _Int32.Const(1))
                        .OrderBy(count.Desc(), h.SalesOrderID);
                });
                var expectedSql =
@"SELECT
    [SalesOrder].[SalesOrderID] AS [SalesOrderID],
    COUNT([SalesOrderDetail].[SalesOrderID]) AS [LineCount]
FROM
    ([SalesLT].[SalesOrderHeader] [SalesOrder]
    INNER JOIN
    [SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    ON [SalesOrder].[SalesOrderID] = [SalesOrderDetail].[SalesOrderID])
GROUP BY [SalesOrder].[SalesOrderID]
HAVING (COUNT([SalesOrderDetail].[SalesOrderID]) > 1)
ORDER BY COUNT([SalesOrderDetail].[SalesOrderID]) DESC, [SalesOrder].[SalesOrderID];
";
                query.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbQuery_inner_join()
        {
            using (var db = Db.Open(SqlVersion.Sql11))
            {
                var query = db.CreateQuery((DbQueryBuilder builder, Adhoc adhoc) =>
                {
                    SalesOrderDetail d;
                    SalesOrder h;
                    Product p;
                    builder.From(db.SalesOrderDetails, out d)
                        .InnerJoin(db.SalesOrders, d.SalesOrderKey, x => x.PrimaryKey, out h)
                        .InnerJoin(db.Products, d.ProductKey, x => x.PrimaryKey, out p)
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
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID],
    [Product].[Name] AS [Name],
    [SalesOrder].[OrderDate] AS [OrderDate],
    [SalesOrder].[Status] AS [Status],
    [SalesOrder].[TotalDue] AS [TotalDue]
FROM
    (([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    INNER JOIN
    [SalesLT].[SalesOrderHeader] [SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [SalesOrder].[SalesOrderID])
    INNER JOIN
    [SalesLT].[Product] [Product]
    ON [SalesOrderDetail].[ProductID] = [Product].[ProductID])
ORDER BY [SalesOrderDetail].[SalesOrderID], [SalesOrderDetail].[SalesOrderDetailID];
";
                query.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbQuery_derived_query_simplified()
        {
            using (var db = Db.Open(SqlVersion.Sql11))
            {
                var query = db.Products.Where(x => x.ProductID > _Int32.Const(500));
                var query2 = db.CreateQuery((DbQueryBuilder builder, Product model) =>
                {
                    Product p;
                    builder.From(query, out p)
                        .Select(p.ProductID, model.ProductID);
                });
                var expectedSql =
@"SELECT
    [Product].[ProductID] AS [ProductID],
    NULL AS [Name],
    NULL AS [ProductNumber],
    NULL AS [Color],
    NULL AS [StandardCost],
    NULL AS [ListPrice],
    NULL AS [Size],
    NULL AS [Weight],
    NULL AS [ProductCategoryID],
    NULL AS [ProductModelID],
    NULL AS [SellStartDate],
    NULL AS [SellEndDate],
    NULL AS [DiscontinuedDate],
    NULL AS [ThumbNailPhoto],
    NULL AS [ThumbnailPhotoFileName],
    NULL AS [RowGuid],
    NULL AS [ModifiedDate]
FROM [SalesLT].[Product] [Product]
WHERE ([Product].[ProductID] > 500);
";
                query2.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbQuery_CreateChild()
        {
            using (var db = Db.Open(SqlVersion.Sql11))
            {
                var salesOrders = db.SalesOrders.Where(x => x.SalesOrderID == 71774 | x.SalesOrderID == 71776).OrderBy(x => x.SalesOrderID);
                salesOrders.MockSequentialKeyTempTable();
                var childQuery = salesOrders.CreateChild(x => x.SalesOrderDetails, db.SalesOrderDetails.OrderBy(x => x.SalesOrderDetailID));
                var expectedSql =
@"SELECT
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID],
    [SalesOrderDetail].[OrderQty] AS [OrderQty],
    [SalesOrderDetail].[ProductID] AS [ProductID],
    [SalesOrderDetail].[UnitPrice] AS [UnitPrice],
    [SalesOrderDetail].[UnitPriceDiscount] AS [UnitPriceDiscount],
    [SalesOrderDetail].[LineTotal] AS [LineTotal],
    [SalesOrderDetail].[RowGuid] AS [RowGuid],
    [SalesOrderDetail].[ModifiedDate] AS [ModifiedDate]
FROM
    ([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    INNER JOIN
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
ORDER BY [sys_sequential_SalesOrder].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
";
                Assert.AreEqual(expectedSql, childQuery.ToString());
            }
        }

        [TestMethod]
        public void DbQuery_CreateChild_aggregate()
        {
            using (var db = Db.Open(SqlVersion.Sql11))
            {
                var salesOrders = db.SalesOrders.Where(x => x.SalesOrderID == 71774 | x.SalesOrderID == 71776).OrderBy(x => x.SalesOrderID);
                salesOrders.MockSequentialKeyTempTable();
                var salesOrderDetails = salesOrders.CreateChild(x => x.SalesOrderDetails, (DbAggregateQueryBuilder builder, SalesOrderDetail model) =>
                {
                    SalesOrderDetail d;
                    builder.From(db.SalesOrderDetails, out d)
                        .AutoSelect()
                        .OrderBy(d.SalesOrderDetailID);
                });
                var expectedSql =
@"SELECT
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID],
    [SalesOrderDetail].[OrderQty] AS [OrderQty],
    [SalesOrderDetail].[ProductID] AS [ProductID],
    [SalesOrderDetail].[UnitPrice] AS [UnitPrice],
    [SalesOrderDetail].[UnitPriceDiscount] AS [UnitPriceDiscount],
    [SalesOrderDetail].[LineTotal] AS [LineTotal],
    [SalesOrderDetail].[RowGuid] AS [RowGuid],
    [SalesOrderDetail].[ModifiedDate] AS [ModifiedDate],
    [sys_sequential_SalesOrder].[sys_row_id] AS [sys_parent_row_id]
FROM
    ([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    INNER JOIN
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
GROUP BY
    [SalesOrderDetail].[SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID],
    [SalesOrderDetail].[OrderQty],
    [SalesOrderDetail].[ProductID],
    [SalesOrderDetail].[UnitPrice],
    [SalesOrderDetail].[UnitPriceDiscount],
    [SalesOrderDetail].[LineTotal],
    [SalesOrderDetail].[RowGuid],
    [SalesOrderDetail].[ModifiedDate],
    [sys_sequential_SalesOrder].[sys_row_id]
ORDER BY [sys_sequential_SalesOrder].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
";
                Assert.AreEqual(expectedSql, salesOrderDetails.ToString());
            }
        }

        [TestMethod]
        public void DbQuery_SequentialSelectStatement_union_query()
        {
            using (var db = Db.Open(SqlVersion.Sql11))
            {
                var unionQuery = db.Products.Where(x => x.ProductID < _Int32.Const(720)).UnionAll(db.Products.Where(x => x.ProductID > _Int32.Const(800)));
                unionQuery.MockSequentialKeyTempTable();
                var expectedSql =
@"SELECT
    [Product].[ProductID] AS [ProductID],
    [Product].[Name] AS [Name],
    [Product].[ProductNumber] AS [ProductNumber],
    [Product].[Color] AS [Color],
    [Product].[StandardCost] AS [StandardCost],
    [Product].[ListPrice] AS [ListPrice],
    [Product].[Size] AS [Size],
    [Product].[Weight] AS [Weight],
    [Product].[ProductCategoryID] AS [ProductCategoryID],
    [Product].[ProductModelID] AS [ProductModelID],
    [Product].[SellStartDate] AS [SellStartDate],
    [Product].[SellEndDate] AS [SellEndDate],
    [Product].[DiscontinuedDate] AS [DiscontinuedDate],
    [Product].[ThumbNailPhoto] AS [ThumbNailPhoto],
    [Product].[ThumbnailPhotoFileName] AS [ThumbnailPhotoFileName],
    [Product].[RowGuid] AS [RowGuid],
    [Product].[ModifiedDate] AS [ModifiedDate],
    [sys_sequential_Product].[sys_row_id] AS [sys_row_id]
FROM
    (((SELECT
        [Product].[ProductID] AS [ProductID],
        [Product].[Name] AS [Name],
        [Product].[ProductNumber] AS [ProductNumber],
        [Product].[Color] AS [Color],
        [Product].[StandardCost] AS [StandardCost],
        [Product].[ListPrice] AS [ListPrice],
        [Product].[Size] AS [Size],
        [Product].[Weight] AS [Weight],
        [Product].[ProductCategoryID] AS [ProductCategoryID],
        [Product].[ProductModelID] AS [ProductModelID],
        [Product].[SellStartDate] AS [SellStartDate],
        [Product].[SellEndDate] AS [SellEndDate],
        [Product].[DiscontinuedDate] AS [DiscontinuedDate],
        [Product].[ThumbNailPhoto] AS [ThumbNailPhoto],
        [Product].[ThumbnailPhotoFileName] AS [ThumbnailPhotoFileName],
        [Product].[RowGuid] AS [RowGuid],
        [Product].[ModifiedDate] AS [ModifiedDate]
    FROM [SalesLT].[Product] [Product]
    WHERE ([Product].[ProductID] < 720))
    UNION ALL
    (SELECT
        [Product].[ProductID] AS [ProductID],
        [Product].[Name] AS [Name],
        [Product].[ProductNumber] AS [ProductNumber],
        [Product].[Color] AS [Color],
        [Product].[StandardCost] AS [StandardCost],
        [Product].[ListPrice] AS [ListPrice],
        [Product].[Size] AS [Size],
        [Product].[Weight] AS [Weight],
        [Product].[ProductCategoryID] AS [ProductCategoryID],
        [Product].[ProductModelID] AS [ProductModelID],
        [Product].[SellStartDate] AS [SellStartDate],
        [Product].[SellEndDate] AS [SellEndDate],
        [Product].[DiscontinuedDate] AS [DiscontinuedDate],
        [Product].[ThumbNailPhoto] AS [ThumbNailPhoto],
        [Product].[ThumbnailPhotoFileName] AS [ThumbnailPhotoFileName],
        [Product].[RowGuid] AS [RowGuid],
        [Product].[ModifiedDate] AS [ModifiedDate]
    FROM [SalesLT].[Product] [Product]
    WHERE ([Product].[ProductID] > 800))) [Product]
    INNER JOIN
    [#sys_sequential_Product] [sys_sequential_Product]
    ON [Product].[ProductID] = [sys_sequential_Product].[ProductID])
ORDER BY [sys_sequential_Product].[sys_row_id] ASC;
";
                Assert.AreEqual(expectedSql, db.GetSqlString(unionQuery.SequentialQueryStatement));
            }
        }

        [TestMethod]
        public void DbQuery_SequentialSelectStatement()
        {
            using (var db = Db.Open(SqlVersion.Sql11))
            {
                var salesOrders = db.SalesOrders.Where(x => x.SalesOrderID == _Int32.Const(71774) | x.SalesOrderID == _Int32.Const(71776)).OrderBy(x => x.SalesOrderID);
                salesOrders.MockSequentialKeyTempTable();
                var expectedSql =
@"SELECT
    [SalesOrder].[SalesOrderID] AS [SalesOrderID],
    [SalesOrder].[RevisionNumber] AS [RevisionNumber],
    [SalesOrder].[OrderDate] AS [OrderDate],
    [SalesOrder].[DueDate] AS [DueDate],
    [SalesOrder].[ShipDate] AS [ShipDate],
    [SalesOrder].[Status] AS [Status],
    [SalesOrder].[OnlineOrderFlag] AS [OnlineOrderFlag],
    [SalesOrder].[SalesOrderNumber] AS [SalesOrderNumber],
    [SalesOrder].[PurchaseOrderNumber] AS [PurchaseOrderNumber],
    [SalesOrder].[AccountNumber] AS [AccountNumber],
    [SalesOrder].[CustomerID] AS [CustomerID],
    [SalesOrder].[ShipToAddressID] AS [ShipToAddressID],
    [SalesOrder].[BillToAddressID] AS [BillToAddressID],
    [SalesOrder].[ShipMethod] AS [ShipMethod],
    [SalesOrder].[CreditCardApprovalCode] AS [CreditCardApprovalCode],
    [SalesOrder].[SubTotal] AS [SubTotal],
    [SalesOrder].[TaxAmt] AS [TaxAmt],
    [SalesOrder].[Freight] AS [Freight],
    [SalesOrder].[TotalDue] AS [TotalDue],
    [SalesOrder].[Comment] AS [Comment],
    [SalesOrder].[RowGuid] AS [RowGuid],
    [SalesOrder].[ModifiedDate] AS [ModifiedDate],
    [sys_sequential_SalesOrder].[sys_row_id] AS [sys_row_id]
FROM
    ([SalesLT].[SalesOrderHeader] [SalesOrder]
    INNER JOIN
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrder].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
WHERE (([SalesOrder].[SalesOrderID] = 71774) OR ([SalesOrder].[SalesOrderID] = 71776))
ORDER BY [sys_sequential_SalesOrder].[sys_row_id] ASC;
";
                Assert.AreEqual(expectedSql, db.GetSqlString(salesOrders.SequentialQueryStatement));
            }
        }

        [TestMethod]
        public void DbQuery_SequentialSelectStatement_child_model()
        {
            using (var db = Db.Open(SqlVersion.Sql11))
            {
                var salesOrders = db.SalesOrders.Where(x => x.SalesOrderID == 71774 | x.SalesOrderID == 71776).OrderBy(x => x.SalesOrderID);
                salesOrders.MockSequentialKeyTempTable();
                var salesOrderDetails = salesOrders.CreateChild(x => x.SalesOrderDetails, db.SalesOrderDetails.OrderBy(x => x.SalesOrderDetailID));
                salesOrderDetails.MockSequentialKeyTempTable();
                var expectedSql =
@"SELECT
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID],
    [SalesOrderDetail].[OrderQty] AS [OrderQty],
    [SalesOrderDetail].[ProductID] AS [ProductID],
    [SalesOrderDetail].[UnitPrice] AS [UnitPrice],
    [SalesOrderDetail].[UnitPriceDiscount] AS [UnitPriceDiscount],
    [SalesOrderDetail].[LineTotal] AS [LineTotal],
    [SalesOrderDetail].[RowGuid] AS [RowGuid],
    [SalesOrderDetail].[ModifiedDate] AS [ModifiedDate],
    [sys_sequential_SalesOrder].[sys_row_id] AS [sys_parent_row_id],
    [sys_sequential_SalesOrderDetail].[sys_row_id] AS [sys_row_id]
FROM
    (([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    INNER JOIN
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
    INNER JOIN
    [#sys_sequential_SalesOrderDetail] [sys_sequential_SalesOrderDetail]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrderDetail].[SalesOrderID] AND [SalesOrderDetail].[SalesOrderDetailID] = [sys_sequential_SalesOrderDetail].[SalesOrderDetailID])
ORDER BY [sys_sequential_SalesOrderDetail].[sys_row_id] ASC;
";
                Assert.AreEqual(expectedSql, db.GetSqlString(salesOrderDetails.SequentialQueryStatement));
            }
        }

        [TestMethod]
        public void DbQuery_SequentialKeyTempTable()
        {
            var sqlVersion = SqlVersion.Sql11;
            using (var db = Db.Open(sqlVersion))
            {
                var salesOrders = db.SalesOrders.Where(x => x.SalesOrderID == _Int32.Const(71774) | x.SalesOrderID == _Int32.Const(71776)).OrderBy(x => x.SalesOrderID);
                var commands = salesOrders.GetCreateSequentialKeyTempTableCommands();
                var expectedSql = new string[]
                {
@"CREATE TABLE [#sys_sequential_SalesOrder] (
    [SalesOrderID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);",

@"INSERT INTO [#sys_sequential_SalesOrder]
([SalesOrderID])
SELECT [SalesOrder].[SalesOrderID] AS [SalesOrderID]
FROM [SalesLT].[SalesOrderHeader] [SalesOrder]
WHERE (([SalesOrder].[SalesOrderID] = 71774) OR ([SalesOrder].[SalesOrderID] = 71776))
ORDER BY [SalesOrder].[SalesOrderID];"
                };
                commands.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbQuery_SequentialKeyTempTable_aggregate_query()
        {
            var sqlVersion = SqlVersion.Sql11;
            using (var db = Db.Open(sqlVersion))
            {
                var salesOrders = db.CreateQuery((DbAggregateQueryBuilder queryBuilder, SalesOrder model) =>
                {
                    SalesOrder h;
                    queryBuilder.From(db.SalesOrders, out h)
                        .AutoSelect()
                        .Where(h.SalesOrderID == _Int32.Const(71774) | h.SalesOrderID == _Int32.Const(71776))
                        .OrderBy(h.SalesOrderNumber.Desc());
                });

                var commands = salesOrders.GetCreateSequentialKeyTempTableCommands();

                var expectedSql = new string[]
                {
@"CREATE TABLE [#sys_sequential_SalesOrder] (
    [SalesOrderID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);",

@"INSERT INTO [#sys_sequential_SalesOrder]
([SalesOrderID])
SELECT [SalesOrder].[SalesOrderID] AS [SalesOrderID]
FROM [SalesLT].[SalesOrderHeader] [SalesOrder]
WHERE (([SalesOrder].[SalesOrderID] = 71774) OR ([SalesOrder].[SalesOrderID] = 71776))
GROUP BY
    [SalesOrder].[SalesOrderID],
    [SalesOrder].[RevisionNumber],
    [SalesOrder].[OrderDate],
    [SalesOrder].[DueDate],
    [SalesOrder].[ShipDate],
    [SalesOrder].[Status],
    [SalesOrder].[OnlineOrderFlag],
    [SalesOrder].[SalesOrderNumber],
    [SalesOrder].[PurchaseOrderNumber],
    [SalesOrder].[AccountNumber],
    [SalesOrder].[CustomerID],
    [SalesOrder].[ShipToAddressID],
    [SalesOrder].[BillToAddressID],
    [SalesOrder].[ShipMethod],
    [SalesOrder].[CreditCardApprovalCode],
    [SalesOrder].[SubTotal],
    [SalesOrder].[TaxAmt],
    [SalesOrder].[Freight],
    [SalesOrder].[TotalDue],
    [SalesOrder].[Comment],
    [SalesOrder].[RowGuid],
    [SalesOrder].[ModifiedDate]
ORDER BY [SalesOrder].[SalesOrderNumber] DESC;"
                };

                commands.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbQuery_SequentialKeyTempTable_union_query()
        {
            var sqlVersion = SqlVersion.Sql11;
            using (var db = Db.Open(sqlVersion))
            {
                var unionQuery = db.Products.Where(x => x.ProductID < _Int32.Const(720)).UnionAll(db.Products.Where(x => x.ProductID > _Int32.Const(800)));
                var commands = unionQuery.GetCreateSequentialKeyTempTableCommands();

                var expectedSql = new string[]
                {
@"CREATE TABLE [#sys_sequential_Product] (
    [ProductID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([ProductID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);",

@"INSERT INTO [#sys_sequential_Product]
([ProductID])
SELECT [Product].[ProductID] AS [ProductID]
FROM
    ((SELECT
        [Product].[ProductID] AS [ProductID],
        [Product].[Name] AS [Name],
        [Product].[ProductNumber] AS [ProductNumber],
        [Product].[Color] AS [Color],
        [Product].[StandardCost] AS [StandardCost],
        [Product].[ListPrice] AS [ListPrice],
        [Product].[Size] AS [Size],
        [Product].[Weight] AS [Weight],
        [Product].[ProductCategoryID] AS [ProductCategoryID],
        [Product].[ProductModelID] AS [ProductModelID],
        [Product].[SellStartDate] AS [SellStartDate],
        [Product].[SellEndDate] AS [SellEndDate],
        [Product].[DiscontinuedDate] AS [DiscontinuedDate],
        [Product].[ThumbNailPhoto] AS [ThumbNailPhoto],
        [Product].[ThumbnailPhotoFileName] AS [ThumbnailPhotoFileName],
        [Product].[RowGuid] AS [RowGuid],
        [Product].[ModifiedDate] AS [ModifiedDate]
    FROM [SalesLT].[Product] [Product]
    WHERE ([Product].[ProductID] < 720))
    UNION ALL
    (SELECT
        [Product].[ProductID] AS [ProductID],
        [Product].[Name] AS [Name],
        [Product].[ProductNumber] AS [ProductNumber],
        [Product].[Color] AS [Color],
        [Product].[StandardCost] AS [StandardCost],
        [Product].[ListPrice] AS [ListPrice],
        [Product].[Size] AS [Size],
        [Product].[Weight] AS [Weight],
        [Product].[ProductCategoryID] AS [ProductCategoryID],
        [Product].[ProductModelID] AS [ProductModelID],
        [Product].[SellStartDate] AS [SellStartDate],
        [Product].[SellEndDate] AS [SellEndDate],
        [Product].[DiscontinuedDate] AS [DiscontinuedDate],
        [Product].[ThumbNailPhoto] AS [ThumbNailPhoto],
        [Product].[ThumbnailPhotoFileName] AS [ThumbnailPhotoFileName],
        [Product].[RowGuid] AS [RowGuid],
        [Product].[ModifiedDate] AS [ModifiedDate]
    FROM [SalesLT].[Product] [Product]
    WHERE ([Product].[ProductID] > 800))) [Product];"
                };

                commands.Verify(expectedSql);
            }
        }
    }
}

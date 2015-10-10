using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Helpers;
using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class DbSetTests
    {
        [TestMethod]
        public void DbSet_where_order_by()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var query = db.SalesOrders.Where(x => x.SalesOrderID == _Int32.Const(71774) | x.SalesOrderID == _Int32.Const(71776)).OrderBy(x => x.SalesOrderID);
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
    [SalesOrder].[ModifiedDate] AS [ModifiedDate]
FROM [SalesLT].[SalesOrderHeader] [SalesOrder]
WHERE (([SalesOrder].[SalesOrderID] = 71774) OR ([SalesOrder].[SalesOrderID] = 71776))
ORDER BY [SalesOrder].[SalesOrderID];
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }

        [TestMethod]
        public void DbSet_Union()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var query = db.Products.Where(x => x.ProductID < _Int32.Const(720)).UnionAll(db.Products.Where(x => x.ProductID > _Int32.Const(800)));
                var expectedSql =
@"(SELECT
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
WHERE ([Product].[ProductID] > 800));
";
                Assert.AreEqual(expectedSql, query.ToString());

                var query2 = query.OrderBy(x => x.Name.Asc());
                expectedSql =
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
    [Product].[ModifiedDate] AS [ModifiedDate]
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
    WHERE ([Product].[ProductID] > 800))) [Product]
ORDER BY [Product].[Name] ASC;
";
                Assert.AreEqual(expectedSql, query2.ToString());
            }
        }

        [TestMethod]
        public void DbSet_offset_fetch()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var query = db.SalesOrderDetails.OrderBy(10, 20, x => x.SalesOrderDetailID);
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
FROM [SalesLT].[SalesOrderDetail] [SalesOrderDetail]
ORDER BY [SalesOrderDetail].[SalesOrderDetailID]
OFFSET 10 ROWS FETCH NEXT 20 ROWS ONLY;
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }

        [TestMethod]
        public void DbSet_ToTempTable_table()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var commands = db.Products.GetToTempTableCommands();
                var expectedSql1 =
@"CREATE TABLE [#Product] (
    [ProductID] INT NOT NULL,
    [Name] NVARCHAR(50) NULL,
    [ProductNumber] NVARCHAR(25) NOT NULL,
    [Color] NVARCHAR(15) NULL,
    [StandardCost] MONEY NOT NULL,
    [ListPrice] MONEY NOT NULL,
    [Size] NVARCHAR(5) NULL,
    [Weight] DECIMAL(8, 2) NULL,
    [ProductCategoryID] INT NULL,
    [ProductModelID] INT NULL,
    [SellStartDate] DATETIME NOT NULL,
    [SellEndDate] DATETIME NULL,
    [DiscontinuedDate] DATETIME NULL,
    [ThumbNailPhoto] VARBINARY(MAX) NULL,
    [ThumbnailPhotoFileName] NVARCHAR(50) NULL,
    [RowGuid] UNIQUEIDENTIFIER NOT NULL DEFAULT(NEWID()),
    [ModifiedDate] DATETIME NOT NULL DEFAULT(GETDATE()),
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([ProductID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);
";
                var expectedSql2 =
@"INSERT INTO [#Product]
([ProductID], [Name], [ProductNumber], [Color], [StandardCost], [ListPrice], [Size], [Weight], [ProductCategoryID], [ProductModelID], [SellStartDate], [SellEndDate], [DiscontinuedDate], [ThumbNailPhoto], [ThumbnailPhotoFileName], [RowGuid], [ModifiedDate])
SELECT
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
FROM [SalesLT].[Product] [Product];
";
                Assert.AreEqual(expectedSql1, commands[0].ToTraceString());
                Assert.AreEqual(expectedSql2, commands[1].ToTraceString());
            }
        }

        [TestMethod]
        public void DbSet_ToTempTable_query()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var salesOrders = db.SalesOrders.Where(x => x.SalesOrderID == _Int32.Const(71774) | x.SalesOrderID == _Int32.Const(71776)).OrderBy(x => x.SalesOrderID);
                var commands = salesOrders.GetToTempTableCommands();
                var expectedSql1 =
@"CREATE TABLE [#SalesOrder] (
    [SalesOrderID] INT NOT NULL,
    [RevisionNumber] TINYINT NOT NULL DEFAULT(0),
    [OrderDate] DATETIME NOT NULL DEFAULT(GETDATE()),
    [DueDate] DATETIME NOT NULL,
    [ShipDate] DATETIME NULL,
    [Status] TINYINT NOT NULL DEFAULT(1),
    [OnlineOrderFlag] BIT NOT NULL DEFAULT(1),
    [SalesOrderNumber] NVARCHAR(25) NULL,
    [PurchaseOrderNumber] NVARCHAR(25) NULL,
    [AccountNumber] NVARCHAR(15) NULL,
    [CustomerID] INT NOT NULL,
    [ShipToAddressID] INT NULL,
    [BillToAddressID] INT NULL,
    [ShipMethod] NVARCHAR(50) NOT NULL,
    [CreditCardApprovalCode] NVARCHAR(15) NULL,
    [SubTotal] MONEY NOT NULL DEFAULT(0),
    [TaxAmt] MONEY NOT NULL DEFAULT(0),
    [Freight] MONEY NOT NULL DEFAULT(0),
    [TotalDue] MONEY NOT NULL DEFAULT(0),
    [Comment] NVARCHAR(MAX) NULL,
    [RowGuid] UNIQUEIDENTIFIER NOT NULL DEFAULT(NEWID()),
    [ModifiedDate] DATETIME NOT NULL DEFAULT(GETDATE()),
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);
";

                var expectedSql2 =
@"INSERT INTO [#SalesOrder]
([SalesOrderID], [RevisionNumber], [OrderDate], [DueDate], [ShipDate], [Status], [OnlineOrderFlag], [SalesOrderNumber], [PurchaseOrderNumber], [AccountNumber], [CustomerID], [ShipToAddressID], [BillToAddressID], [ShipMethod], [CreditCardApprovalCode], [SubTotal], [TaxAmt], [Freight], [TotalDue], [Comment], [RowGuid], [ModifiedDate])
SELECT
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
    [SalesOrder].[ModifiedDate] AS [ModifiedDate]
FROM [SalesLT].[SalesOrderHeader] [SalesOrder]
WHERE (([SalesOrder].[SalesOrderID] = 71774) OR ([SalesOrder].[SalesOrderID] = 71776))
ORDER BY [SalesOrder].[SalesOrderID];
";
                Assert.AreEqual(expectedSql1, commands[0].CommandText);
                Assert.AreEqual(expectedSql2, commands[1].CommandText);
            }
        }

        [TestMethod]
        public void DbSet_ToTempTable_child_query()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var salesOrders = db.SalesOrders.Where(x => x.SalesOrderID == 71774 | x.SalesOrderID == 71776).OrderBy(x => x.SalesOrderID);
                salesOrders.MockSequentialKeyTempTable();
                var childQuery = salesOrders.CreateChild(x => x.SalesOrderDetails, db.SalesOrderDetails.OrderBy(x => x.SalesOrderDetailID));
                var commands = childQuery.GetToTempTableCommands();

                var expectedSql1 =
@"CREATE TABLE [#SalesOrderDetail] (
    [SalesOrderID] INT NOT NULL,
    [SalesOrderDetailID] INT NOT NULL,
    [OrderQty] SMALLINT NOT NULL,
    [ProductID] INT NOT NULL,
    [UnitPrice] MONEY NOT NULL,
    [UnitPriceDiscount] MONEY NOT NULL DEFAULT(0),
    [LineTotal] MONEY NOT NULL,
    [RowGuid] UNIQUEIDENTIFIER NOT NULL DEFAULT(NEWID()),
    [ModifiedDate] DATETIME NOT NULL DEFAULT(GETDATE()),
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID], [SalesOrderDetailID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);
";

                var expectedSql2 =
@"INSERT INTO [#SalesOrderDetail]
([SalesOrderID], [SalesOrderDetailID], [OrderQty], [ProductID], [UnitPrice], [UnitPriceDiscount], [LineTotal], [RowGuid], [ModifiedDate])
SELECT
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
                Assert.AreEqual(expectedSql1, commands[0].CommandText);
                Assert.AreEqual(expectedSql2, commands[1].CommandText);
            }
        }

        [TestMethod]
        public void DbSet_ToTempTable_union_query()
        {
            var sqlVersion = SqlVersion.Sql11;
            using (var db = Db.Create(sqlVersion))
            {
                var unionQuery = db.Products.Where(x => x.ProductID < _Int32.Const(720)).UnionAll(db.Products.Where(x => x.ProductID > _Int32.Const(800)));
                var commands = unionQuery.GetToTempTableCommands();
                var expectedSql1 =
@"CREATE TABLE [#Product] (
    [ProductID] INT NOT NULL,
    [Name] NVARCHAR(50) NULL,
    [ProductNumber] NVARCHAR(25) NOT NULL,
    [Color] NVARCHAR(15) NULL,
    [StandardCost] MONEY NOT NULL,
    [ListPrice] MONEY NOT NULL,
    [Size] NVARCHAR(5) NULL,
    [Weight] DECIMAL(8, 2) NULL,
    [ProductCategoryID] INT NULL,
    [ProductModelID] INT NULL,
    [SellStartDate] DATETIME NOT NULL,
    [SellEndDate] DATETIME NULL,
    [DiscontinuedDate] DATETIME NULL,
    [ThumbNailPhoto] VARBINARY(MAX) NULL,
    [ThumbnailPhotoFileName] NVARCHAR(50) NULL,
    [RowGuid] UNIQUEIDENTIFIER NOT NULL DEFAULT(NEWID()),
    [ModifiedDate] DATETIME NOT NULL DEFAULT(GETDATE()),
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([ProductID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);
";
                var expectedSql2 =
@"INSERT INTO [#Product]
([ProductID], [Name], [ProductNumber], [Color], [StandardCost], [ListPrice], [Size], [Weight], [ProductCategoryID], [ProductModelID], [SellStartDate], [SellEndDate], [DiscontinuedDate], [ThumbNailPhoto], [ThumbnailPhotoFileName], [RowGuid], [ModifiedDate])
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
WHERE ([Product].[ProductID] > 800));
";
                Assert.AreEqual(expectedSql1, commands[0].ToTraceString());
                Assert.AreEqual(expectedSql2, commands[1].ToTraceString());
            }
        }
    }
}

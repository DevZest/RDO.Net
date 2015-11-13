using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Helpers;
using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
        public void DbSet_Where_multi_level()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var query = db.ProductCategories.Where(x => x.ParentProductCategoryID.IsNull()).Where(x => x.Name.IsNotNull());
                var expectedSql =
@"SELECT
    [ProductCategory].[ProductCategoryID] AS [ProductCategoryID],
    [ProductCategory].[ParentProductCategoryID] AS [ParentProductCategoryID],
    [ProductCategory].[Name] AS [Name],
    [ProductCategory].[RowGuid] AS [RowGuid],
    [ProductCategory].[ModifiedDate] AS [ModifiedDate]
FROM [SalesLT].[ProductCategory] [ProductCategory]
WHERE (([ProductCategory].[ParentProductCategoryID] IS NULL) AND ([ProductCategory].[Name] IS NOT NULL));
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
    }
}

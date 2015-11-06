using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Helpers;
using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DevZest.Data.Primitives;

namespace DevZest.Data
{
    [TestClass]
    public partial class DbTableInsertTests
    {
        [TestMethod]
        public void DbTable_Insert_DbSet()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var table = db.MockTempTable<ProductCategory>();
                var command = table.GetInsertCommand(db.ProductCategories.Where(x => x.ParentProductCategoryID.IsNull()));
                var expectedSql =
@"INSERT INTO [#ProductCategory]
([ProductCategoryID], [ParentProductCategoryID], [Name], [RowGuid], [ModifiedDate])
SELECT
    [ProductCategory].[ProductCategoryID] AS [ProductCategoryID],
    [ProductCategory].[ParentProductCategoryID] AS [ParentProductCategoryID],
    [ProductCategory].[Name] AS [Name],
    [ProductCategory].[RowGuid] AS [RowGuid],
    [ProductCategory].[ModifiedDate] AS [ModifiedDate]
FROM [SalesLT].[ProductCategory] [ProductCategory]
WHERE ([ProductCategory].[ParentProductCategoryID] IS NULL);
";
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }

        [TestMethod]
        public void DbTable_Insert_DbSet_auto_join()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var table = db.MockTempTable<ProductCategory>();
                var command = table.GetInsertCommand(db.ProductCategories.Where(x => x.ParentProductCategoryID.IsNull()), autoJoin: true);
                var expectedSql =
@"INSERT INTO [#ProductCategory]
([ProductCategoryID], [ParentProductCategoryID], [Name], [RowGuid], [ModifiedDate])
SELECT
    [ProductCategory].[ProductCategoryID] AS [ProductCategoryID],
    [ProductCategory].[ParentProductCategoryID] AS [ParentProductCategoryID],
    [ProductCategory].[Name] AS [Name],
    [ProductCategory].[RowGuid] AS [RowGuid],
    [ProductCategory].[ModifiedDate] AS [ModifiedDate]
FROM
    ([SalesLT].[ProductCategory] [ProductCategory]
    LEFT JOIN
    [#ProductCategory] [ProductCategory1]
    ON [ProductCategory].[ProductCategoryID] = [ProductCategory1].[ProductCategoryID])
WHERE (([ProductCategory].[ParentProductCategoryID] IS NULL) AND ([ProductCategory1].[ProductCategoryID] IS NULL));
";
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }

        [TestMethod]
        public void DbTable_Insert_Scalar()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var table = db.MockTempTable<ProductCategory>();
                var dataSet = DataSet<ProductCategory>.New();
                var dataRow = dataSet.AddRow();
                dataSet._.Name[dataRow] = "Name";
                dataSet._.ParentProductCategoryID[dataRow] = null;
                dataSet._.RowGuid[dataRow] = new Guid("040D9B64-05FD-4464-B398-74679C427980");
                dataSet._.ModifiedDate[dataRow] = new DateTime(2015, 9, 8);
                var command = table.GetInsertScalarCommand(dataSet, 0);
                var expectedSql =
@"DECLARE @p1 INT = 0;
DECLARE @p2 INT = NULL;
DECLARE @p3 NVARCHAR(50) = N'Name';
DECLARE @p4 UNIQUEIDENTIFIER = '040d9b64-05fd-4464-b398-74679c427980';
DECLARE @p5 DATETIME = '2015-09-08 00:00:00.000';

INSERT INTO [#ProductCategory]
([ProductCategoryID], [ParentProductCategoryID], [Name], [RowGuid], [ModifiedDate])
SELECT
    @p1 AS [ProductCategoryID],
    @p2 AS [ParentProductCategoryID],
    @p3 AS [Name],
    @p4 AS [RowGuid],
    @p5 AS [ModifiedDate];
";
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }

        [TestMethod]
        public void DbTable_Insert_Scalar_auto_join()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var table = db.MockTempTable<ProductCategory>();
                var dataSet = DataSet<ProductCategory>.New();
                var dataRow = dataSet.AddRow();
                dataSet._.Name[dataRow] = "Name";
                dataSet._.ParentProductCategoryID[dataRow] = null;
                dataSet._.RowGuid[dataRow] = new Guid("040D9B64-05FD-4464-B398-74679C427980");
                dataSet._.ModifiedDate[dataRow] = new DateTime(2015, 9, 8);
                var command = table.GetInsertScalarCommand(dataSet, 0, autoJoin: true);
                var expectedSql =
@"DECLARE @p1 INT = 0;
DECLARE @p2 INT = NULL;
DECLARE @p3 NVARCHAR(50) = N'Name';
DECLARE @p4 UNIQUEIDENTIFIER = '040d9b64-05fd-4464-b398-74679c427980';
DECLARE @p5 DATETIME = '2015-09-08 00:00:00.000';

INSERT INTO [#ProductCategory]
([ProductCategoryID], [ParentProductCategoryID], [Name], [RowGuid], [ModifiedDate])
SELECT
    @p1 AS [ProductCategoryID],
    @p2 AS [ParentProductCategoryID],
    @p3 AS [Name],
    @p4 AS [RowGuid],
    @p5 AS [ModifiedDate]
FROM
    ((SELECT @p1 AS [ProductCategoryID]) [ProductCategory]
    LEFT JOIN
    [#ProductCategory] [ProductCategory1]
    ON [ProductCategory].[ProductCategoryID] = [ProductCategory1].[ProductCategoryID])
WHERE ([ProductCategory1].[ProductCategoryID] IS NULL);
";
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }

        [TestMethod]
        public void DbTable_Insert_union_query_into_temp_table()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var tempTable = db.MockTempTable<Product>();
                var unionQuery = db.Products.Where(x => x.ProductID < _Int32.Const(720)).UnionAll(db.Products.Where(x => x.ProductID > _Int32.Const(800)));
                var command = tempTable.GetInsertCommand(unionQuery);
                var expectedSql =
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
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }

        [TestMethod]
        public void DbTable_Insert_BuildUpdateIdentityStatement()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var tempSalesOrders = db.MockTempTable<SalesOrder>();
                var identityOutput = db.MockTempTable<IdentityMapping>();
                var statement = DbTable<SalesOrder>.BuildUpdateIdentityStatement(tempSalesOrders, identityOutput);
                var command = db.GetUpdateCommand(statement);
                var expectedSql =
@"UPDATE [SalesOrder] SET
    [SalesOrderID] = [sys_identity_mapping].[NewValue]
FROM
    ([#sys_identity_mapping] [sys_identity_mapping]
    INNER JOIN
    [#SalesOrder] [SalesOrder]
    ON [sys_identity_mapping].[OldValue] = [SalesOrder].[SalesOrderID]);
";
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }
    }
}

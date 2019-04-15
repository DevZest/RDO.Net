using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Data.Primitives;
using DevZest.Data.Resources;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class SqlSessionTests
    {
        [TestMethod]
        public void SqlSession_BuildQuery()
        {
            using (var db = new Db(SqlVersion.Sql13))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategories);
                var query = db.BuildQuery(dataSet, null, (Data.ColumnMapper builder, ProductCategory source, Adhoc target) =>
                {
                    builder.Select(source.Name, target.AddColumn(source.Name, initializer: x => x.DbColumnName = source.Name.DbColumnName));
                });
                var expectedSql =
@"DECLARE @p1 NVARCHAR(MAX) = N'[{""ProductCategoryID"":1,""ParentProductCategoryID"":null,""Name"":""Bikes"",""RowGuid"":""cfbda25c-df71-47a7-b81b-64ee161aa37c"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":0},{""ProductCategoryID"":2,""ParentProductCategoryID"":null,""Name"":""Components"",""RowGuid"":""c657828d-d808-4aba-91a3-af2ce02300e9"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":1},{""ProductCategoryID"":3,""ParentProductCategoryID"":null,""Name"":""Clothing"",""RowGuid"":""10a7c342-ca82-48d4-8a38-46a2eb089b74"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":2},{""ProductCategoryID"":4,""ParentProductCategoryID"":null,""Name"":""Accessories"",""RowGuid"":""2be3be36-d9a2-4eee-b593-ed895d97c2a6"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":3}]';

SELECT [@ProductCategory].[Name] AS [Name]
FROM OPENJSON(@p1) WITH (
    [ProductCategoryID] INT,
    [ParentProductCategoryID] INT,
    [Name] NVARCHAR(50),
    [RowGuid] UNIQUEIDENTIFIER,
    [ModifiedDate] DATETIME,
    [sys_dataset_ordinal] INT) AS [@ProductCategory]
ORDER BY [@ProductCategory].[sys_dataset_ordinal] ASC;
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }

        [TestMethod]
        public void SqlSession_BuildQuery_json()
        {
            using (var db = new Db(SqlVersion.Sql13))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategories);
                var query = db.BuildQuery(dataSet, null, (Data.ColumnMapper builder, ProductCategory source, Adhoc target) =>
                {
                    builder.Select(source.Name, target.AddColumn(source.Name, initializer: x => x.DbColumnName = source.Name.DbColumnName));
                });
                var expectedSql =
@"DECLARE @p1 NVARCHAR(MAX) = N'[{""ProductCategoryID"":1,""ParentProductCategoryID"":null,""Name"":""Bikes"",""RowGuid"":""cfbda25c-df71-47a7-b81b-64ee161aa37c"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":0},{""ProductCategoryID"":2,""ParentProductCategoryID"":null,""Name"":""Components"",""RowGuid"":""c657828d-d808-4aba-91a3-af2ce02300e9"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":1},{""ProductCategoryID"":3,""ParentProductCategoryID"":null,""Name"":""Clothing"",""RowGuid"":""10a7c342-ca82-48d4-8a38-46a2eb089b74"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":2},{""ProductCategoryID"":4,""ParentProductCategoryID"":null,""Name"":""Accessories"",""RowGuid"":""2be3be36-d9a2-4eee-b593-ed895d97c2a6"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":3}]';

SELECT [@ProductCategory].[Name] AS [Name]
FROM OPENJSON(@p1) WITH (
    [ProductCategoryID] INT,
    [ParentProductCategoryID] INT,
    [Name] NVARCHAR(50),
    [RowGuid] UNIQUEIDENTIFIER,
    [ModifiedDate] DATETIME,
    [sys_dataset_ordinal] INT) AS [@ProductCategory]
ORDER BY [@ProductCategory].[sys_dataset_ordinal] ASC;
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }

        [TestMethod]
        public void SqlSession_BuildImportQuery()
        {
            using (var db = new Db(SqlVersion.Sql13))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategories);
                var query = db.BuildImportQuery(dataSet);
                var expectedSql =
@"DECLARE @p1 NVARCHAR(MAX) = N'[{""ProductCategoryID"":1,""ParentProductCategoryID"":null,""Name"":""Bikes"",""RowGuid"":""cfbda25c-df71-47a7-b81b-64ee161aa37c"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":0},{""ProductCategoryID"":2,""ParentProductCategoryID"":null,""Name"":""Components"",""RowGuid"":""c657828d-d808-4aba-91a3-af2ce02300e9"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":1},{""ProductCategoryID"":3,""ParentProductCategoryID"":null,""Name"":""Clothing"",""RowGuid"":""10a7c342-ca82-48d4-8a38-46a2eb089b74"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":2},{""ProductCategoryID"":4,""ParentProductCategoryID"":null,""Name"":""Accessories"",""RowGuid"":""2be3be36-d9a2-4eee-b593-ed895d97c2a6"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":3}]';

SELECT
    [@ProductCategory].[ProductCategoryID] AS [ProductCategoryID],
    [@ProductCategory].[ParentProductCategoryID] AS [ParentProductCategoryID],
    [@ProductCategory].[Name] AS [Name],
    [@ProductCategory].[RowGuid] AS [RowGuid],
    [@ProductCategory].[ModifiedDate] AS [ModifiedDate]
FROM OPENJSON(@p1) WITH (
    [ProductCategoryID] INT,
    [ParentProductCategoryID] INT,
    [Name] NVARCHAR(50),
    [RowGuid] UNIQUEIDENTIFIER,
    [ModifiedDate] DATETIME,
    [sys_dataset_ordinal] INT) AS [@ProductCategory]
ORDER BY [@ProductCategory].[sys_dataset_ordinal] ASC;
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }
    }
}

using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DevZest.Data.Resources;
using DevZest.Data.SqlServer.Helpers;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class DbTableUpdateTests
    {
        [TestMethod]
        public void DbTable_Update_without_source()
        {
            using (var db = new Db(SqlVersion.Sql13))
            {
                var command = db.ProductCategory.MockUpdate(0, (builder, x) => builder.Select(_DateTime.Now(), x.ModifiedDate),
                    x => x.ModifiedDate.IsNull());
                var expectedSql =
@"UPDATE [ProductCategory] SET
    [ModifiedDate] = GETDATE()
FROM [SalesLT].[ProductCategory] [ProductCategory]
WHERE ([ProductCategory].[ModifiedDate] IS NULL);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Update_from_temp_table()
        {
            using (var db = new Db(SqlVersion.Sql13))
            {
                var tempTable = db.MockTempTable<ProductCategory>();
                var command = db.ProductCategory.MockUpdate(0, tempTable);
                var expectedSql =
@"UPDATE [ProductCategory] SET
    [ParentProductCategoryID] = [#ProductCategory].[ParentProductCategoryID],
    [Name] = [#ProductCategory].[Name],
    [RowGuid] = [#ProductCategory].[RowGuid],
    [ModifiedDate] = [#ProductCategory].[ModifiedDate]
FROM
    ([#ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory]
    ON [#ProductCategory].[ProductCategoryID] = [ProductCategory].[ProductCategoryID]);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Update_from_query()
        {
            using (var db = new Db(SqlVersion.Sql13))
            {
                var query = db.ProductCategory.Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategory.MockUpdate(0, query);
                var expectedSql =
@"UPDATE [ProductCategory] SET
    [ParentProductCategoryID] = [ProductCategory].[ParentProductCategoryID],
    [Name] = [ProductCategory].[Name],
    [RowGuid] = [ProductCategory].[RowGuid],
    [ModifiedDate] = [ProductCategory].[ModifiedDate]
FROM
    ([SalesLT].[ProductCategory] [ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory1]
    ON [ProductCategory].[ProductCategoryID] = [ProductCategory1].[ProductCategoryID])
WHERE ([ProductCategory].[ModifiedDate] IS NULL);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Update_from_simple_query()
        {
            using (var db = new Db(SqlVersion.Sql13))
            {
                var query = db.MockTempTable<ProductCategory>().Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategory.MockUpdate(0, query);
                var expectedSql =
@"UPDATE [ProductCategory] SET
    [ParentProductCategoryID] = [#ProductCategory].[ParentProductCategoryID],
    [Name] = [#ProductCategory].[Name],
    [RowGuid] = [#ProductCategory].[RowGuid],
    [ModifiedDate] = [#ProductCategory].[ModifiedDate]
FROM
    ([#ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory]
    ON [#ProductCategory].[ProductCategoryID] = [ProductCategory].[ProductCategoryID])
WHERE ([#ProductCategory].[ModifiedDate] IS NULL);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Update_scalar()
        {
            using (var db = new Db(SqlVersion.Sql13))
            {
                var dataSet = DataSet<ProductCategory>.Create();
                var index = dataSet.AddRow().Ordinal;
                dataSet._.Name[index] = "Name";
                dataSet._.RowGuid[index] = new Guid("EC359D7D-AE3A-4A9D-BDCB-03F0A7799514");
                dataSet._.ModifiedDate[index] = new DateTime(2015, 9, 23);
                var command = db.ProductCategory.MockUpdate(true, dataSet, 0);
                var expectedSql =
@"DECLARE @p1 INT = NULL;
DECLARE @p2 NVARCHAR(50) = N'Name';
DECLARE @p3 UNIQUEIDENTIFIER = 'ec359d7d-ae3a-4a9d-bdcb-03f0a7799514';
DECLARE @p4 DATETIME = '2015-09-23 00:00:00.000';
DECLARE @p5 INT = 0;

UPDATE [ProductCategory] SET
    [ParentProductCategoryID] = @p1,
    [Name] = @p2,
    [RowGuid] = @p3,
    [ModifiedDate] = @p4
FROM
    ((SELECT @p5 AS [ProductCategoryID]) [@ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory]
    ON [@ProductCategory].[ProductCategoryID] = [ProductCategory].[ProductCategoryID]);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Update_from_DataSet_Sql13()
        {
            using (var db = new Db(SqlVersion.Sql13))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategoriesLevel1);
                var expectedSql =
@"DECLARE @p1 NVARCHAR(MAX) = N'[{""ProductCategoryID"":1,""ParentProductCategoryID"":null,""Name"":""Bikes"",""RowGuid"":""cfbda25c-df71-47a7-b81b-64ee161aa37c"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":0},{""ProductCategoryID"":2,""ParentProductCategoryID"":null,""Name"":""Other"",""RowGuid"":""c657828d-d808-4aba-91a3-af2ce02300e9"",""ModifiedDate"":""2002-06-01T00:00:00"",""sys_dataset_ordinal"":1}]';

UPDATE [ProductCategory] SET
    [ParentProductCategoryID] = [@ProductCategory].[ParentProductCategoryID],
    [Name] = [@ProductCategory].[Name],
    [RowGuid] = [@ProductCategory].[RowGuid],
    [ModifiedDate] = [@ProductCategory].[ModifiedDate]
FROM
    (OPENJSON(@p1) WITH (
        [ProductCategoryID] INT,
        [ParentProductCategoryID] INT,
        [Name] NVARCHAR(50),
        [RowGuid] UNIQUEIDENTIFIER,
        [ModifiedDate] DATETIME,
        [sys_dataset_ordinal] INT) AS [@ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory]
    ON [@ProductCategory].[ProductCategoryID] = [ProductCategory].[ProductCategoryID]);
";
                var command = db.ProductCategory.MockUpdate(dataSet.Count, dataSet);
                command.Verify(expectedSql);
            }
        }
    }
}

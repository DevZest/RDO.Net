using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.SqlServer;
using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class DbTableUpdateTests
    {
        [TestMethod]
        public void DbTable_Update_without_from()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var command = db.ProductCategories.GetUpdateCommand((builder, x) => builder.Select(Functions.GetDate(), x.ModifiedDate),
                    x => x.ModifiedDate.IsNull());
                var expectedSql =
@"UPDATE [ProductCategory] SET
    [ModifiedDate] = GETDATE()
FROM [SalesLT].[ProductCategory] [ProductCategory]
WHERE ([ProductCategory].[ModifiedDate] IS NULL);
";
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }

        [TestMethod]
        public void DbTable_Update_from_temp_table()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var tempTable = db.ProductCategories.MockTempTable();
                var command = db.ProductCategories.GetUpdateCommand(tempTable);
                var expectedSql =
@"UPDATE [ProductCategory1] SET
    [ParentProductCategoryID] = [ProductCategory].[ParentProductCategoryID],
    [Name] = [ProductCategory].[Name],
    [RowGuid] = [ProductCategory].[RowGuid],
    [ModifiedDate] = [ProductCategory].[ModifiedDate]
FROM
    ([#ProductCategory] [ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory1]
    ON [ProductCategory].[ProductCategoryID] = [ProductCategory1].[ProductCategoryID]);
";
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }

        [TestMethod]
        public void DbTable_Update_from_query()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var query = db.ProductCategories.Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategories.GetUpdateCommand(query);
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
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }

        [TestMethod]
        public void DbTable_Update_from_simple_query()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var query = db.ProductCategories.MockTempTable().Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategories.GetUpdateCommand(query);
                var expectedSql =
@"UPDATE [ProductCategory1] SET
    [ParentProductCategoryID] = [ProductCategory].[ParentProductCategoryID],
    [Name] = [ProductCategory].[Name],
    [RowGuid] = [ProductCategory].[RowGuid],
    [ModifiedDate] = [ProductCategory].[ModifiedDate]
FROM
    ([#ProductCategory] [ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory1]
    ON [ProductCategory].[ProductCategoryID] = [ProductCategory1].[ProductCategoryID])
WHERE ([ProductCategory].[ModifiedDate] IS NULL);
";
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }

        [TestMethod]
        public void DbTable_Update_scalar()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var dataSet = DataSet<ProductCategory>.New();
                var index = dataSet.AddRow().Ordinal;
                dataSet._.Name[index] = "Name";
                dataSet._.RowGuid[index] = new Guid("EC359D7D-AE3A-4A9D-BDCB-03F0A7799514");
                dataSet._.ModifiedDate[index] = new DateTime(2015, 9, 23);
                var command = db.ProductCategories.GetUpdateScalarCommand(dataSet, 0);
                var expectedSql =
@"DECLARE @p1 INT = NULL;
DECLARE @p2 NVARCHAR(50) = N'Name';
DECLARE @p3 UNIQUEIDENTIFIER = 'ec359d7d-ae3a-4a9d-bdcb-03f0a7799514';
DECLARE @p4 DATETIME = '2015-09-23 00:00:00.000';
DECLARE @p5 INT = 0;

UPDATE [ProductCategory1] SET
    [ParentProductCategoryID] = @p1,
    [Name] = @p2,
    [RowGuid] = @p3,
    [ModifiedDate] = @p4
FROM
    ((SELECT @p5 AS [ProductCategoryID]) [ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory1]
    ON [ProductCategory].[ProductCategoryID] = [ProductCategory1].[ProductCategoryID]);
";
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }
    }
}

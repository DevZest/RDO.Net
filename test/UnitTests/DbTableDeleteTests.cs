using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.SqlServer;
using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class DbTableDeleteTests
    {
        [TestMethod]
        public void DbTable_Delete_without_from()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var command = db.ProductCategories.MockDelete(x => x.ModifiedDate.IsNull());
                var expectedSql =
@"DELETE [ProductCategory]
FROM [SalesLT].[ProductCategory] [ProductCategory]
WHERE ([ProductCategory].[ModifiedDate] IS NULL);
";
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }

        [TestMethod]
        public void DbTable_Delete_from_temp_table()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var tempTable = db.MockTempTable<ProductCategory>();
                var command = db.ProductCategories.MockDelete(tempTable);
                var expectedSql =
@"DELETE [ProductCategory1]
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
        public void DbTable_Delete_from_query()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var query = db.ProductCategories.Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategories.MockDelete(query);
                var expectedSql =
@"DELETE [ProductCategory]
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
        public void DbTable_Delete_from_simple_query()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var query = db.MockTempTable<ProductCategory>().Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategories.MockDelete(query);
                var expectedSql =
@"DELETE [ProductCategory1]
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
        public void DbTable_Delete_scalar()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var dataSet = DataSet<ProductCategory>.New();
                var index = dataSet.AddRow().Ordinal;
                dataSet._.Name[index] = "Name";
                dataSet._.RowGuid[index] = new Guid("EC359D7D-AE3A-4A9D-BDCB-03F0A7799514");
                dataSet._.ModifiedDate[index] = new DateTime(2015, 9, 23);
                var command = db.ProductCategories.MockDelete(dataSet, 0);
                var expectedSql =
@"DECLARE @p1 INT = 0;

DELETE [ProductCategory1]
FROM
    ((SELECT @p1 AS [ProductCategoryID]) [ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory1]
    ON [ProductCategory].[ProductCategoryID] = [ProductCategory1].[ProductCategoryID]);
";
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }
    }
}

﻿using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DevZest.Data.Resources;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class DbTableDeleteTests
    {
        [TestMethod]
        public void DbTable_Delete_without_from()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var command = db.ProductCategory.MockDelete(x => x.ModifiedDate.IsNull());
                var expectedSql =
@"DELETE [ProductCategory]
FROM [SalesLT].[ProductCategory] [ProductCategory]
WHERE ([ProductCategory].[ModifiedDate] IS NULL);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Delete_from_temp_table()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var tempTable = db.MockTempTable<ProductCategory>();
                var command = db.ProductCategory.MockDelete(tempTable, (s, _) => s.Match(_));
                var expectedSql =
@"DELETE [ProductCategory1]
FROM
    ([#ProductCategory] [ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory1]
    ON [ProductCategory].[ProductCategoryID] = [ProductCategory1].[ProductCategoryID]);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Delete_from_query()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var query = db.ProductCategory.Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategory.MockDelete(query, (s, _) => s.Match(_));
                var expectedSql =
@"DELETE [ProductCategory]
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
        public void DbTable_Delete_from_simple_query()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var query = db.MockTempTable<ProductCategory>().Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategory.MockDelete(query, (s, _) => s.Match(_));
                var expectedSql =
@"DELETE [ProductCategory1]
FROM
    ([#ProductCategory] [ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory1]
    ON [ProductCategory].[ProductCategoryID] = [ProductCategory1].[ProductCategoryID])
WHERE ([ProductCategory].[ModifiedDate] IS NULL);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Delete_scalar()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var dataSet = DataSet<ProductCategory>.Create();
                var index = dataSet.AddRow().Ordinal;
                dataSet._.Name[index] = "Name";
                dataSet._.RowGuid[index] = new Guid("EC359D7D-AE3A-4A9D-BDCB-03F0A7799514");
                dataSet._.ModifiedDate[index] = new DateTime(2015, 9, 23);
                var command = db.ProductCategory.MockDelete(dataSet, 0, (s, _) => s.Match(_));
                var expectedSql =
@"DECLARE @p1 INT = 0;

DELETE [ProductCategory1]
FROM
    ((SELECT @p1 AS [ProductCategoryID]) [ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory1]
    ON [ProductCategory].[ProductCategoryID] = [ProductCategory1].[ProductCategoryID]);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Delete_from_DataSet()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var salesOrder = DataSet<SalesOrder>.ParseJson(Json.SalesOrder_71774);
                var salesOrderDetails = salesOrder.Children(x => x.SalesOrderDetails);
                var command = db.SalesOrderDetail.MockDelete(salesOrderDetails, (s, _) => s.Match(_));
                var expectedSql =
@"DECLARE @p1 XML = N'<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <row>
    <col_0>71774</col_0>
    <col_1>110562</col_1>
    <col_2>1</col_2>
  </row>
  <row>
    <col_0>71774</col_0>
    <col_1>110563</col_1>
    <col_2>2</col_2>
  </row>
</root>';

DELETE [SalesOrderDetail]
FROM
    (@p1.nodes('/root/row') [@SalesOrderDetail]([Xml])
    INNER JOIN
    [SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    ON [@SalesOrderDetail].[Xml].value('col_0[1]/text()[1]', 'INT') = [SalesOrderDetail].[SalesOrderID] AND [@SalesOrderDetail].[Xml].value('col_1[1]/text()[1]', 'INT') = [SalesOrderDetail].[SalesOrderDetailID]);
";
                command.Verify(expectedSql);
            }
        }
    }
}

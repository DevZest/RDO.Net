using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DevZest.Data.Resources;
using DevZest.Data.SqlServer.Helpers;

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
                var command = db.ProductCategory.MockDelete(0, x => x.ModifiedDate.IsNull());
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
                var command = db.ProductCategory.MockDelete(0, tempTable, (s, _) => s.Match(_));
                var expectedSql =
@"DELETE [ProductCategory]
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
        public void DbTable_Delete_from_query()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var query = db.ProductCategory.Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategory.MockDelete(0, query, (s, _) => s.Match(_));
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
                var command = db.ProductCategory.MockDelete(0, query, (s, _) => s.Match(_));
                var expectedSql =
@"DELETE [ProductCategory]
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
        public void DbTable_Delete_scalar()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var dataSet = DataSet<ProductCategory>.Create();
                var index = dataSet.AddRow().Ordinal;
                dataSet._.Name[index] = "Name";
                dataSet._.RowGuid[index] = new Guid("EC359D7D-AE3A-4A9D-BDCB-03F0A7799514");
                dataSet._.ModifiedDate[index] = new DateTime(2015, 9, 23);
                var command = db.ProductCategory.MockDelete(false, dataSet, 0, (s, _) => s.Match(_));
                var expectedSql =
@"DECLARE @p1 INT = 0;

DELETE [ProductCategory]
FROM
    ((SELECT @p1 AS [ProductCategoryID]) [@ProductCategory]
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory]
    ON [@ProductCategory].[ProductCategoryID] = [ProductCategory].[ProductCategoryID]);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Delete_from_DataSet_Sql11()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var salesOrder = DataSet<SalesOrder>.ParseJson(Json.SalesOrder_71774);
                var salesOrderDetails = salesOrder.Children(x => x.SalesOrderDetails);
                var command = db.SalesOrderDetail.MockDelete(0, salesOrderDetails, (s, _) => s.Match(_));
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

        [TestMethod]
        public void DbTable_Delete_from_DataSet_Sql13()
        {
            using (var db = new Db(SqlVersion.Sql13))
            {
                var salesOrder = DataSet<SalesOrder>.ParseJson(Json.SalesOrder_71774);
                var salesOrderDetails = salesOrder.Children(x => x.SalesOrderDetails);
                var command = db.SalesOrderDetail.MockDelete(0, salesOrderDetails, (s, _) => s.Match(_));
                var expectedSql =
@"DECLARE @p1 NVARCHAR(MAX) = N'[{""SalesOrderID"":71774,""SalesOrderDetailID"":110562,""OrderQty"":1,""ProductID"":836,""UnitPrice"":356.8980,""UnitPriceDiscount"":0,""LineTotal"":356.8980,""RowGuid"":""e3a1994c-7a68-4ce8-96a3-77fdd3bbd730"",""ModifiedDate"":""2008-06-01T00:00:00"",""sys_dataset_ordinal"":0},{""SalesOrderID"":71774,""SalesOrderDetailID"":110563,""OrderQty"":1,""ProductID"":822,""UnitPrice"":356.8980,""UnitPriceDiscount"":0,""LineTotal"":356.8980,""RowGuid"":""5c77f557-fdb6-43ba-90b9-9a7aec55ca32"",""ModifiedDate"":""2008-06-01T00:00:00"",""sys_dataset_ordinal"":1}]';

DELETE [SalesOrderDetail]
FROM
    (OPENJSON(@p1) WITH (
        [SalesOrderID] INT,
        [SalesOrderDetailID] INT,
        [OrderQty] SMALLINT,
        [ProductID] INT,
        [UnitPrice] MONEY,
        [UnitPriceDiscount] MONEY,
        [LineTotal] MONEY,
        [RowGuid] UNIQUEIDENTIFIER,
        [ModifiedDate] DATETIME,
        [sys_dataset_ordinal] INT) AS [@SalesOrderDetail]
    INNER JOIN
    [SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    ON [@SalesOrderDetail].[SalesOrderID] = [SalesOrderDetail].[SalesOrderID] AND [@SalesOrderDetail].[SalesOrderDetailID] = [SalesOrderDetail].[SalesOrderDetailID]);
";
                command.Verify(expectedSql);
            }
        }
    }
}

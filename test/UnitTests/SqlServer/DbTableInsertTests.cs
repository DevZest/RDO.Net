﻿using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DevZest.Data.Resources;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public partial class DbTableInsertTests
    {
        [TestMethod]
        public void DbTable_Insert_from_DbTable()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var table = db.MockTempTable<Product>();
                var commands = table.MockInsert(0, db.Product);
                var expectedSql =
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
                commands.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_from_DbQuery()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var table = db.MockTempTable<ProductCategory>();
                var command = table.MockInsert(0, db.ProductCategory.Where(x => x.ParentProductCategoryID.IsNull()));
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
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_from_DbQuery_skipExisting()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var table = db.MockTempTable<ProductCategory>();
                var command = table.MockInsert(0, db.ProductCategory.Where(x => x.ParentProductCategoryID.IsNull()), skipExisting: true);
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
    [#ProductCategory]
    ON [ProductCategory].[ProductCategoryID] = [#ProductCategory].[ProductCategoryID])
WHERE (([ProductCategory].[ParentProductCategoryID] IS NULL) AND ([#ProductCategory].[ProductCategoryID] IS NULL));
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_from_child_DbQuery()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var salesOrders = db.SalesOrderHeader.ToDbQuery<SalesOrder>().Where(x => x.SalesOrderID == 71774 | x.SalesOrderID == 71776).OrderBy(x => x.SalesOrderID);
                salesOrders.MockSequentialKeyTempTable();
                var childQuery = salesOrders.CreateChildAsync(x => x.SalesOrderDetails, db.SalesOrderDetail.OrderBy(x => x.SalesOrderDetailID)).Result;
                var tempTable = db.MockTempTable<SalesOrderDetail>();
                var command = tempTable.MockInsert(0, childQuery);

                var expectedSql =
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
    [#sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [#sys_sequential_SalesOrder].[SalesOrderID])
ORDER BY [#sys_sequential_SalesOrder].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_Scalar()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var table = db.MockTempTable<ProductCategory>();
                var dataSet = DataSet<ProductCategory>.Create();
                var dataRow = dataSet.AddRow();
                dataSet._.Name[dataRow] = "Name";
                dataSet._.ParentProductCategoryID[dataRow] = null;
                dataSet._.RowGuid[dataRow] = new Guid("040D9B64-05FD-4464-B398-74679C427980");
                dataSet._.ModifiedDate[dataRow] = new DateTime(2015, 9, 8);
                var command = table.MockInsert(true, dataSet, 0);
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
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_Scalar_updateIdentity()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var table = db.ProductCategory;
                var dataSet = DataSet<ProductCategory>.Create();
                var dataRow = dataSet.AddRow();
                dataSet._.Name[dataRow] = "Name";
                dataSet._.ParentProductCategoryID[dataRow] = null;
                dataSet._.RowGuid[dataRow] = new Guid("040D9B64-05FD-4464-B398-74679C427980");
                dataSet._.ModifiedDate[dataRow] = new DateTime(2015, 9, 8);
                var command = table.MockInsert(true, dataSet, 0, updateIdentity: true);
                var expectedSql =
@"DECLARE @p1 INT = NULL;
DECLARE @p2 NVARCHAR(50) = N'Name';
DECLARE @p3 UNIQUEIDENTIFIER = '040d9b64-05fd-4464-b398-74679c427980';
DECLARE @p4 DATETIME = '2015-09-08 00:00:00.000';

INSERT INTO [SalesLT].[ProductCategory]
([ParentProductCategoryID], [Name], [RowGuid], [ModifiedDate])
SELECT
    @p1 AS [ParentProductCategoryID],
    @p2 AS [Name],
    @p3 AS [RowGuid],
    @p4 AS [ModifiedDate];

SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_Scalar_skipExisting()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var table = db.MockTempTable<ProductCategory>();
                var dataSet = DataSet<ProductCategory>.Create();
                var dataRow = dataSet.AddRow();
                dataSet._.Name[dataRow] = "Name";
                dataSet._.ParentProductCategoryID[dataRow] = null;
                dataSet._.RowGuid[dataRow] = new Guid("040D9B64-05FD-4464-B398-74679C427980");
                dataSet._.ModifiedDate[dataRow] = new DateTime(2015, 9, 8);
                var command = table.MockInsert(true, dataSet, 0, skipExisting: true);
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
    ((SELECT @p1 AS [ProductCategoryID]) [@ProductCategory]
    LEFT JOIN
    [#ProductCategory]
    ON [@ProductCategory].[ProductCategoryID] = [#ProductCategory].[ProductCategoryID])
WHERE ([#ProductCategory].[ProductCategoryID] IS NULL);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_union_query()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var tempTable = db.MockTempTable<Product>();
                var unionQuery = db.Product.Where(x => x.ProductID < _Int32.Const(720)).UnionAll(db.Product.Where(x => x.ProductID > _Int32.Const(800)));
                var command = tempTable.MockInsert(0, unionQuery);
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
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_BuildUpdateIdentityStatement()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var tempSalesOrders = db.MockTempTable<SalesOrder>();
                var identityOutput = db.MockTempTable<Int32IdentityMapping>();
                var statements = tempSalesOrders.BuildUpdateIdentityStatement(identityOutput);
                Assert.AreEqual(1, statements.Count);
                var command = db.GetUpdateCommand(statements[0]);
                var expectedSql =
@"UPDATE [#SalesOrder] SET
    [SalesOrderID] = [#sys_identity_mapping].[NewValue]
FROM
    ([#sys_identity_mapping]
    INNER JOIN
    [#SalesOrder]
    ON [#sys_identity_mapping].[OldValue] = [#SalesOrder].[SalesOrderID]);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_from_DataSet_Sql11()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategories);
                var tempTable = db.MockTempTable<ProductCategory>();
                var commands = tempTable.MockInsert(4, dataSet);

                var expectedSql =
@"DECLARE @p1 XML = N'<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <row>
    <col_0>1</col_0>
    <col_1></col_1>
    <col_2>Bikes</col_2>
    <col_3>cfbda25c-df71-47a7-b81b-64ee161aa37c</col_3>
    <col_4>2002-06-01 00:00:00.000</col_4>
    <col_5>1</col_5>
  </row>
  <row>
    <col_0>2</col_0>
    <col_1></col_1>
    <col_2>Components</col_2>
    <col_3>c657828d-d808-4aba-91a3-af2ce02300e9</col_3>
    <col_4>2002-06-01 00:00:00.000</col_4>
    <col_5>2</col_5>
  </row>
  <row>
    <col_0>3</col_0>
    <col_1></col_1>
    <col_2>Clothing</col_2>
    <col_3>10a7c342-ca82-48d4-8a38-46a2eb089b74</col_3>
    <col_4>2002-06-01 00:00:00.000</col_4>
    <col_5>3</col_5>
  </row>
  <row>
    <col_0>4</col_0>
    <col_1></col_1>
    <col_2>Accessories</col_2>
    <col_3>2be3be36-d9a2-4eee-b593-ed895d97c2a6</col_3>
    <col_4>2002-06-01 00:00:00.000</col_4>
    <col_5>4</col_5>
  </row>
</root>';

INSERT INTO [#ProductCategory]
([ProductCategoryID], [ParentProductCategoryID], [Name], [RowGuid], [ModifiedDate])
SELECT
    [@ProductCategory].[Xml].value('col_0[1]/text()[1]', 'INT') AS [ProductCategoryID],
    [@ProductCategory].[Xml].value('col_1[1]/text()[1]', 'INT') AS [ParentProductCategoryID],
    [@ProductCategory].[Xml].value('col_2[1]/text()[1]', 'NVARCHAR(50)') AS [Name],
    [@ProductCategory].[Xml].value('col_3[1]/text()[1]', 'UNIQUEIDENTIFIER') AS [RowGuid],
    [@ProductCategory].[Xml].value('col_4[1]/text()[1]', 'DATETIME') AS [ModifiedDate]
FROM @p1.nodes('/root/row') [@ProductCategory]([Xml])
ORDER BY [@ProductCategory].[Xml].value('col_5[1]/text()[1]', 'INT') ASC;
";

                commands.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_from_DataSet_Sql13()
        {
            using (var db = new Db(SqlVersion.Sql13))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategories);
                var tempTable = db.MockTempTable<ProductCategory>();
                var commands = tempTable.MockInsert(4, dataSet);

                var expectedSql =
@"DECLARE @p1 NVARCHAR(MAX) = N'[{""ProductCategoryID"":1,""ParentProductCategoryID"":null,""Name"":""Bikes"",""RowGuid"":""cfbda25c-df71-47a7-b81b-64ee161aa37c"",""ModifiedDate"":""2002-06-01T00:00:00.000"",""sys_dataset_ordinal"":0},{""ProductCategoryID"":2,""ParentProductCategoryID"":null,""Name"":""Components"",""RowGuid"":""c657828d-d808-4aba-91a3-af2ce02300e9"",""ModifiedDate"":""2002-06-01T00:00:00.000"",""sys_dataset_ordinal"":1},{""ProductCategoryID"":3,""ParentProductCategoryID"":null,""Name"":""Clothing"",""RowGuid"":""10a7c342-ca82-48d4-8a38-46a2eb089b74"",""ModifiedDate"":""2002-06-01T00:00:00.000"",""sys_dataset_ordinal"":2},{""ProductCategoryID"":4,""ParentProductCategoryID"":null,""Name"":""Accessories"",""RowGuid"":""2be3be36-d9a2-4eee-b593-ed895d97c2a6"",""ModifiedDate"":""2002-06-01T00:00:00.000"",""sys_dataset_ordinal"":3}]';

INSERT INTO [#ProductCategory]
([ProductCategoryID], [ParentProductCategoryID], [Name], [RowGuid], [ModifiedDate])
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

                commands.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_from_DataSet_updateIdentity_Sql13()
        {
            using (var db = new Db(SqlVersion.Sql13))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategories);
                var table = db.ProductCategory;
                var commands = table.MockInsert(4, dataSet, updateIdentity: true);

                var expectedSql = new string[] {
@"CREATE TABLE [#IdentityOutput] (
    [NewValue] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY CLUSTERED ([sys_row_id] ASC)
);",

@"DECLARE @p1 NVARCHAR(MAX) = N'[{""ProductCategoryID"":1,""ParentProductCategoryID"":null,""Name"":""Bikes"",""RowGuid"":""cfbda25c-df71-47a7-b81b-64ee161aa37c"",""ModifiedDate"":""2002-06-01T00:00:00.000"",""sys_dataset_ordinal"":0},{""ProductCategoryID"":2,""ParentProductCategoryID"":null,""Name"":""Components"",""RowGuid"":""c657828d-d808-4aba-91a3-af2ce02300e9"",""ModifiedDate"":""2002-06-01T00:00:00.000"",""sys_dataset_ordinal"":1},{""ProductCategoryID"":3,""ParentProductCategoryID"":null,""Name"":""Clothing"",""RowGuid"":""10a7c342-ca82-48d4-8a38-46a2eb089b74"",""ModifiedDate"":""2002-06-01T00:00:00.000"",""sys_dataset_ordinal"":2},{""ProductCategoryID"":4,""ParentProductCategoryID"":null,""Name"":""Accessories"",""RowGuid"":""2be3be36-d9a2-4eee-b593-ed895d97c2a6"",""ModifiedDate"":""2002-06-01T00:00:00.000"",""sys_dataset_ordinal"":3}]';

INSERT INTO [SalesLT].[ProductCategory]
([ParentProductCategoryID], [Name], [RowGuid], [ModifiedDate])
OUTPUT INSERTED.[ProductCategoryID] INTO [#IdentityOutput] ([NewValue])
SELECT
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
ORDER BY [@ProductCategory].[sys_dataset_ordinal] ASC;"
                };

                commands.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_into_child_temp_table_optimized()
        {
            var salesOrders = DataSet<SalesOrder>.ParseJson(Json.SalesOrder_71774);
            using (var db = new Db(SqlVersion.Sql11))
            {
                var tempSalesOrders = db.MockTempTable<SalesOrder>();
                var tempSalesOrderDetails = tempSalesOrders.MockCreateChild(x => x.SalesOrderDetails);
                tempSalesOrders.MockInsert(true, salesOrders, 0);
                var salesOrderDetails = salesOrders.Children(x => x.SalesOrderDetails, 0);
                var command = tempSalesOrderDetails.MockInsert(salesOrderDetails.Count, salesOrderDetails);
                var expectedSql =
@"DECLARE @p1 XML = N'<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <row>
    <col_0>71774</col_0>
    <col_1>110562</col_1>
    <col_2>1</col_2>
    <col_3>836</col_3>
    <col_4>356.8980</col_4>
    <col_5>0</col_5>
    <col_6>356.8980</col_6>
    <col_7>e3a1994c-7a68-4ce8-96a3-77fdd3bbd730</col_7>
    <col_8>2008-06-01 00:00:00.000</col_8>
    <col_9>1</col_9>
  </row>
  <row>
    <col_0>71774</col_0>
    <col_1>110563</col_1>
    <col_2>1</col_2>
    <col_3>822</col_3>
    <col_4>356.8980</col_4>
    <col_5>0</col_5>
    <col_6>356.8980</col_6>
    <col_7>5c77f557-fdb6-43ba-90b9-9a7aec55ca32</col_7>
    <col_8>2008-06-01 00:00:00.000</col_8>
    <col_9>2</col_9>
  </row>
</root>';

INSERT INTO [#SalesOrderDetail]
([SalesOrderID], [SalesOrderDetailID], [OrderQty], [ProductID], [UnitPrice], [UnitPriceDiscount], [LineTotal], [RowGuid], [ModifiedDate])
SELECT
    [@SalesOrderDetail].[Xml].value('col_0[1]/text()[1]', 'INT') AS [SalesOrderID],
    [@SalesOrderDetail].[Xml].value('col_1[1]/text()[1]', 'INT') AS [SalesOrderDetailID],
    [@SalesOrderDetail].[Xml].value('col_2[1]/text()[1]', 'SMALLINT') AS [OrderQty],
    [@SalesOrderDetail].[Xml].value('col_3[1]/text()[1]', 'INT') AS [ProductID],
    [@SalesOrderDetail].[Xml].value('col_4[1]/text()[1]', 'MONEY') AS [UnitPrice],
    [@SalesOrderDetail].[Xml].value('col_5[1]/text()[1]', 'MONEY') AS [UnitPriceDiscount],
    [@SalesOrderDetail].[Xml].value('col_6[1]/text()[1]', 'MONEY') AS [LineTotal],
    [@SalesOrderDetail].[Xml].value('col_7[1]/text()[1]', 'UNIQUEIDENTIFIER') AS [RowGuid],
    [@SalesOrderDetail].[Xml].value('col_8[1]/text()[1]', 'DATETIME') AS [ModifiedDate]
FROM @p1.nodes('/root/row') [@SalesOrderDetail]([Xml])
ORDER BY [@SalesOrderDetail].[Xml].value('col_9[1]/text()[1]', 'INT') ASC;
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_into_child_temp_table_do_not_optimize()
        {
            var salesOrders = DataSet<SalesOrder>.ParseJson(Json.SalesOrder_71774);
            using (var db = new Db(SqlVersion.Sql11))
            {
                var tempSalesOrders = db.MockTempTable<SalesOrder>();
                var tempSalesOrderDetails = tempSalesOrders.MockCreateChild(x => x.SalesOrderDetails);
                tempSalesOrders.MockInsert(true, salesOrders, 0);
                salesOrders._.RowGuid[0] = Guid.NewGuid();  // make some change on the dataset
                var salesOrderDetails = salesOrders.Children(x => x.SalesOrderDetails, 0);
                var command = tempSalesOrderDetails.MockInsert(salesOrderDetails.Count, salesOrderDetails);
                var expectedSql =
@"DECLARE @p1 XML = N'<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <row>
    <col_0>71774</col_0>
    <col_1>110562</col_1>
    <col_2>1</col_2>
    <col_3>836</col_3>
    <col_4>356.8980</col_4>
    <col_5>0</col_5>
    <col_6>356.8980</col_6>
    <col_7>e3a1994c-7a68-4ce8-96a3-77fdd3bbd730</col_7>
    <col_8>2008-06-01 00:00:00.000</col_8>
    <col_9>1</col_9>
  </row>
  <row>
    <col_0>71774</col_0>
    <col_1>110563</col_1>
    <col_2>1</col_2>
    <col_3>822</col_3>
    <col_4>356.8980</col_4>
    <col_5>0</col_5>
    <col_6>356.8980</col_6>
    <col_7>5c77f557-fdb6-43ba-90b9-9a7aec55ca32</col_7>
    <col_8>2008-06-01 00:00:00.000</col_8>
    <col_9>2</col_9>
  </row>
</root>';

INSERT INTO [#SalesOrderDetail]
([SalesOrderID], [SalesOrderDetailID], [OrderQty], [ProductID], [UnitPrice], [UnitPriceDiscount], [LineTotal], [RowGuid], [ModifiedDate])
SELECT
    [@SalesOrderDetail].[Xml].value('col_0[1]/text()[1]', 'INT') AS [SalesOrderID],
    [@SalesOrderDetail].[Xml].value('col_1[1]/text()[1]', 'INT') AS [SalesOrderDetailID],
    [@SalesOrderDetail].[Xml].value('col_2[1]/text()[1]', 'SMALLINT') AS [OrderQty],
    [@SalesOrderDetail].[Xml].value('col_3[1]/text()[1]', 'INT') AS [ProductID],
    [@SalesOrderDetail].[Xml].value('col_4[1]/text()[1]', 'MONEY') AS [UnitPrice],
    [@SalesOrderDetail].[Xml].value('col_5[1]/text()[1]', 'MONEY') AS [UnitPriceDiscount],
    [@SalesOrderDetail].[Xml].value('col_6[1]/text()[1]', 'MONEY') AS [LineTotal],
    [@SalesOrderDetail].[Xml].value('col_7[1]/text()[1]', 'UNIQUEIDENTIFIER') AS [RowGuid],
    [@SalesOrderDetail].[Xml].value('col_8[1]/text()[1]', 'DATETIME') AS [ModifiedDate]
FROM
    (@p1.nodes('/root/row') [@SalesOrderDetail]([Xml])
    INNER JOIN
    [#SalesOrder]
    ON [@SalesOrderDetail].[Xml].value('col_0[1]/text()[1]', 'INT') = [#SalesOrder].[SalesOrderID])
ORDER BY [@SalesOrderDetail].[Xml].value('col_9[1]/text()[1]', 'INT') ASC;
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Insert_from_temp_table_updateIdentity()
        {
            using (var db = new Db(SqlVersion.Sql11))
            {
                var sourceData = db.MockTempTable<ProductCategory>();
                var children = sourceData.MockCreateChild(x => x.SubCategories);
                var grandChildren = children.MockCreateChild(x => x.SubCategories);
                var commands = db.ProductCategory.MockInsert(10, sourceData, updateIdentity: true);

                var expectedSql = new string[]
                {
@"CREATE TABLE [#sys_identity_mapping] (
    [OldValue] INT NOT NULL,
    [NewValue] INT NULL,
    [OriginalSysRowId] INT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    CONSTRAINT [PK_sys_identity_mapping_] PRIMARY KEY NONCLUSTERED ([OldValue]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);",

@"CREATE TABLE [#IdentityOutput] (
    [NewValue] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY CLUSTERED ([sys_row_id] ASC)
);",

@"INSERT INTO [SalesLT].[ProductCategory]
([ParentProductCategoryID], [Name], [RowGuid], [ModifiedDate])
OUTPUT INSERTED.[ProductCategoryID] INTO [#IdentityOutput] ([NewValue])
SELECT
    [#ProductCategory].[ParentProductCategoryID] AS [ParentProductCategoryID],
    [#ProductCategory].[Name] AS [Name],
    [#ProductCategory].[RowGuid] AS [RowGuid],
    [#ProductCategory].[ModifiedDate] AS [ModifiedDate]
FROM [#ProductCategory]
ORDER BY [#ProductCategory].[sys_row_id] ASC;",

@"INSERT INTO [#sys_identity_mapping]
([OldValue], [OriginalSysRowId])
SELECT
    [#ProductCategory].[ProductCategoryID] AS [OldValue],
    [#ProductCategory].[sys_row_id] AS [OriginalSysRowId]
FROM [#ProductCategory]
ORDER BY [#ProductCategory].[sys_row_id] ASC;",

@"UPDATE [#sys_identity_mapping] SET
    [NewValue] = [#IdentityOutput].[NewValue]
FROM
    ([#sys_identity_mapping]
    INNER JOIN
    [#IdentityOutput]
    ON [#sys_identity_mapping].[sys_row_id] = [#IdentityOutput].[sys_row_id]);",

@"UPDATE [#ProductCategory] SET
    [ProductCategoryID] = [#sys_identity_mapping].[NewValue]
FROM
    ([#sys_identity_mapping]
    INNER JOIN
    [#ProductCategory]
    ON [#sys_identity_mapping].[OldValue] = [#ProductCategory].[ProductCategoryID]);",

@"UPDATE [#ProductCategory1] SET
    [ParentProductCategoryID] = [#sys_identity_mapping].[NewValue]
FROM
    ([#sys_identity_mapping]
    INNER JOIN
    [#ProductCategory1]
    ON [#sys_identity_mapping].[OldValue] = [#ProductCategory1].[ParentProductCategoryID]);"
            };

                commands.Verify(true, expectedSql);
            }
        }
    }
}
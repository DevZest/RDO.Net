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
        public void DbTable_Insert_from_DbTable()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var table = db.MockTempTable<Product>();
                var commands = table.MockInsert(0, db.Products);
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
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var table = db.MockTempTable<ProductCategory>();
                var command = table.MockInsert(0, db.ProductCategories.Where(x => x.ParentProductCategoryID.IsNull()));
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
        public void DbTable_Insert_from_DbQuery_auto_join()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var table = db.MockTempTable<ProductCategory>();
                var command = table.MockInsert(0, db.ProductCategories.Where(x => x.ParentProductCategoryID.IsNull()), autoJoin: true);
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
        public void DbTable_Insert_from_child_DbQuery()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var salesOrders = db.SalesOrders.Where(x => x.SalesOrderID == 71774 | x.SalesOrderID == 71776).OrderBy(x => x.SalesOrderID);
                salesOrders.MockSequentialKeyTempTable();
                var childQuery = salesOrders.CreateChild(x => x.SalesOrderDetails, db.SalesOrderDetails.OrderBy(x => x.SalesOrderDetailID));
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
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
ORDER BY [sys_sequential_SalesOrder].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
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
                var command = table.MockInsert(true, dataSet, 0, autoJoin: true);
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
        public void DbTable_Insert_union_query()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var tempTable = db.MockTempTable<Product>();
                var unionQuery = db.Products.Where(x => x.ProductID < _Int32.Const(720)).UnionAll(db.Products.Where(x => x.ProductID > _Int32.Const(800)));
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
                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }

        [TestMethod]
        public void DbTable_BuildUpdateIdentityStatement()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var tempSalesOrders = db.MockTempTable<SalesOrder>();
                var identityOutput = db.MockTempTable<IdentityMapping>();
                var statements = tempSalesOrders.BuildUpdateIdentityStatement(identityOutput);
                Assert.AreEqual(1, statements.Count);
                var command = db.GetUpdateCommand(statements[0]);
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

        [TestMethod]
        public void DbTable_Insert_from_DataSet()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(StringRes.ProductCategoriesJson);
                var tempTable = db.MockTempTable<ProductCategory>();
                var command = tempTable.MockInsert(4, db.GetDbQuery<ProductCategory, ProductCategory>(dataSet, null));

                var expectedSql =
@"DECLARE @p1 XML = N'
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
    [SqlXmlModel].[Xml].value('col_0[1]/text()[1]', 'INT') AS [ProductCategoryID],
    [SqlXmlModel].[Xml].value('col_1[1]/text()[1]', 'INT') AS [ParentProductCategoryID],
    [SqlXmlModel].[Xml].value('col_2[1]/text()[1]', 'NVARCHAR(50)') AS [Name],
    [SqlXmlModel].[Xml].value('col_3[1]/text()[1]', 'UNIQUEIDENTIFIER') AS [RowGuid],
    [SqlXmlModel].[Xml].value('col_4[1]/text()[1]', 'DATETIME') AS [ModifiedDate]
FROM @p1.nodes('/root/row') [SqlXmlModel]([Xml])
ORDER BY [SqlXmlModel].[Xml].value('col_5[1]/text()[1]', 'INT') ASC;
";

                Assert.AreEqual(expectedSql, command.ToTraceString());
            }
        }

        [TestMethod]
        public void DbTable_Insert_into_child_temp_table_optimized()
        {
            var salesOrders = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var tempSalesOrders = db.MockTempTable<SalesOrder>();
                var tempSalesOrderDetails = tempSalesOrders.MockCreateChild(x => x.SalesOrderDetails);
                tempSalesOrders.MockInsert(true, salesOrders, 0);
                var salesOrderDetails = salesOrders.Children(x => x.SalesOrderDetails, 0);
                var command = tempSalesOrderDetails.MockInsert(salesOrderDetails.Count, salesOrderDetails);
                var expectedSql =
@"DECLARE @p1 XML = N'
<root>
  <row>
    <col_0>71774</col_0>
    <col_1>110562</col_1>
    <col_2>1</col_2>
    <col_3>836</col_3>
    <col_4>356.8980</col_4>
    <col_5>0.0000</col_5>
    <col_6>356.898000</col_6>
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
    <col_5>0.0000</col_5>
    <col_6>356.898000</col_6>
    <col_7>5c77f557-fdb6-43ba-90b9-9a7aec55ca32</col_7>
    <col_8>2008-06-01 00:00:00.000</col_8>
    <col_9>2</col_9>
  </row>
</root>';

INSERT INTO [#SalesOrderDetail]
([SalesOrderID], [SalesOrderDetailID], [OrderQty], [ProductID], [UnitPrice], [UnitPriceDiscount], [LineTotal], [RowGuid], [ModifiedDate])
SELECT
    [SqlXmlModel].[Xml].value('col_0[1]/text()[1]', 'INT') AS [SalesOrderID],
    [SqlXmlModel].[Xml].value('col_1[1]/text()[1]', 'INT') AS [SalesOrderDetailID],
    [SqlXmlModel].[Xml].value('col_2[1]/text()[1]', 'SMALLINT') AS [OrderQty],
    [SqlXmlModel].[Xml].value('col_3[1]/text()[1]', 'INT') AS [ProductID],
    [SqlXmlModel].[Xml].value('col_4[1]/text()[1]', 'MONEY') AS [UnitPrice],
    [SqlXmlModel].[Xml].value('col_5[1]/text()[1]', 'MONEY') AS [UnitPriceDiscount],
    [SqlXmlModel].[Xml].value('col_6[1]/text()[1]', 'MONEY') AS [LineTotal],
    [SqlXmlModel].[Xml].value('col_7[1]/text()[1]', 'UNIQUEIDENTIFIER') AS [RowGuid],
    [SqlXmlModel].[Xml].value('col_8[1]/text()[1]', 'DATETIME') AS [ModifiedDate]
FROM @p1.nodes('/root/row') [SqlXmlModel]([Xml])
ORDER BY [SqlXmlModel].[Xml].value('col_9[1]/text()[1]', 'INT') ASC;
";
                command.Verify(expectedSql);
            }
        }
    }
}

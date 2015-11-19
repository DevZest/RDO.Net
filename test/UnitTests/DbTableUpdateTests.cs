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
        public void DbTable_Update_without_source()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var command = db.ProductCategories.MockUpdate(0, (builder, x) => builder.Select(Functions.GetDate(), x.ModifiedDate),
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
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var tempTable = db.MockTempTable<ProductCategory>();
                var command = db.ProductCategories.MockUpdate(0, tempTable);
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
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Update_from_query()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var query = db.ProductCategories.Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategories.MockUpdate(0, query);
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
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var query = db.MockTempTable<ProductCategory>().Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategories.MockUpdate(0, query);
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
                command.Verify(expectedSql);
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
                var command = db.ProductCategories.MockUpdate(true, dataSet, 0);
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
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Update_from_DataSet()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(StringRes.ProductCategoriesLevel1Json);
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
    <col_2>Other</col_2>
    <col_3>c657828d-d808-4aba-91a3-af2ce02300e9</col_3>
    <col_4>2002-06-01 00:00:00.000</col_4>
    <col_5>2</col_5>
  </row>
</root>';

UPDATE [ProductCategory] SET
    [ParentProductCategoryID] = [SqlXmlModel].[Xml].value('col_1[1]/text()[1]', 'INT'),
    [Name] = [SqlXmlModel].[Xml].value('col_2[1]/text()[1]', 'NVARCHAR(50)'),
    [RowGuid] = [SqlXmlModel].[Xml].value('col_3[1]/text()[1]', 'UNIQUEIDENTIFIER'),
    [ModifiedDate] = [SqlXmlModel].[Xml].value('col_4[1]/text()[1]', 'DATETIME')
FROM
    (@p1.nodes('/root/row') [SqlXmlModel]([Xml])
    INNER JOIN
    [SalesLT].[ProductCategory] [ProductCategory]
    ON [SqlXmlModel].[Xml].value('col_0[1]/text()[1]', 'INT') = [ProductCategory].[ProductCategoryID]);
";
                var command = db.ProductCategories.MockUpdate(dataSet.Count, dataSet);
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Update_child_temp_table()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var salesOrders = db.MockTempTable<SalesOrder>();
                var salesOrderDetails = salesOrders.MockCreateChild(x => x.SalesOrderDetails);

                var source = db.MockTempTable<SalesOrderDetail>();

                var command = salesOrderDetails.MockUpdate(0, source);
                var expectedSql =
@"UPDATE [SalesOrderDetail1] SET
    [SalesOrderDetailID] = [SalesOrderDetail].[SalesOrderDetailID],
    [OrderQty] = [SalesOrderDetail].[OrderQty],
    [ProductID] = [SalesOrderDetail].[ProductID],
    [UnitPrice] = [SalesOrderDetail].[UnitPrice],
    [UnitPriceDiscount] = [SalesOrderDetail].[UnitPriceDiscount],
    [LineTotal] = [SalesOrderDetail].[LineTotal],
    [RowGuid] = [SalesOrderDetail].[RowGuid],
    [ModifiedDate] = [SalesOrderDetail].[ModifiedDate]
FROM
    ([#SalesOrderDetail1] [SalesOrderDetail]
    INNER JOIN
    [#SalesOrderDetail] [SalesOrderDetail1]
    ON [SalesOrderDetail].[SalesOrderID] = [SalesOrderDetail1].[SalesOrderID] AND [SalesOrderDetail].[SalesOrderDetailID] = [SalesOrderDetail1].[SalesOrderDetailID]);
";
                command.Verify(expectedSql);
            }
        }
    }
}

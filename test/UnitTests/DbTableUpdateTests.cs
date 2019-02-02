using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.MySql.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DevZest.Data.MySql.Resources;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class DbTableUpdateTests
    {
        [TestMethod]
        public void DbTable_Update_without_source()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var command = db.ProductCategory.MockUpdate(0, (builder, x) => builder.Select(_DateTime.Now(), x.ModifiedDate),
                    x => x.ModifiedDate.IsNull());
                var expectedSql =
@"UPDATE `ProductCategory`
SET
    `ProductCategory`.`ModifiedDate` = NOW()
WHERE (`ProductCategory`.`ModifiedDate` IS NULL);
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Update_from_temp_table()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var tempTable = db.MockTempTable<ProductCategory>();
                var command = db.ProductCategory.MockUpdate(0, tempTable);
                var expectedSql =
@"UPDATE
    (`#ProductCategory`
    INNER JOIN
    `ProductCategory`
    ON `#ProductCategory`.`ProductCategoryID` = `ProductCategory`.`ProductCategoryID`)
SET
    `ProductCategory`.`ParentProductCategoryID` = `#ProductCategory`.`ParentProductCategoryID`,
    `ProductCategory`.`Name` = `#ProductCategory`.`Name`,
    `ProductCategory`.`RowGuid` = `#ProductCategory`.`RowGuid`,
    `ProductCategory`.`ModifiedDate` = `#ProductCategory`.`ModifiedDate`;
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Update_from_simple_query()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var query = db.MockTempTable<ProductCategory>().Where(x => x.ModifiedDate.IsNull());
                var command = db.ProductCategory.MockUpdate(0, query);
                var expectedSql =
@"UPDATE
    (`#ProductCategory`
    INNER JOIN
    `ProductCategory`
    ON `#ProductCategory`.`ProductCategoryID` = `ProductCategory`.`ProductCategoryID`)
SET
    `ProductCategory`.`ParentProductCategoryID` = `#ProductCategory`.`ParentProductCategoryID`,
    `ProductCategory`.`Name` = `#ProductCategory`.`Name`,
    `ProductCategory`.`RowGuid` = `#ProductCategory`.`RowGuid`,
    `ProductCategory`.`ModifiedDate` = `#ProductCategory`.`ModifiedDate`
WHERE (`#ProductCategory`.`ModifiedDate` IS NULL);
";
                command.Verify(expectedSql);
            }
        }

        //        [TestMethod]
        //        public void DbTable_Update_scalar()
        //        {
        //            using (var db = new Db(SqlVersion.Sql11))
        //            {
        //                var dataSet = DataSet<ProductCategory>.Create();
        //                var index = dataSet.AddRow().Ordinal;
        //                dataSet._.Name[index] = "Name";
        //                dataSet._.RowGuid[index] = new Guid("EC359D7D-AE3A-4A9D-BDCB-03F0A7799514");
        //                dataSet._.ModifiedDate[index] = new DateTime(2015, 9, 23);
        //                var command = db.ProductCategory.MockUpdate(true, dataSet, 0);
        //                var expectedSql =
        //@"DECLARE @p1 INT = NULL;
        //DECLARE @p2 NVARCHAR(50) = N'Name';
        //DECLARE @p3 UNIQUEIDENTIFIER = 'ec359d7d-ae3a-4a9d-bdcb-03f0a7799514';
        //DECLARE @p4 DATETIME = '2015-09-23 00:00:00.000';
        //DECLARE @p5 INT = 0;

        //UPDATE [ProductCategory1] SET
        //    [ParentProductCategoryID] = @p1,
        //    [Name] = @p2,
        //    [RowGuid] = @p3,
        //    [ModifiedDate] = @p4
        //FROM
        //    ((SELECT @p5 AS [ProductCategoryID]) [ProductCategory]
        //    INNER JOIN
        //    [SalesLT].[ProductCategory] [ProductCategory1]
        //    ON [ProductCategory].[ProductCategoryID] = [ProductCategory1].[ProductCategoryID]);
        //";
        //                command.Verify(expectedSql);
        //            }
        //        }

        //        [TestMethod]
        //        public void DbTable_Update_from_DataSet()
        //        {
        //            using (var db = new Db(SqlVersion.Sql11))
        //            {
        //                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategoriesLevel1);
        //                var expectedSql =
        //@"DECLARE @p1 XML = N'<?xml version=""1.0"" encoding=""utf-8""?>
        //<root>
        //  <row>
        //    <col_0>1</col_0>
        //    <col_1></col_1>
        //    <col_2>Bikes</col_2>
        //    <col_3>cfbda25c-df71-47a7-b81b-64ee161aa37c</col_3>
        //    <col_4>2002-06-01 00:00:00.000</col_4>
        //    <col_5>1</col_5>
        //  </row>
        //  <row>
        //    <col_0>2</col_0>
        //    <col_1></col_1>
        //    <col_2>Other</col_2>
        //    <col_3>c657828d-d808-4aba-91a3-af2ce02300e9</col_3>
        //    <col_4>2002-06-01 00:00:00.000</col_4>
        //    <col_5>2</col_5>
        //  </row>
        //</root>';

        //UPDATE [ProductCategory] SET
        //    [ParentProductCategoryID] = [@ProductCategory].[Xml].value('col_1[1]/text()[1]', 'INT'),
        //    [Name] = [@ProductCategory].[Xml].value('col_2[1]/text()[1]', 'NVARCHAR(50)'),
        //    [RowGuid] = [@ProductCategory].[Xml].value('col_3[1]/text()[1]', 'UNIQUEIDENTIFIER'),
        //    [ModifiedDate] = [@ProductCategory].[Xml].value('col_4[1]/text()[1]', 'DATETIME')
        //FROM
        //    (@p1.nodes('/root/row') [@ProductCategory]([Xml])
        //    INNER JOIN
        //    [SalesLT].[ProductCategory] [ProductCategory]
        //    ON [@ProductCategory].[Xml].value('col_0[1]/text()[1]', 'INT') = [ProductCategory].[ProductCategoryID]);
        //";
        //                var command = db.ProductCategory.MockUpdate(dataSet.Count, dataSet);
        //                command.Verify(expectedSql);
        //            }
        //        }

        //        [TestMethod]
        //        public void DbTable_Update_child_temp_table()
        //        {
        //            using (var db = new Db(SqlVersion.Sql11))
        //            {
        //                var salesOrders = db.MockTempTable<SalesOrder>();
        //                var salesOrderDetails = salesOrders.MockCreateChild(x => x.SalesOrderDetails);

        //                var source = db.MockTempTable<SalesOrderDetail>();

        //                var command = salesOrderDetails.MockUpdate(0, source);
        //                var expectedSql =
        //@"UPDATE [SalesOrderDetail1] SET
        //    [SalesOrderDetailID] = [SalesOrderDetail].[SalesOrderDetailID],
        //    [OrderQty] = [SalesOrderDetail].[OrderQty],
        //    [ProductID] = [SalesOrderDetail].[ProductID],
        //    [UnitPrice] = [SalesOrderDetail].[UnitPrice],
        //    [UnitPriceDiscount] = [SalesOrderDetail].[UnitPriceDiscount],
        //    [LineTotal] = [SalesOrderDetail].[LineTotal],
        //    [RowGuid] = [SalesOrderDetail].[RowGuid],
        //    [ModifiedDate] = [SalesOrderDetail].[ModifiedDate]
        //FROM
        //    ([#SalesOrderDetail1] [SalesOrderDetail]
        //    INNER JOIN
        //    [#SalesOrderDetail] [SalesOrderDetail1]
        //    ON [SalesOrderDetail].[SalesOrderID] = [SalesOrderDetail1].[SalesOrderID] AND [SalesOrderDetail].[SalesOrderDetailID] = [SalesOrderDetail1].[SalesOrderDetailID]);
        //";
        //                command.Verify(expectedSql);
        //            }
        //        }
    }
}

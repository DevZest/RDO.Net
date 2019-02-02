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

        [TestMethod]
        public void DbTable_Update_scalar()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var dataSet = DataSet<ProductCategory>.Create();
                var index = dataSet.AddRow().Ordinal;
                dataSet._.Name[index] = "Name";
                dataSet._.RowGuid[index] = new Guid("EC359D7D-AE3A-4A9D-BDCB-03F0A7799514");
                dataSet._.ModifiedDate[index] = new DateTime(2015, 9, 23);
                var command = db.ProductCategory.MockUpdate(true, dataSet, 0);
                var expectedSql =
@"SET @p1 = 0;
SET @p2 = NULL;
SET @p3 = 'Name';
SET @p4 = 'ec359d7d-ae3a-4a9d-bdcb-03f0a7799514';
SET @p5 = (TIMESTAMP '2015-09-23 00:00:00');

UPDATE
    ((SELECT @p1 AS `ProductCategoryID`) `@ProductCategory`
    INNER JOIN
    `ProductCategory`
    ON `@ProductCategory`.`ProductCategoryID` = `ProductCategory`.`ProductCategoryID`)
SET
    `ProductCategory`.`ParentProductCategoryID` = @p2,
    `ProductCategory`.`Name` = @p3,
    `ProductCategory`.`RowGuid` = @p4,
    `ProductCategory`.`ModifiedDate` = @p5;
";
                command.Verify(expectedSql);
            }
        }

        [TestMethod]
        public void DbTable_Update_from_DataSet()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategoriesLevel1);
                var expectedSql =
@"SET @p1 = '[{""ProductCategoryID"":1,""Name"":""Bikes"",""RowGuid"":""cfbda25c-df71-47a7-b81b-64ee161aa37c"",""ModifiedDate"":""2002-06-01T00:00:00.000""},{""ProductCategoryID"":2,""Name"":""Other"",""RowGuid"":""c657828d-d808-4aba-91a3-af2ce02300e9"",""ModifiedDate"":""2002-06-01T00:00:00.000""}]';

UPDATE
    (JSON_TABLE(@p1, '$[*]' COLUMNS (
        `ProductCategoryID` INT PATH '$.ProductCategoryID',
        `ParentProductCategoryID` INT PATH '$.ParentProductCategoryID',
        `Name` VARCHAR(50) PATH '$.Name',
        `RowGuid` CHAR(36) PATH '$.RowGuid',
        `ModifiedDate` DATETIME PATH '$.ModifiedDate',
        `sys_dataset_ordinal` FOR ORDINALITY)) AS `@ProductCategory`
    INNER JOIN
    `ProductCategory`
    ON `@ProductCategory`.`ProductCategoryID` = `ProductCategory`.`ProductCategoryID`)
SET
    `ProductCategory`.`ParentProductCategoryID` = `@ProductCategory`.`ParentProductCategoryID`,
    `ProductCategory`.`Name` = `@ProductCategory`.`Name`,
    `ProductCategory`.`RowGuid` = `@ProductCategory`.`RowGuid`,
    `ProductCategory`.`ModifiedDate` = `@ProductCategory`.`ModifiedDate`;
";
                var command = db.ProductCategory.MockUpdate(dataSet.Count, dataSet);
                command.Verify(expectedSql);
            }
        }
    }
}

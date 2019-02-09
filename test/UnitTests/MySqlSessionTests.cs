using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Data.MySql.Resources;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class MySqlSessionTests
    {
        [TestMethod]
        public void MySqlSession_BuildQuery()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategories);
                var query = db.BuildQuery(dataSet, null, (Data.ColumnMapper builder, ProductCategory source, Adhoc target) =>
                {
                    builder.Select(source.Name, target.AddColumn(source.Name, initializer: x => x.DbColumnName = source.Name.DbColumnName));
                });
                var expectedSql =
@"SET @p1 = '[{""ProductCategoryID"":1,""Name"":""Bikes"",""RowGuid"":""cfbda25c-df71-47a7-b81b-64ee161aa37c"",""ModifiedDate"":""2002-06-01T00:00:00""},{""ProductCategoryID"":2,""Name"":""Components"",""RowGuid"":""c657828d-d808-4aba-91a3-af2ce02300e9"",""ModifiedDate"":""2002-06-01T00:00:00""},{""ProductCategoryID"":3,""Name"":""Clothing"",""RowGuid"":""10a7c342-ca82-48d4-8a38-46a2eb089b74"",""ModifiedDate"":""2002-06-01T00:00:00""},{""ProductCategoryID"":4,""Name"":""Accessories"",""RowGuid"":""2be3be36-d9a2-4eee-b593-ed895d97c2a6"",""ModifiedDate"":""2002-06-01T00:00:00""}]';

SELECT `@ProductCategory`.`Name` AS `Name`
FROM JSON_TABLE(@p1, '$[*]' COLUMNS (
    `ProductCategoryID` INT PATH '$.ProductCategoryID',
    `ParentProductCategoryID` INT PATH '$.ParentProductCategoryID',
    `Name` VARCHAR(50) PATH '$.Name',
    `RowGuid` CHAR(36) PATH '$.RowGuid',
    `ModifiedDate` DATETIME PATH '$.ModifiedDate',
    `sys_dataset_ordinal` FOR ORDINALITY)) AS `@ProductCategory`
ORDER BY `@ProductCategory`.`sys_dataset_ordinal` ASC;
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }

        [TestMethod]
        public void MySqlSession_BuildImportQuery()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategories);
                var query = db.BuildImportQuery(dataSet);
                var expectedSql =
@"SET @p1 = '[{""ProductCategoryID"":1,""Name"":""Bikes"",""RowGuid"":""cfbda25c-df71-47a7-b81b-64ee161aa37c"",""ModifiedDate"":""2002-06-01T00:00:00""},{""ProductCategoryID"":2,""Name"":""Components"",""RowGuid"":""c657828d-d808-4aba-91a3-af2ce02300e9"",""ModifiedDate"":""2002-06-01T00:00:00""},{""ProductCategoryID"":3,""Name"":""Clothing"",""RowGuid"":""10a7c342-ca82-48d4-8a38-46a2eb089b74"",""ModifiedDate"":""2002-06-01T00:00:00""},{""ProductCategoryID"":4,""Name"":""Accessories"",""RowGuid"":""2be3be36-d9a2-4eee-b593-ed895d97c2a6"",""ModifiedDate"":""2002-06-01T00:00:00""}]';

SELECT
    `@ProductCategory`.`ProductCategoryID` AS `ProductCategoryID`,
    `@ProductCategory`.`ParentProductCategoryID` AS `ParentProductCategoryID`,
    `@ProductCategory`.`Name` AS `Name`,
    `@ProductCategory`.`RowGuid` AS `RowGuid`,
    `@ProductCategory`.`ModifiedDate` AS `ModifiedDate`
FROM JSON_TABLE(@p1, '$[*]' COLUMNS (
    `ProductCategoryID` INT PATH '$.ProductCategoryID',
    `ParentProductCategoryID` INT PATH '$.ParentProductCategoryID',
    `Name` VARCHAR(50) PATH '$.Name',
    `RowGuid` CHAR(36) PATH '$.RowGuid',
    `ModifiedDate` DATETIME PATH '$.ModifiedDate',
    `sys_dataset_ordinal` FOR ORDINALITY)) AS `@ProductCategory`
ORDER BY `@ProductCategory`.`sys_dataset_ordinal` ASC;
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }

        [TestMethod]
        public void MySqlSession_BuildImportKeyQuery()
        {
            using (var db = new Db(MySqlVersion.LowestSupported))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(Json.ProductCategories);
                var query = db.BuildImportKeyQuery(dataSet);
                var expectedSql =
@"SET @p1 = '[{""ProductCategoryID"":1,""Name"":""Bikes"",""RowGuid"":""cfbda25c-df71-47a7-b81b-64ee161aa37c"",""ModifiedDate"":""2002-06-01T00:00:00""},{""ProductCategoryID"":2,""Name"":""Components"",""RowGuid"":""c657828d-d808-4aba-91a3-af2ce02300e9"",""ModifiedDate"":""2002-06-01T00:00:00""},{""ProductCategoryID"":3,""Name"":""Clothing"",""RowGuid"":""10a7c342-ca82-48d4-8a38-46a2eb089b74"",""ModifiedDate"":""2002-06-01T00:00:00""},{""ProductCategoryID"":4,""Name"":""Accessories"",""RowGuid"":""2be3be36-d9a2-4eee-b593-ed895d97c2a6"",""ModifiedDate"":""2002-06-01T00:00:00""}]';

SELECT `@ProductCategory`.`ProductCategoryID` AS `ProductCategoryID`
FROM JSON_TABLE(@p1, '$[*]' COLUMNS (
    `ProductCategoryID` INT PATH '$.ProductCategoryID',
    `ParentProductCategoryID` INT PATH '$.ParentProductCategoryID',
    `Name` VARCHAR(50) PATH '$.Name',
    `RowGuid` CHAR(36) PATH '$.RowGuid',
    `ModifiedDate` DATETIME PATH '$.ModifiedDate',
    `sys_dataset_ordinal` FOR ORDINALITY)) AS `@ProductCategory`
ORDER BY `@ProductCategory`.`sys_dataset_ordinal` ASC;
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }
    }
}

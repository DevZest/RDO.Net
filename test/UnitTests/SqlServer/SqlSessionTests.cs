using DevZest.Samples.AdventureWorksLT;
using DevZest.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Data.Primitives;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class SqlSessionTests
    {
        [TestMethod]
        public void SqlSession_GetDbQuery_from_DataSet()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(StringRes.ProductCategoriesJson);
                var query = db.GetDbQuery<ProductCategory, ProductCategory>(dataSet, null);
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

SELECT
    [SqlXmlModel].[Xml].value('col_0[1]/text()[1]', 'INT') AS [ProductCategoryID],
    [SqlXmlModel].[Xml].value('col_1[1]/text()[1]', 'INT') AS [ParentProductCategoryID],
    [SqlXmlModel].[Xml].value('col_2[1]/text()[1]', 'NVARCHAR(50)') AS [Name],
    [SqlXmlModel].[Xml].value('col_3[1]/text()[1]', 'UNIQUEIDENTIFIER') AS [RowGuid],
    [SqlXmlModel].[Xml].value('col_4[1]/text()[1]', 'DATETIME') AS [ModifiedDate]
FROM @p1.nodes('/root/row') [SqlXmlModel]([Xml])
ORDER BY [SqlXmlModel].[Xml].value('col_5[1]/text()[1]', 'INT') ASC;
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }

        [TestMethod]
        public void SqlSession_Insert_DbTable_with_identity_mappings()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var sourceData = db.MockTempTable<ProductCategory>();
                var identityMappings = db.MockTempTable<IdentityMapping>();
                var identityOutput = db.MockTempTable<IdentityOutput>();

                {
                    var statement = db.ProductCategories.BuildInsertStatement(sourceData, null, false);
                    var insertCommand = db.GetInsertCommand(statement);
                    var expectedSql =
@"INSERT INTO [SalesLT].[ProductCategory]
([ParentProductCategoryID], [Name], [RowGuid], [ModifiedDate])
SELECT
    [ProductCategory].[ParentProductCategoryID] AS [ParentProductCategoryID],
    [ProductCategory].[Name] AS [Name],
    [ProductCategory].[RowGuid] AS [RowGuid],
    [ProductCategory].[ModifiedDate] AS [ModifiedDate]
FROM [#ProductCategory] [ProductCategory]
ORDER BY [ProductCategory].[sys_row_id] ASC;
";
                    Assert.AreEqual(expectedSql, insertCommand.ToTraceString());
                }

                {
                    var insertIntoIdentityMappingsCommand = db.GetInsertIntoIdentityMappingsCommand<ProductCategory, ProductCategory>(sourceData, identityMappings, null);
                    var expectedSql =
@"INSERT INTO [#sys_identity_mapping]
([OldValue], [OriginalSysRowId])
SELECT
    [ProductCategory].[ProductCategoryID] AS [OldValue],
    [ProductCategory].[sys_row_id] AS [OriginalSysRowId]
FROM [#ProductCategory] [ProductCategory]
ORDER BY [ProductCategory].[sys_row_id] ASC;
";
                    Assert.AreEqual(expectedSql, insertIntoIdentityMappingsCommand.ToTraceString());
                }

                {
                    var insertIntoIdentityMappingsCommand = db.GetInsertIntoIdentityMappingsCommand<ProductCategory, ProductCategory>(sourceData, identityMappings, db.ProductCategories);
                    var expectedSql =
@"INSERT INTO [#sys_identity_mapping]
([OldValue], [OriginalSysRowId])
SELECT
    [ProductCategory].[ProductCategoryID] AS [OldValue],
    [ProductCategory].[sys_row_id] AS [OriginalSysRowId]
FROM
    ([#ProductCategory] [ProductCategory]
    LEFT JOIN
    [SalesLT].[ProductCategory] [ProductCategory1]
    ON [ProductCategory].[ProductCategoryID] = [ProductCategory1].[ProductCategoryID])
WHERE ([ProductCategory1].[ProductCategoryID] IS NULL)
ORDER BY [ProductCategory].[sys_row_id] ASC;
";
                    Assert.AreEqual(expectedSql, insertIntoIdentityMappingsCommand.ToTraceString());
                }

                {
                    var updateIdentityMappingsCommand = db.GetUpdateIdentityMappingsCommand(identityMappings, identityOutput);
                    var expectedSql =
@"UPDATE [sys_identity_mapping] SET
    [NewValue] = [IdentityOutput].[NewValue]
FROM
    ([#sys_identity_mapping] [sys_identity_mapping]
    INNER JOIN
    [#IdentityOutput] [IdentityOutput]
    ON [sys_identity_mapping].[sys_row_id] = [IdentityOutput].[sys_row_id]);
";
                    Assert.AreEqual(expectedSql, updateIdentityMappingsCommand.ToTraceString());

                }
            }
        }

        [TestMethod]
        public void SqlSession_GetDbQuery_from_DataSet_custom_column_mapping()
        {
            using (var db = Db.Create(SqlVersion.Sql11))
            {
                var dataSet = DataSet<ProductCategory>.ParseJson(StringRes.ProductCategoriesJson);
                var query = db.GetDbQuery(dataSet, (ColumnMappingsBuilder builder, ProductCategory source, Adhoc target) =>
                {
                    builder.Select(source.Name, target.AddColumn(source.Name, initializer: x => x.DbColumnName = source.Name.DbColumnName));
                });
                var expectedSql =
@"DECLARE @p1 XML = N'
<root>
  <row>
    <col_0>Bikes</col_0>
    <col_1>1</col_1>
  </row>
  <row>
    <col_0>Components</col_0>
    <col_1>2</col_1>
  </row>
  <row>
    <col_0>Clothing</col_0>
    <col_1>3</col_1>
  </row>
  <row>
    <col_0>Accessories</col_0>
    <col_1>4</col_1>
  </row>
</root>';

SELECT [SqlXmlModel].[Xml].value('col_0[1]/text()[1]', 'NVARCHAR(50)') AS [Name]
FROM @p1.nodes('/root/row') [SqlXmlModel]([Xml])
ORDER BY [SqlXmlModel].[Xml].value('col_1[1]/text()[1]', 'INT') ASC;
";
                Assert.AreEqual(expectedSql, query.ToString());
            }
        }
    }
}

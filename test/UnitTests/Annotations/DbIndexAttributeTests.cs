using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class DbIndexAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Id);
                RegisterColumn((TestModel _) => _.Value);
            }

            [DbIndex("IDX_ID", IsUnique = true)]
            public _Int32 Id { get; private set; }

            [DbIndex("IDX_VALUE", SortDirection = SortDirection.Descending)]
            public _Int32 Value { get; private set; }
        }

        private sealed class TestDb : SqlSession
        {
            public TestDb(SqlVersion sqlVersion)
                : base(new SqlConnection())
            {
                SqlVersion = sqlVersion;
            }

            private DbTable<TestModel> _testTable;
            public DbTable<TestModel> TestTable
            {
                get { return GetTable(ref _testTable, nameof(TestTable)); }
            }
        }

        [TestMethod]
        public void DbIndexAttribute_sql_generation()
        {
            using (var testDb = new TestDb(SqlVersion.Sql11))
            {
                var command = testDb.GetCreateTableCommand(new TestModel(), nameof(TestDb.TestTable), false);
                var expectedSql =
@"CREATE TABLE [TestTable] (
    [Id] INT NULL,
    [Value] INT NULL

    INDEX [IDX_ID] UNIQUE NONCLUSTERED ([Id] ASC),
    INDEX [IDX_VALUE] NONCLUSTERED ([Value] DESC)
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }
    }
}

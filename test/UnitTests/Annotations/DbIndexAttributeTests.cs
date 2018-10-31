using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class DbIndexAttributeTests
    {
        [DbIndex(nameof(IDX_ID), IsUnique = true)]
        [DbIndex(nameof(IDX_ID_VALUE))]
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Id);
                RegisterColumn((TestModel _) => _.Value);
            }

            public _Int32 Id { get; private set; }

            public _Int32 Value { get; private set; }

            private ColumnSort[] IDX_ID => new ColumnSort[] { Id.Asc() };

            private ColumnSort[] IDX_ID_VALUE => new ColumnSort[] { Id, Value.Desc() };
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
                var command = testDb.GetCreateTableCommand(testDb.TestTable._, nameof(TestDb.TestTable), null, false);
                var expectedSql =
@"CREATE TABLE [TestTable] (
    [Id] INT NULL,
    [Value] INT NULL

    INDEX [IDX_ID] UNIQUE NONCLUSTERED ([Id] ASC),
    INDEX [IDX_ID_VALUE] NONCLUSTERED ([Id], [Value] DESC)
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }
    }
}

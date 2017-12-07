using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class DbCompositeIndexAttributeTests
    {
        [DbCompositeIndex(IDX_ID, IsUnique = true)]
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Id1);
                RegisterColumn((TestModel _) => _.Id2);
            }

            private const string IDX_ID = nameof(IDX_ID);

            [DbIndexMember(IDX_ID, Order = 2)]
            public _Int32 Id1 { get; private set; }

            [DbIndexMember(IDX_ID, Order = 1, SortDirection = SortDirection.Descending)]
            public _Int32 Id2 { get; private set; }
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
        public void UniqueColumnsAttribute_sql_generation()
        {
            using (var testDb = new TestDb(SqlVersion.Sql11))
            {
                var command = testDb.GetCreateTableCommand(testDb.TestTable._, nameof(TestDb.TestTable), null, false);
                var expectedSql =
@"CREATE TABLE [TestTable] (
    [Id1] INT NULL,
    [Id2] INT NULL

    INDEX [IDX_ID] UNIQUE NONCLUSTERED ([Id2] DESC, [Id1] ASC)
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }
    }
}

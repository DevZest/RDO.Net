using DevZest.Data.Primitives;
using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class DbColumnNameAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.ID);
            }

            [DbColumn("DB_ID")]
            public _Int32 ID { get; private set; }
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
                get { return GetTable(ref _testTable); }
            }
        }

        [TestMethod]
        public void DbColumnNameAttribute()
        {
            using (var testDb = new TestDb(SqlVersion.Sql11))
            {
                var command = testDb.GetCreateTableCommand(testDb.TestTable._, false);
                var expectedSql =
@"CREATE TABLE [TestTable] (
    [DB_ID] INT NULL
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }
    }
}

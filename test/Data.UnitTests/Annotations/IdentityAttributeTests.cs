using DevZest.Data.Primitives;
using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class IdentityAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Id);
            }

            [Identity(1, 1)]
            public _Int32 Id { get; private set; }
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
        public void IdentityAttribute_sql_generation()
        {
            using (var testDb = new TestDb(SqlVersion.Sql13))
            {
                var command = testDb.GetCreateTableCommand(testDb.TestTable._, false);
                var expectedSql =
@"CREATE TABLE [TestTable] (
    [Id] INT NOT NULL IDENTITY(1, 1)
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }

        [TestMethod]
        public void IdentityAttribute_data_set()
        {
            var dataSet = DataSet<TestModel>.Create();
            var dataRow = dataSet.AddRow();
            Assert.AreEqual(0, dataSet._.Id[dataRow]);
        }
    }
}

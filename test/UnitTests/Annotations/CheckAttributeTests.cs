using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class CheckAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Id);
            }

            public _Int32 Id { get; private set; }

            [Check("ERR_Check", Name = nameof(CK_TestModel_Id))]
            private _Boolean CK_TestModel_Id
            {
                get { return Id > _Int32.Const(0); }
            }
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
        public void CheckAttribute_sql_generation()
        {
            using (var testDb = new TestDb(SqlVersion.Sql11))
            {
                var command = testDb.GetCreateTableCommand(testDb.TestTable.Model, nameof(TestDb.TestTable), null, false);
                var expectedSql =
@"CREATE TABLE [TestTable] (
    [Id] INT NULL

    CONSTRAINT [CK_TestModel_Id] CHECK ([Id] > 0)
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }

        [TestMethod]
        public void CheckAttribute_data_set()
        {
            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Id[row] = 1);
                Assert.AreEqual(0, dataSet._.Validate(dataRow, ValidationSeverity.Error).Count);
            }

            {
                var dataSet = DataSet<TestModel>.New();
                var dataRow = dataSet.AddRow((_, row) => _.Id[row] = 0);
                var messages = dataSet._.Validate(dataRow, ValidationSeverity.Error);
                Assert.AreEqual(1, messages.Count);
                Assert.AreEqual("ERR_Check", messages[0].Description);
            }
        }
    }
}

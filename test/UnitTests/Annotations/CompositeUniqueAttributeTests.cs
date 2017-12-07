using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class CompositeUniqueAttributeTests
    {
        [CompositeUnique(UNIQUE_ID, MessageId = "ERR_DuplicateIds")]
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Id1);
                RegisterColumn((TestModel _) => _.Id2);
            }

            private const string UNIQUE_ID = nameof(UNIQUE_ID);

            [UniqueMember(UNIQUE_ID, Order = 2)]
            public _Int32 Id1 { get; private set; }

            [UniqueMember(UNIQUE_ID, Order = 1, SortDirection = SortDirection.Descending)]
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
        public void CompositeUniqueAttribute_sql_generation()
        {
            using (var testDb = new TestDb(SqlVersion.Sql11))
            {
                var command = testDb.GetCreateTableCommand(testDb.TestTable._, nameof(TestDb.TestTable), null, false);
                var expectedSql =
@"CREATE TABLE [TestTable] (
    [Id1] INT NULL,
    [Id2] INT NULL

    CONSTRAINT [UNIQUE_ID] UNIQUE NONCLUSTERED ([Id2] DESC, [Id1] ASC)
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }

        [TestMethod]
        public void CompositeUniqueAttribute_data_set()
        {
            var dataSet = DataSet<TestModel>.New();
            var dataRow1 = dataSet.AddRow((_, row) => {
                _.Id1[row] = 1;
                _.Id2[row] = 1;
                });
            Assert.AreEqual(0, dataSet._.Validate(dataRow1, ValidationSeverity.Error).Count);
            var dataRow2 = dataSet.AddRow((_, row) => {
                _.Id1[row] = 1;
                _.Id2[row] = 1;
            });
            var messages1 = dataSet._.Validate(dataRow1, ValidationSeverity.Error);
            var messages2 = dataSet._.Validate(dataRow2, ValidationSeverity.Error);
            Assert.AreEqual(1, messages1.Count);
            Assert.AreEqual("ERR_DuplicateIds", messages1[0].Id);
            Assert.AreEqual(1, messages2.Count);
            Assert.AreEqual("ERR_DuplicateIds", messages2[0].Id);

        }
    }
}

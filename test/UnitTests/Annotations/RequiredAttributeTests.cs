using DevZest.Data.Primitives;
using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.Globalization;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class RequiredAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Id);
            }

            [Required]
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
        public void RequiredAttribute_sql_generation()
        {
            using (var testDb = new TestDb(SqlVersion.Sql11))
            {
                var command = testDb.GetCreateTableCommand(testDb.TestTable._, false);
                var expectedSql =
@"CREATE TABLE [TestTable] (
    [Id] INT NOT NULL
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }

        [TestMethod]
        public void RequiredAttribute_data_set()
        {
            var dataSet = DataSet<TestModel>.Create();
            var dataRow1 = dataSet.AddRow((_, row) => _.Id[row] = 1);
            Assert.AreEqual(0, dataSet._.Validate(dataRow1).Count);
            var dataRow2 = dataSet.AddRow();
            var messages2 = dataSet._.Validate(dataRow2);
            Assert.AreEqual(1, messages2.Count);
            Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, UserMessages.RequiredAttribute, nameof(TestModel.Id)), messages2[0].Message);
        }
    }
}

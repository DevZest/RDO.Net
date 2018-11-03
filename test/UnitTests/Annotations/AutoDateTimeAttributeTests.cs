using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class AutoDateTimeAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.DateTime);
            }

            [AutoDateTime]
            [SqlDateTime]
            public _DateTime DateTime { get; private set; }
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
        public void AutoDateTimeAttribute_sql_generation()
        {
            using (var testDb = new TestDb(SqlVersion.Sql11))
            {
                var command = testDb.GetCreateTableCommand(new TestModel(), nameof(TestDb.TestTable), null, false);
                var expectedSql =
@"CREATE TABLE [TestTable] (
    [DateTime] DATETIME NULL DEFAULT(GETDATE())
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }

        [TestMethod]
        public void AutoDateTimeAttribute_default_value()
        {
            var dataSet = DataSet<TestModel>.New();
            var startDateTime = DateTime.Now;
            var dataRow = dataSet.AddRow();
            var endDateTime = DateTime.Now;
            var dateTime = dataSet._.DateTime[dataRow];
            Assert.IsTrue(dateTime.HasValue);
            var value = dateTime.Value;
            Assert.IsTrue(value >= startDateTime && value <= endDateTime);
        }
    }
}

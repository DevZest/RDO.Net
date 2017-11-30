using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class DefaultValueAttributeTests
    {
        private enum WeekDay
        {
            Sun = 0,
            Mon,
            Tue,
            Wed,
            Thu,
            Fri,
            Sat
        }

        private enum Status
        {
            Active = 'A',
            Inactive = 'I'
        }

        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Int);
                RegisterColumn((TestModel _) => _.WeekDay);
                RegisterColumn((TestModel _) => _.Status);
            }

            [DefaultValue(5)]
            public _Int32 Int { get; private set; }

            [DefaultValue(typeof(WeekDay), nameof(DefaultValueAttributeTests.WeekDay.Mon))]
            public _ByteEnum<WeekDay> WeekDay { get; private set; }

            [DefaultValue(typeof(Status), nameof(DefaultValueAttributeTests.Status.Inactive))]
            public _CharEnum<Status> Status { get; private set; }
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
        public void DefaultValueAttribute_sql_generation()
        {
            using (var testDb = new TestDb(SqlVersion.Sql11))
            {
                var command = testDb.GetCreateTableCommand(new TestModel(), nameof(TestDb.TestTable), false);
                var expectedSql =
@"CREATE TABLE [TestTable] (
    [Int] INT NULL DEFAULT(5),
    [WeekDay] TINYINT NULL DEFAULT(1),
    [Status] CHAR(1) NULL DEFAULT('I')
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }

        [TestMethod]
        public void DefaultValueAttribute_data_set()
        {
            var dataSet = DataSet<TestModel>.New();
            var dataRow = dataSet.AddRow();
            var _ = dataSet._;
            Assert.AreEqual(5, _.Int[dataRow]);
            Assert.AreEqual(WeekDay.Mon, _.WeekDay[dataRow]);
            Assert.AreEqual(Status.Inactive, _.Status[dataRow]);
        }
    }
}

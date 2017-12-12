﻿using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class UniqueAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Id);
            }

            [Unique(Message = "ERR_DuplicateId")]
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
                get { return GetTable(ref _testTable, nameof(TestTable)); }
            }
        }

        [TestMethod]
        public void UniqueAttribute_sql_generation()
        {
            using (var testDb = new TestDb(SqlVersion.Sql11))
            {
                var command = testDb.GetCreateTableCommand(new TestModel(), nameof(TestDb.TestTable), null, false);
                var expectedSql =
@"CREATE TABLE [TestTable] (
    [Id] INT NULL

    UNIQUE NONCLUSTERED ([Id] ASC)
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }

        [TestMethod]
        public void UniqueAttribute_data_set()
        {
            var dataSet = DataSet<TestModel>.New();
            var dataRow1 = dataSet.AddRow((_, row) => _.Id[row] = 1);
            Assert.AreEqual(0, dataSet._.Validate(dataRow1, ValidationSeverity.Error).Count);
            var dataRow2 = dataSet.AddRow((_, row) => _.Id[row] = 1);
            var messages1 = dataSet._.Validate(dataRow1, ValidationSeverity.Error);
            var messages2 = dataSet._.Validate(dataRow2, ValidationSeverity.Error);
            Assert.AreEqual(1, messages1.Count);
            Assert.AreEqual("ERR_DuplicateId", messages1[0].Description);
            Assert.AreEqual(1, messages2.Count);
            Assert.AreEqual("ERR_DuplicateId", messages2[0].Description);
        }
    }
}
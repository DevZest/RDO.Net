using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class ComputationAttributeTests
    {
        [Computation(nameof(ComputeIdPlusOne))]
        [Computation(nameof(ComputeTotalValue), ComputationMode.Aggregate)]
        private sealed class Header : Model
        {
            static Header()
            {
                RegisterColumn((Header _) => _.Id);
                RegisterColumn((Header _) => _.IdPlusOne);
                RegisterColumn((Header _) => _.TotalValue);
                RegisterChildModel((Header _) => _.Details);
            }

            public _Int32 Id { get; private set; }

            public _Int32 IdPlusOne { get; private set; }

            public _Int32 TotalValue { get; private set; }

            public Detail Details { get; private set; }

            private void ComputeIdPlusOne()
            {
                IdPlusOne.ComputedAs(Id + _Int32.Const(1));
            }

            private void ComputeTotalValue()
            {
                TotalValue.ComputedAs(Details.Value.Sum());
            }
        }

        private sealed class Detail : Model
        {
            static Detail()
            {
                RegisterColumn((Detail _) => _.Value);
            }

            public _Int32 Value { get; private set; }
        }

        private sealed class TestDb : SqlSession
        {
            public TestDb(SqlVersion sqlVersion)
                : base(new SqlConnection())
            {
                SqlVersion = sqlVersion;
            }

            private DbTable<Header> _headers;
            public DbTable<Header> Headers
            {
                get { return GetTable(ref _headers, nameof(Headers)); }
            }
        }

        [TestMethod]
        public void ComputationAttribute_sql_generation()
        {
            using (var testDb = new TestDb(SqlVersion.Sql13))
            {
                var command = testDb.GetCreateTableCommand(testDb.Headers._, false);
                var expectedSql =
@"CREATE TABLE [Headers] (
    [Id] INT NULL,
    [IdPlusOne] AS (([Id] + 1)),
    [TotalValue] INT NULL
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }

        [TestMethod]
        public void ComputationAttribute_data_set()
        {
            var dataSet = DataSet<Header>.Create();
            var dataRow = dataSet.AddRow((_, row) =>
            {
                _.Id[row] = 1;
            });

            Assert.AreEqual(2, dataSet._.IdPlusOne[dataRow]);

            var details = dataSet._.Details.GetChildDataSet(dataRow);
            details.AddRow((_, row) => { _.Value[row] = 1; });
            details.AddRow((_, row) => { _.Value[row] = 2; });
            details.AddRow((_, row) => { _.Value[row] = 3; });

            Assert.AreEqual(6, dataSet._.TotalValue[dataRow]);
        }
    }
}

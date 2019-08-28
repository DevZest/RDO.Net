using DevZest.Data.Annotations;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class DbTableInsertTests : AdventureWorksTestsBase
    {
        private sealed class SimpleModel : Model<SimpleModel.PK>
        {
            public sealed class PK : CandidateKey
            {
                public PK(_Int32 id) : base(id)
                {
                }
            }

            protected override PK CreatePrimaryKey()
            {
                return new PK(Id);
            }

            public static readonly Mounter<_Int32> _Id = RegisterColumn((SimpleModel _) => _.Id);

            [Identity]
            public _Int32 Id { get; private set; }
        }

        private sealed class SimpleDb : MySqlSession
        {
            public SimpleDb(string connectionString, Action<SimpleDb> initializer)
                : base(new MySqlConnection(connectionString))
            {
                initializer?.Invoke(this);
            }

            private DbTable<SimpleModel> _simpleModel;
            public DbTable<SimpleModel> SimpleModel
            {
                get { return GetTable(ref _simpleModel); }
            }
        }

        private sealed class MockEmptySimpleModel : DbMock<SimpleDb>
        {
            public static Task<SimpleDb> CreateAsync(SimpleDb db, IProgress<DbInitProgress> progress = null, CancellationToken ct = default(CancellationToken))
            {
                return new MockEmptySimpleModel().MockAsync(db, progress, ct);
            }

            protected override void Initialize()
            {
                Mock(Db.SimpleModel);
            }
        }

        private static SimpleDb CreateSimpleDb(StringBuilder log, LogCategory logCategory = LogCategory.CommandText)
        {
            return new SimpleDb(App.GetConnectionString(), db =>
            {
                db.SetLogger(s => log.Append(s), logCategory);
            });
        }

        [TestMethod]
        public async Task DbTable_InsertScalarAsync_default_values()
        {
            var log = new StringBuilder();
            using (var db = await MockEmptySimpleModel.CreateAsync(CreateSimpleDb(log)))
            {
                await db.SimpleModel.InsertAsync();
                var result = await db.SimpleModel.ToDataSetAsync();
                Assert.AreEqual(1, result.Count);
            }
        }

        private static DataSet<SalesOrder> NewSalesOrdersTestData(int count = 2)
        {
            var result = DataSet<SalesOrder>.Create();
            result.AddTestDataRows(count);
            return result;
        }

        [TestMethod]
        public async Task DbTable_InsertScalarAsync()
        {
            var log = new StringBuilder();
            using (var db = await MockEmptySalesOrder.CreateAsync(CreateDb(log)))
            {
                await db.SalesOrderHeader.InsertAsync((m, _) =>
                {
                    m.Select(_Int32.Param(1), _.CustomerID)
                        .Select(_DateTime.Now(), _.DueDate)
                        .Select(_String.Param("FLIGHT"), _.ShipMethod);
                });
                var result = await db.SalesOrderHeader.ToDataSetAsync();
                Assert.AreEqual(1, result.Count);
            }
        }

        [TestMethod]
        public async Task DbTable_InsertScalarAsync_update_identity()
        {
            var salesOrder = NewSalesOrdersTestData(1);
            var log = new StringBuilder();
            using (var db = await MockEmptySalesOrder.CreateAsync(CreateDb(log)))
            {
                await db.SalesOrderHeader.InsertAsync(salesOrder, updateIdentity: true);
            }
            Assert.AreEqual(1, salesOrder._.SalesOrderID[0]);
        }

        [TestMethod]
        public async Task DbTable_InsertAsync_DataSet_update_identity()
        {
            var salesOrders = NewSalesOrdersTestData();
            var log = new StringBuilder();
            using (var db = await MockEmptySalesOrder.CreateAsync(CreateDb(log, LogCategory.All)))
            {
                var result = await db.SalesOrderHeader.InsertAsync(salesOrders, updateIdentity: true);
                Assert.AreEqual(2, result);
            }
            Assert.AreEqual(1, salesOrders._.SalesOrderID[0]);
            Assert.AreEqual(2, salesOrders._.SalesOrderID[1]);
        }
    }
}
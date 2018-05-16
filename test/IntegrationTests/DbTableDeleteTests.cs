using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class DbTableDeleteTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public async Task DbTable_DeleteAsync_without_source()
        {
            var log = new StringBuilder();
            using (var db = await new SalesOrderMockDb().InitializeAsync(await OpenDbAsync(log)))
            {
                var count = await db.SalesOrderDetails.CountAsync();
                var countToDelete = await db.SalesOrderDetails.Where(_ => _.SalesOrderDetailID > 2).CountAsync();
                Assert.IsTrue(countToDelete > 0);
                var countDeleted = await db.SalesOrderDetails.Delete(_ => _.SalesOrderDetailID > 2).ExecuteAsync();
                Assert.AreEqual(countToDelete, countDeleted);
                Assert.AreEqual(count - countToDelete, await db.SalesOrderDetails.CountAsync());
            }
        }

        [TestMethod]
        public async Task DbTable_DeleteAsync_from_scalar()
        {
            var log = new StringBuilder();
            using (var db = await new SalesOrderMockDb().InitializeAsync(await OpenDbAsync(log)))
            {
                var count = await db.SalesOrderDetails.CountAsync();
                var dataSet = await db.SalesOrderDetails.Where(x => x.SalesOrderDetailID == 1).ToDataSetAsync();
                Assert.IsTrue(dataSet.Count == 1);

                bool success = await db.SalesOrderDetails.Delete(dataSet, 0, (s, _) => s.Match(_)).ExecuteAsync() > 0;
                Assert.IsTrue(success);
                Assert.AreEqual(count - 1, await db.SalesOrderDetails.CountAsync());
            }
        }

        [TestMethod]
        public async Task DbTable_DeleteAsync_from_DataSet()
        {
            var log = new StringBuilder();
            using (var db = await new SalesOrderMockDb().InitializeAsync(await OpenDbAsync(log)))
            {
                var count = await db.SalesOrderDetails.CountAsync();
                var countToDelete = await db.SalesOrderDetails.Where(_ => _.SalesOrderID == 1 | _.SalesOrderID == 2).CountAsync();
                Assert.IsTrue(countToDelete > 1);

                var query = db.CreateQuery((DbQueryBuilder builder, SalesOrder.Key _) =>
                {
                    builder.From(db.SalesOrderHeaders, out var s)
                    .AutoSelect()
                    .Where(s.SalesOrderID == 1 | s.SalesOrderID == 2);
                });
                var dataSet = await query.ToDataSetAsync();

                var countDeleted = await db.SalesOrderDetails.Delete(dataSet, (s, _) => s.Match(_.FK_SalesOrderHeader)).ExecuteAsync();
                Assert.AreEqual(countToDelete, countDeleted);
                Assert.AreEqual(count - countDeleted, await db.SalesOrderDetails.CountAsync());
            }
        }
    }
}

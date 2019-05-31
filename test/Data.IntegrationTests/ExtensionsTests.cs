using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class ExtensionsTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public async Task DbSet_SingleOrDefaultAsync()
        {
            var sb = new StringBuilder();
            using (var db = CreateDb(sb))
            {
                {
                    var salesOrderId = 71774;
                    var result = await db.SalesOrderHeader.Where(_ => _.SalesOrderID == salesOrderId).SingleOrDefaultAsync(_ => _.SalesOrderID);
                    Assert.AreEqual(salesOrderId, result[0]);
                }

                {
                    var salesOrderId = 0;
                    var result = await db.SalesOrderHeader.Where(_ => _.SalesOrderID == salesOrderId).SingleOrDefaultAsync(_ => _.SalesOrderID);
                    Assert.IsNull(result);
                }
            }
        }
    }
}

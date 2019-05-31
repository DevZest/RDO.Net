using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class ExtensionsTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public async Task DbSet_SingleOrDefaultAsync()
        {
            using (var db = CreateDb())
            {
                {
                    var salesOrderId = 71774;
                    var result = await db.SalesOrderHeader.SingleOrDefaultAsync(_ => _.SalesOrderID == salesOrderId, _ => _.SalesOrderID);
                    Assert.AreEqual(salesOrderId, result[0]);
                }

                {
                    var salesOrderId = 0;
                    var result = await db.SalesOrderHeader.SingleOrDefaultAsync(_ => _.SalesOrderID == salesOrderId, _ => _.SalesOrderID);
                    Assert.IsNull(result);
                }
            }
        }
    }
}

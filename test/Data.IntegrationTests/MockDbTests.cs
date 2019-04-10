using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Samples.AdventureWorksLT;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class MockDbTests : AdventureWorksTestsBase
    {
        /// <remarks><see cref="ProductCategory"/> is chosen for only having self referencing foreign key.</remarks>
        [TestMethod]
        public async Task MockDb_ProductCategory()
        {
            var log = new StringBuilder();
            using (var db = await MockProductCategory.CreateAsync(CreateDb(log)))
            {
                Assert.AreEqual(13, db.ProductCategory.CountAsync().Result);
            }
        }


        [TestMethod]
        public async Task MockDb_SalesOrder()
        {
            var log = new StringBuilder();
            using (var db = await MockEmptySalesOrder.CreateAsync(CreateDb(log)))
            {
                Assert.AreEqual(0, db.SalesOrderHeader.CountAsync().Result);
                Assert.AreEqual(0, db.SalesOrderDetail.CountAsync().Result);
            }
        }
    }
}

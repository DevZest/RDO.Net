using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Samples.AdventureWorksLT;
using System.Text;

namespace DevZest.Data
{
    [TestClass]
    public class MockDbTests : AdventureWorksTestsBase
    {
        /// <remarks><see cref="ProductCategory"/> is chosen for only having self referencing foreign key.</remarks>
        [TestMethod]
        public void MockDb_ProductCategory()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().Initialize(OpenDb(log)))
            {
                Assert.AreEqual(13, db.ProductCategories.Count());
            }
        }


        [TestMethod]
        public void MockDb_SalesOrder()
        {
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(OpenDb(log)))
            {
                Assert.AreEqual(0, db.SalesOrderHeaders.Count());
                Assert.AreEqual(0, db.SalesOrderDetails.Count());
            }
        }
    }
}

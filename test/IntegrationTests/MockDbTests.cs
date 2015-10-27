
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DevZest.Samples.AdventureWorksLT;
using System.Text;

namespace DevZest.Data
{
    [TestClass]
    public class MockDbTests : AdventureWorksTestsBase
    {
        /// <remarks><see cref="ProductCategory"/> is chosen for only having self referencing foreign key.</remarks>
        private sealed class ProductCategoryMockDb : MockDb<Db>
        {
            public ProductCategoryMockDb(DataSet<ProductCategory> productCategories)
            {
                _productCategories = productCategories;
            }

            DataSet<ProductCategory> _productCategories;

            protected override void Initialize()
            {
                Mock(Db.ProductCategories, _productCategories);
            }
        }

        [TestMethod]
        public void MockDb_ProductCategory()
        {
            var productCategories = DataSet<ProductCategory>.New();
            for (int i = 0; i < 3; i++)
            {
                var row = productCategories.AddRow();
                productCategories._.Name[row] = "Name" + i.ToString();
            }

            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb(productCategories).Initialize(OpenDb(log)))
            {
                Assert.AreEqual(productCategories.Count, db.ProductCategories.Count());
            }
        }


        [TestMethod]
        public void MockDb_SalesOrder()
        {
            var log = new StringBuilder();
            using (var db = new SalesOrderMockDb(null, null).Initialize(OpenDb(log)))
            {
                Assert.AreEqual(0, db.SalesOrders.Count());
                Assert.AreEqual(0, db.SalesOrderDetails.Count());
            }
        }
    }
}

using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class RelationshipTests
    {
        [TestMethod]
        public void Relationship_join_with_same_primary_key()
        {
            var product1 = new Product();
            var product2 = new Product();

            var join = product1.Join(product2);
            Assert.AreEqual(product1.PrimaryKey, join.Source);
            Assert.AreEqual(product2.PrimaryKey, join.Target);

            var relationship = join.Mappings;
            Assert.AreEqual(1, relationship.Count);
            Assert.AreEqual(product1.PrimaryKey.ProductID, relationship[0].Source);
            Assert.AreEqual(product2.PrimaryKey.ProductID, relationship[0].Target);
        }

        [TestMethod]
        public void Relationship_join_with_same_foreign_key()
        {
            var product1 = new Product();
            var product2 = new Product();

            var join = product1.Join(product2, _ => _.PrimaryKey, _ => _.PrimaryKey);
            Assert.AreEqual(product1.PrimaryKey, join.Source);
            Assert.AreEqual(product2.PrimaryKey, join.Target);

            var relationship = join.Mappings;
            Assert.AreEqual(1, relationship.Count);
            Assert.AreEqual(product1.PrimaryKey.ProductID, relationship[0].Source);
            Assert.AreEqual(product2.PrimaryKey.ProductID, relationship[0].Target);
        }

        [TestMethod]
        public void Relationship_join_source_to_target()
        {
            var product = new Product();
            var productCategory = new ProductCategory();

            var join = product.JoinTo(productCategory, _ => _.ProductCategoryKey);
            Assert.AreEqual(product.ProductCategoryKey, join.Source);
            Assert.AreEqual(productCategory.PrimaryKey, join.Target);

            var relationship = join.Mappings;
            Assert.AreEqual(1, relationship.Count);
            Assert.AreEqual(product.ProductCategoryKey.ProductCategoryID, relationship[0].Source);
            Assert.AreEqual(productCategory.PrimaryKey.ProductCategoryID, relationship[0].Target);
        }

        [TestMethod]
        public void Relationship_join_target_to_source()
        {
            var productCategory = new ProductCategory();
            var product = new Product();

            var join = productCategory.JoinFrom(product, _ => _.ProductCategoryKey);
            Assert.AreEqual(productCategory.PrimaryKey, join.Source);
            Assert.AreEqual(product.ProductCategoryKey, join.Target);

            var relationship = join.Mappings;
            Assert.AreEqual(1, relationship.Count);
            Assert.AreEqual(productCategory.PrimaryKey.ProductCategoryID, relationship[0].Source);
            Assert.AreEqual(product.ProductCategoryKey.ProductCategoryID, relationship[0].Target);
        }
    }
}

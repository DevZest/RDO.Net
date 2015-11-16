using DevZest.Data;

namespace DevZest.Samples.AdventureWorksLT
{
    internal sealed class ProductCategoryMockDb : MockDb<Db>
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
}

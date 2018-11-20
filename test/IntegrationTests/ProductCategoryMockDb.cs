using DevZest.Samples.AdventureWorksLT;

namespace DevZest.Data
{
    internal sealed class ProductCategoryMockDb : MockDb<Db>
    {
        private static DataSet<ProductCategory>[] s_productCategories = new DataSet<ProductCategory>[] {
            DataSet<ProductCategory>.ParseJson(Strings.Mock_ProductCategories_Level1),
            DataSet<ProductCategory>.ParseJson(Strings.Mock_ProductCategories_Level2),
            DataSet<ProductCategory>.ParseJson(Strings.Mock_ProductCategories_Level3),
        };

        public ProductCategoryMockDb()
            : this(s_productCategories)
        {
        }

        public ProductCategoryMockDb(DataSet<ProductCategory>[] productCategories)
        {
            _productCategories = productCategories;
        }

        DataSet<ProductCategory>[] _productCategories;

        protected override void Initialize()
        {
            Mock(Db.ProductCategory, _productCategories);
        }
    }
}

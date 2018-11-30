using DevZest.Samples.AdventureWorksLT;

namespace DevZest.Data
{
    internal sealed class ProductCategoryMockDb : MockDb<Db>
    {
        private static DataSet<ProductCategory> MockData()
        {
            return DataSet<ProductCategory>.ParseJson(Strings.Mock_ProdcutCategory);
        }

        protected override void Initialize()
        {
            Mock(Db.ProductCategory, MockData);
        }
    }
}

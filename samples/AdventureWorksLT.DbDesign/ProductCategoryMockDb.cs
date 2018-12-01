using DevZest.Data;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class ProductCategoryMockDb : MockDb<Db>
    {
        private static DataSet<ProductCategory> MockData()
        {
            return DataSet<ProductCategory>.ParseJson(Strings.Mock_ProductCategory);
        }

        protected override void Initialize()
        {
            Mock(Db.ProductCategory, MockData);
        }
    }
}

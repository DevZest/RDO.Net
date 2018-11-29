using DevZest.Samples.AdventureWorksLT;

namespace DevZest.Data
{
    internal sealed class ProductCategoryMockDb : MockDb<Db>
    {
        private static DataSet<ProductCategory> Level1()
        {
            return DataSet<ProductCategory>.ParseJson(Strings.Mock_ProductCategories_Level1);
        }

        private static DataSet<ProductCategory> Level2()
        {
            return DataSet<ProductCategory>.ParseJson(Strings.Mock_ProductCategories_Level2);
        }

        private static DataSet<ProductCategory> Level3()
        {
            return DataSet<ProductCategory>.ParseJson(Strings.Mock_ProductCategories_Level3);
        }

        protected override void Initialize()
        {
            Mock(Db.ProductCategory, Level1, Level2, Level3);
        }
    }
}

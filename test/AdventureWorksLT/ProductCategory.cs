using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductCategory : BaseModel<ProductCategory.Key>
    {
        public sealed class Key : KeyBase
        {
            public Key(_Int32 productCategoryID)
            {
                ProductCategoryID = productCategoryID;
            }

            public _Int32 ProductCategoryID { get; private set; }
        }

        public static readonly Property<_Int32> _ProductCategoryID = RegisterColumn((ProductCategory x) => x.ProductCategoryID);
        public static readonly Property<_Int32> _ParentProductCategoryID = RegisterColumn((ProductCategory x) => x.ParentProductCategoryID);
        public static readonly Property<_String> _Name = RegisterColumn((ProductCategory x) => x.Name, x => { x.AsNVarChar(50); });

        static ProductCategory()
        {
            RegisterChildModel((ProductCategory x) => x.SubCategories, (ProductCategory x) => x.ParentProductCategory);
        }

        public ProductCategory()
        {
            _primaryKey = new Key(ProductCategoryID);
            ParentProductCategory = new Key(ParentProductCategoryID);
        }

        public ProductCategory SubCategories { get; private set; }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get { return _primaryKey; }
        }

        [Identity(1, 1)]
        public _Int32 ProductCategoryID { get; private set; }

        public _Int32 ParentProductCategoryID { get; private set; }

        public Key ParentProductCategory { get; private set; }

        [UdtName]
        [Required]
        public _String Name { get; private set; }
    }
}

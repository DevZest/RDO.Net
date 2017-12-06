using System;
using DevZest.Data;
using DevZest.Data.SqlServer;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductCategory : BaseModel<ProductCategory.Key>
    {
        public sealed class Key : PrimaryKey
        {
            public Key(_Int32 productCategoryID)
            {
                ProductCategoryID = productCategoryID;
            }

            public _Int32 ProductCategoryID { get; private set; }
        }

        public class Ref : Model<Key>
        {
            static Ref()
            {
                RegisterColumn((Ref _) => _.ProductCategoryID, _ProductCategoryID);
            }
            
            private Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get
                {
                    if (_primaryKey == null)
                        _primaryKey = new Key(ProductCategoryID);
                    return _primaryKey;
                }
            }

            public _Int32 ProductCategoryID { get; private set; }
        }

        public static readonly Mounter<_Int32> _ProductCategoryID;
        public static readonly Mounter<_Int32> _ParentProductCategoryID;
        public static readonly Mounter<_String> _Name;

        static ProductCategory()
        {
            _ProductCategoryID = RegisterColumn((ProductCategory _) => _.ProductCategoryID);
            _ParentProductCategoryID = RegisterColumn((ProductCategory _) => _.ParentProductCategoryID);
            _Name = RegisterColumn((ProductCategory _) => _.Name);
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
        [Description("Primary key for ProductCategory records.")]
        public _Int32 ProductCategoryID { get; private set; }

        [Description("Product category identification number of immediate ancestor category. Foreign key to ProductCategory.ProductCategoryID.")]
        public _Int32 ParentProductCategoryID { get; private set; }

        [Description("Category description.")]
        public Key ParentProductCategory { get; private set; }

        [UdtName]
        [Required]
        [AsNVarChar(50)]
        public _String Name { get; private set; }
    }
}

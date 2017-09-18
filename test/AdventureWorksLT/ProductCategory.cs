using System;
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

        public class Ref : Model<Key>
        {
            public static readonly Mounter<_Int32> _ProductCategoryID = RegisterColumn((Ref _) => _.ProductCategoryID);

            public Ref()
            {
                _primaryKey = new Key(ProductCategoryID);
            }

            private readonly Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get { return _primaryKey; }
            }

            public _Int32 ProductCategoryID { get; private set; }
        }

        public static readonly Mounter<_Int32> _ProductCategoryID = RegisterColumn((ProductCategory _) => _.ProductCategoryID, Ref._ProductCategoryID);
        public static readonly Mounter<_Int32> _ParentProductCategoryID = RegisterColumn((ProductCategory _) => _.ParentProductCategoryID);
        public static readonly Mounter<_String> _Name = RegisterColumn((ProductCategory _) => _.Name, _ => { _.AsNVarChar(50); });

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

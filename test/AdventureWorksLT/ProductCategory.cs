using System;
using DevZest.Data;
using DevZest.Data.SqlServer;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductCategory : BaseModel<ProductCategory.PK>
    {
        [DbPrimaryKey("PK_ProductCategory_ProductCategoryID", Description = "Primary key (clustered) constraint")]
        public sealed class PK : PrimaryKey
        {
            public PK(_Int32 productCategoryID)
            {
                ProductCategoryID = productCategoryID;
            }

            public _Int32 ProductCategoryID { get; private set; }
        }

        public static IDataValues GetKey(int productCategoryId)
        {
            return DataValues.Create(_Int32.Const(productCategoryId));
        }

        public sealed class Key : Model<PK>
        {
            static Key()
            {
                RegisterColumn((Key _) => _.ProductCategoryID, _ProductCategoryID);
            }

            protected override PK CreatePrimaryKey()
            {
                return new PK(ProductCategoryID);
            }

            public _Int32 ProductCategoryID { get; private set; }
        }

        public class Ref : Ref<PK>
        {
            static Ref()
            {
                Register((Ref _) => _.ProductCategoryID, _ProductCategoryID);
            }

            public _Int32 ProductCategoryID { get; private set; }

            protected override PK CreatePrimaryKey()
            {
                return new PK(ProductCategoryID);
            }
        }

        public class Lookup : LookupBase
        {
            static Lookup()
            {
                Register((Lookup _) => _.Name, _Name);
            }

            public _String Name { get; private set; }
        }

        public static readonly Mounter<_Int32> _ProductCategoryID;
        public static readonly Mounter<_Int32> _ParentProductCategoryID;
        public static readonly Mounter<_String> _Name;

        static ProductCategory()
        {
            _ProductCategoryID = RegisterColumn((ProductCategory _) => _.ProductCategoryID);
            _ParentProductCategoryID = RegisterColumn((ProductCategory _) => _.ParentProductCategoryID);
            _Name = RegisterColumn((ProductCategory _) => _.Name);
            RegisterChildModel((ProductCategory x) => x.SubCategories, (ProductCategory x) => x.FK_ParentProductCategory);
        }

        public ProductCategory SubCategories { get; private set; }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ProductCategoryID);
        }

        [Identity(1, 1)]
        [DbColumn(Description = "Primary key for ProductCategory records.")]
        public _Int32 ProductCategoryID { get; private set; }

        [DbColumn(Description = "Product category identification number of immediate ancestor category. Foreign key to ProductCategory.ProductCategoryID.")]
        public _Int32 ParentProductCategoryID { get; private set; }

        private PK _fk_productCategory;
        [DbColumn(Description = "Category description.")]
        public PK FK_ParentProductCategory
        {
            get { return _fk_productCategory ?? (_fk_productCategory = new PK(ParentProductCategoryID)); }
        }

        [UdtName]
        [Required]
        [AsNVarChar(50)]
        [Unique(Name = "AK_ProductCategory_Name", Description = "Unique nonclustered constraint.")]
        public _String Name { get; private set; }
    }
}

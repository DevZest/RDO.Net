using DevZest.Data;
using DevZest.Data.SqlServer;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [UniqueConstraint(nameof(AK_ProductCategory_Name), Description = "Unique nonclustered constraint.")]
    public class ProductCategory : BaseModel<ProductCategory.PK>
    {
        [DbPrimaryKey("PK_ProductCategory_ProductCategoryID", Description = "Primary key (clustered) constraint")]
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 productCategoryID)
                : base(productCategoryID)
            {
            }
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                Register((Key _) => _.ProductCategoryID, _ProductCategoryID);
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

            protected override PK CreateForeignKey()
            {
                return new PK(ProductCategoryID);
            }
        }

        public class Lookup : Projection
        {
            static Lookup()
            {
                Register((Lookup _) => _.Name, _Name);
            }

            public _String Name { get; private set; }
        }

        public static readonly Mounter<_Int32> _ProductCategoryID = RegisterColumn((ProductCategory _) => _.ProductCategoryID);
        public static readonly Mounter<_Int32> _ParentProductCategoryID = RegisterColumn((ProductCategory _) => _.ParentProductCategoryID);
        public static readonly Mounter<_String> _Name = RegisterColumn((ProductCategory _) => _.Name);

        static ProductCategory()
        {
            RegisterChildModel((ProductCategory x) => x.SubCategories, (ProductCategory x) => x.FK_ParentProductCategory);
        }

        public ProductCategory SubCategories { get; private set; }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ProductCategoryID);
        }

        [Identity]
        [DbColumn(Description = "Primary key for ProductCategory records.")]
        public _Int32 ProductCategoryID { get; private set; }

        [DbColumn(Description = "Product category identification number of immediate ancestor category. Foreign key to ProductCategory.ProductCategoryID.")]
        public _Int32 ParentProductCategoryID { get; private set; }

        private PK _fk_parentProductCategory;
        public PK FK_ParentProductCategory
        {
            get { return _fk_parentProductCategory ?? (_fk_parentProductCategory = new PK(ParentProductCategoryID)); }
        }

        [UdtName]
        [DbColumn(Description = "Category description.")]
        [Required]
        [SqlNVarChar(50)]
        public _String Name { get; private set; }

        [_UniqueConstraint]
        private ColumnSort[] AK_ProductCategory_Name => new ColumnSort[] { Name };
    }
}

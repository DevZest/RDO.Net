using DevZest.Data;
using DevZest.Data.SqlServer;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductDescription : BaseModel<ProductDescription.PK>
    {
        [DbPrimaryKey("PK_ProductDescription_ProductDescriptionID", Description = "Primary key (clustered) constraint")]
        public sealed class PK : PrimaryKey
        {
            public PK(_Int32 productDescriptionID)
                : base(productDescriptionID)
            {
            }
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                Register((Key _) => _.ProductDescriptionID, _ProductDescriptionID);
            }

            protected sealed override PK CreatePrimaryKey()
            {
                return new PK(ProductDescriptionID);
            }

            public _Int32 ProductDescriptionID { get; private set; }
        }

        public class Ref : Ref<PK>
        {
            static Ref()
            {
                Register((Ref _) => _.ProductDescriptionID, _ProductDescriptionID);
            }

            public _Int32 ProductDescriptionID { get; private set; }

            protected override PK CreateForeignKey()
            {
                return new PK(ProductDescriptionID);
            }
        }

        public class Lookup : Projection
        {
            static Lookup()
            {
                Register((Lookup _) => _.Description, _Description);
            }

            public _String Description { get; private set; }
        }

        public static readonly Mounter<_Int32> _ProductDescriptionID = RegisterColumn((ProductDescription _) => _.ProductDescriptionID);
        public static readonly Mounter<_String> _Description = RegisterColumn((ProductDescription _) => _.Description);

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ProductDescriptionID);
        }

        [Identity(1, 1)]
        [DbColumn(Description = "Primary key for ProductDescription records.")]
        [PkColumn]
        public _Int32 ProductDescriptionID { get; private set; }

        [Required]
        [SqlNVarChar(400)]
        [DbColumn(Description = "Description of the product.")]
        public _String Description { get; private set; }
    }
}

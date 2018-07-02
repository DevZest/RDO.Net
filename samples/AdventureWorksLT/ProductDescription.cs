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
            public static IDataValues Const(int productDescriptionID)
            {
                return DataValues.Create(_Int32.Const(productDescriptionID));
            }

            public PK(_Int32 productDescriptionID)
                : base(productDescriptionID)
            {
            }

            public _Int32 ProductDescription
            {
                get { return GetColumn<_Int32>(0); }
            }
        }

        public sealed class Key : Model<PK>
        {
            static Key()
            {
                RegisterColumn((Key _) => _.ProductDescriptionID, _ProductDescriptionID);
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

            protected override PK GetForeignKey()
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

        public static readonly Mounter<_Int32> _ProductDescriptionID;
        public static readonly Mounter<_String> _Description;

        static ProductDescription()
        {
            _ProductDescriptionID = RegisterColumn((ProductDescription _) => _.ProductDescriptionID);
            _Description = RegisterColumn((ProductDescription _) => _.Description);
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ProductDescriptionID);
        }

        [Identity(1, 1)]
        [DbColumn(Description = "Primary key for ProductDescription records.")]
        public _Int32 ProductDescriptionID { get; private set; }

        [Required]
        [AsNVarChar(400)]
        [DbColumn(Description = "Description of the product.")]
        public _String Description { get; private set; }
    }
}

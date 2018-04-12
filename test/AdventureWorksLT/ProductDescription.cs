using System;
using DevZest.Data;
using DevZest.Data.SqlServer;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductDescription : BaseModel<ProductDescription.PK>
    {
        [DbConstraint("PK_ProductDescription_ProductDescriptionID", Description = "Primary key (clustered) constraint")]
        public sealed class PK : PrimaryKey
        {
            public PK(_Int32 productDescriptionID)
            {
                ProductDescriptionID = productDescriptionID;
            }

            public _Int32 ProductDescriptionID { get; private set; }
        }

        public static IDataValues GetKey(int productDescriptionId)
        {
            return DataValues.Create(_Int32.Const(productDescriptionId));
        }

        public class Key : Model<PK>
        {
            static Key()
            {
                RegisterColumn((Key _) => _.ProductDescriptionID, _ProductDescriptionID);
            }

            private PK _primaryKey;
            public sealed override PK PrimaryKey
            {
                get { return _primaryKey ?? (_primaryKey = new PK(ProductDescriptionID)); }
            }

            public _Int32 ProductDescriptionID { get; private set; }
        }

        public static readonly Mounter<_Int32> _ProductDescriptionID;
        public static readonly Mounter<_String> _Description;

        static ProductDescription()
        {
            _ProductDescriptionID = RegisterColumn((ProductDescription _) => _.ProductDescriptionID);
            _Description = RegisterColumn((ProductDescription _) => _.Description);
        }

        private PK _primaryKey;
        public sealed override PK PrimaryKey
        {
            get { return _primaryKey ?? (_primaryKey = new PK(ProductDescriptionID)); }
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

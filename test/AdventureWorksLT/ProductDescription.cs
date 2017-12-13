using System;
using DevZest.Data;
using DevZest.Data.SqlServer;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductDescription : BaseModel<ProductDescription.Key>
    {
        [DbConstraint("PK_ProductDescription_ProductDescriptionID", Description = "Primary key (clustered) constraint")]
        public sealed class Key : PrimaryKey
        {
            public Key(_Int32 productDescriptionID)
            {
                ProductDescriptionID = productDescriptionID;
            }

            public _Int32 ProductDescriptionID { get; private set; }
        }

        public class Ref : Model<Key>
        {
            static Ref()
            {
                RegisterColumn((Ref _) => _.ProductDescriptionID, _ProductDescriptionID);
            }

            private Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get
                {
                    if (_primaryKey == null)
                        _primaryKey = new Key(ProductDescriptionID);
                    return _primaryKey;
                }
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

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get
            {
                if (_primaryKey == null)
                    _primaryKey = new Key(ProductDescriptionID);
                return _primaryKey;
            }
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

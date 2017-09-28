using System;
using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductDescription : BaseModel<ProductDescription.Key>
    {
        public sealed class Key : KeyBase
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
        public _Int32 ProductDescriptionID { get; private set; }

        [Required]
        [AsNVarChar(400)]
        public _String Description { get; private set; }
    }
}

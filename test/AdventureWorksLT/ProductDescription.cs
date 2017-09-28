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
            public static readonly Mounter<_Int32> _ProductDescriptionID;

            static Ref()
            {
                _ProductDescriptionID = RegisterColumn((Ref _) => _.ProductDescriptionID);
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

        public static readonly Mounter<_String> _Description;

        static ProductDescription()
        {
            RegisterColumn((ProductDescription _) => _.ProductDescriptionID, Ref._ProductDescriptionID);
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

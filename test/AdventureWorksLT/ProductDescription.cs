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
            public static readonly Mounter<_Int32> _ProductDescriptionID = RegisterColumn((Ref _) => _.ProductDescriptionID);

            public Ref()
            {
                _primaryKey = new Key(ProductDescriptionID);
            }

            private readonly Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get { return _primaryKey; }
            }

            public _Int32 ProductDescriptionID { get; private set; }
        }

        public static readonly Mounter<_Int32> _ProductDescriptionID = RegisterColumn((ProductDescription _) => _.ProductDescriptionID, Ref._ProductDescriptionID);
        public static readonly Mounter<_String> _Description = RegisterColumn((ProductDescription _) => _.Description);

        public ProductDescription()
        {
            _primaryKey = new Key(ProductDescriptionID);
        }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get { return _primaryKey; }
        }

        [Identity(1, 1)]
        public _Int32 ProductDescriptionID { get; private set; }

        [Required]
        [AsNVarChar(400)]
        public _String Description { get; private set; }
    }
}

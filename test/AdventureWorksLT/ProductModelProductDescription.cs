using DevZest.Data;
using DevZest.Data.SqlServer;
using System;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductModelProductDescription : BaseModel<ProductModelProductDescription.Key>
    {
        public sealed class Key : ModelKey
        {
            public Key(_Int32 productModelID, _Int32 productDescriptionID, _String culture)
            {
                ProductModelID = productModelID;
                ProductDescriptionID = productDescriptionID;
                Culture = culture;
            }

            public _Int32 ProductModelID { get; private set; }

            public _Int32 ProductDescriptionID { get; private set; }

            public _String Culture { get; private set; }
        }

        public static Property<_Int32> _ProductModelID = RegisterColumn((ProductModelProductDescription x) => x.ProductModelID);
        public static Property<_Int32> _ProductDescriptionID = RegisterColumn((ProductModelProductDescription x) => x.ProductDescriptionID);
        public static readonly Property<_String> _Culture = RegisterColumn((ProductModelProductDescription x) => x.Culture);

        public ProductModelProductDescription()
        {
            _primaryKey = new Key(ProductModelID, ProductDescriptionID, Culture);
            ProductModelKey = new ProductModel.Key(ProductModelID);
            ProductDescriptionKey = new ProductDescription.Key(ProductDescriptionID);
        }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get { return _primaryKey; }
        }

        public ProductModel.Key ProductModelKey { get; private set; }

        public ProductDescription.Key ProductDescriptionKey { get; private set; }

        public _Int32 ProductModelID { get; private set; }

        public _Int32 ProductDescriptionID { get; private set; }

        [AsNChar(6)]
        public _String Culture { get; private set; }
    }
}

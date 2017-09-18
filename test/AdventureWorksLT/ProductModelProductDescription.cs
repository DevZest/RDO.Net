using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductModelProductDescription : BaseModel<ProductModelProductDescription.Key>
    {
        public sealed class Key : KeyBase
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

        public static Mounter<_Int32> _ProductModelID = RegisterColumn((ProductModelProductDescription _) => _.ProductModelID);
        public static Mounter<_Int32> _ProductDescriptionID = RegisterColumn((ProductModelProductDescription _) => _.ProductDescriptionID);
        public static readonly Mounter<_String> _Culture = RegisterColumn((ProductModelProductDescription _) => _.Culture);

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

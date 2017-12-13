using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductModelProductDescription : BaseModel<ProductModelProductDescription.Key>
    {
        [DbConstraint("PK_ProductModelProductDescription_ProductModelID_ProductDescriptionID_Culture", Description = "Primary key (clustered) constraint")]
        public sealed class Key : PrimaryKey
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

        public static readonly Mounter<_String> _Culture;

        static ProductModelProductDescription()
        {
            RegisterColumn((ProductModelProductDescription _) => _.ProductModelID, AdventureWorksLT.ProductModel._ProductModelID);
            RegisterColumn((ProductModelProductDescription _) => _.ProductDescriptionID, AdventureWorksLT.ProductDescription._ProductDescriptionID);
            _Culture = RegisterColumn((ProductModelProductDescription _) => _.Culture);
        }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get
            {
                if (_primaryKey == null)
                    _primaryKey = new Key(ProductModelID, ProductDescriptionID, Culture);
                return _primaryKey;
            }
        }

        private ProductModel.Key _productModel;
        public ProductModel.Key ProductModel
        {
            get
            {
                if (_productModel == null)
                    _productModel = new ProductModel.Key(ProductModelID);
                return _productModel;
            }
        }

        private ProductDescription.Key _productDescription;
        public ProductDescription.Key ProductDescription
        {
            get
            {
                if (_productDescription == null)
                    _productDescription = new ProductDescription.Key(ProductDescriptionID);
                return _productDescription;
            }
        }

        [DbColumn(Description = "Primary key. Foreign key to ProductModel.ProductModelID.")]
        public _Int32 ProductModelID { get; private set; }

        [DbColumn(Description = "Primary key. Foreign key to ProductDescription.ProductDescriptionID.")]
        public _Int32 ProductDescriptionID { get; private set; }

        [AsNChar(6)]
        [DbColumn(Description = "The culture for which the description is written.")]
        public _String Culture { get; private set; }
    }
}

using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductModelProductDescription : BaseModel<ProductModelProductDescription.PK>
    {
        [DbConstraint("PK_ProductModelProductDescription_ProductModelID_ProductDescriptionID_Culture", Description = "Primary key (clustered) constraint")]
        public sealed class PK : PrimaryKey
        {
            public PK(_Int32 productModelID, _Int32 productDescriptionID, _String culture)
            {
                ProductModelID = productModelID;
                ProductDescriptionID = productDescriptionID;
                Culture = culture;
            }

            public _Int32 ProductModelID { get; private set; }

            public _Int32 ProductDescriptionID { get; private set; }

            public _String Culture { get; private set; }
        }

        public static IDataValues GetKey(int productModelId, int productDescriptionId, string culture)
        {
            return DataValues.Create(_Int32.Const(productModelId), _Int32.Const(productDescriptionId), _String.Const(culture));
        }

        public static readonly Mounter<_String> _Culture;

        static ProductModelProductDescription()
        {
            RegisterColumn((ProductModelProductDescription _) => _.ProductModelID, AdventureWorksLT.ProductModel._ProductModelID);
            RegisterColumn((ProductModelProductDescription _) => _.ProductDescriptionID, AdventureWorksLT.ProductDescription._ProductDescriptionID);
            _Culture = RegisterColumn((ProductModelProductDescription _) => _.Culture);
        }

        private PK _primaryKey;
        public sealed override PK PrimaryKey
        {
            get { return _primaryKey ?? (_primaryKey = new PK(ProductModelID, ProductDescriptionID, Culture)); }
        }

        private ProductModel.PK _fk_productModel;
        public ProductModel.PK FK_ProductModel
        {
            get { return _fk_productModel ?? (_fk_productModel = new ProductModel.PK(ProductModelID)); }
        }

        private ProductDescription.PK _fk_productDescription;
        public ProductDescription.PK FK_ProductDescription
        {
            get { return _fk_productDescription ?? (_fk_productDescription = new ProductDescription.PK(ProductDescriptionID)); }
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

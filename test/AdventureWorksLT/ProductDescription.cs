using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductDescription : BaseModel<ProductDescription.Key>
    {
        public sealed class Key : ModelKey
        {
            public Key(_Int32 productDescriptionID)
            {
                ProductDescriptionID = productDescriptionID;
            }

            public _Int32 ProductDescriptionID { get; private set; }
        }

        public static readonly Accessor<ProductDescription, _Int32> ProductDescriptionIDAccessor = RegisterColumn((ProductDescription x) => x.ProductDescriptionID);
        public static readonly Accessor<ProductDescription, _String> DescriptionAccessor = RegisterColumn((ProductDescription x) => x.Description);

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

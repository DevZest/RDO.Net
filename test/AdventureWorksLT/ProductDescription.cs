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

        public static readonly Property<_Int32> _ProductDescriptionID = RegisterColumn((ProductDescription x) => x.ProductDescriptionID);
        public static readonly Property<_String> _Description = RegisterColumn((ProductDescription x) => x.Description);

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

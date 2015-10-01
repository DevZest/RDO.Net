using DevZest.Data;
using DevZest.Data.SqlServer;

namespace AdventureWorksLT
{
    public class Product : BaseModel<Product.Key>
    {
        public sealed class Key : ModelKey
        {
            public Key(_Int32 productID)
            {
                ProductID = productID;
            }

            public _Int32 ProductID { get; private set; }
        }

        public static readonly Accessor<Product, _Int32> ProductIDAccessor = RegisterColumn((Product x) => x.ProductID);
        public static readonly Accessor<Product, _String> NameAccessor = RegisterColumn((Product x) => x.Name);
        public static readonly Accessor<Product, _String> ProductNumberAccessor = RegisterColumn((Product x) => x.ProductNumber);
        public static readonly Accessor<Product, _String> ColorAccessor = RegisterColumn((Product x) => x.Color);
        public static readonly Accessor<Product, _Decimal> StandardCostAccessor = RegisterColumn((Product x) => x.StandardCost);
        public static readonly Accessor<Product, _Decimal> ListPriceAccessor = RegisterColumn((Product x) => x.ListPrice);
        public static readonly Accessor<Product, _String> SizeAccessor = RegisterColumn((Product x) => x.Size);
        public static readonly Accessor<Product, _Decimal> WeightAccessor = RegisterColumn((Product x) => x.Weight);
        public static readonly Accessor<Product, _Int32> ProductCategoryIDAccessor = RegisterColumn((Product x) => x.ProductCategoryID);
        public static readonly Accessor<Product, _Int32> ProductModelIDAccessor = RegisterColumn((Product x) => x.ProductModelID);
        public static readonly Accessor<Product, _DateTime> SellStartDateAccessor = RegisterColumn((Product x) => x.SellStartDate, x => x.AsDateTime());
        public static readonly Accessor<Product, _DateTime> SellEndDateAccessor = RegisterColumn((Product x) => x.SellEndDate, x => x.AsDateTime());
        public static readonly Accessor<Product, _DateTime> DiscontinuedDateAccessor = RegisterColumn((Product x) => x.DiscontinuedDate, x => x.AsDateTime());
        public static readonly Accessor<Product, _Binary> ThumbNailPhotoAccessor = RegisterColumn((Product x) => x.ThumbNailPhoto);
        public static readonly Accessor<Product, _String> ThumbnailPhotoFileNameAccessor = RegisterColumn((Product x) => x.ThumbnailPhotoFileName);

        public Product()
        {
            _primaryKey = new Key(ProductID);
            ProductCategoryKey = new ProductCategory.Key(ProductCategoryID);
            ProductModelKey = new ProductModel.Key(ProductModelID);
        }

        private Key _primaryKey;
        public override Key PrimaryKey
        {
            get { return _primaryKey; }
        }

        public ProductCategory.Key ProductCategoryKey { get; private set; }

        public ProductModel.Key ProductModelKey { get; private set; }

        [Identity(1, 1)]
        public _Int32 ProductID { get; private set; }

        [UdtName]
        public _String Name { get; private set; }

        [Nullable(false)]
        [AsNVarChar(25)]
        public _String ProductNumber { get; private set; }

        [Nullable(true)]
        [AsNVarChar(15)]
        public _String Color { get; private set; }

        [Nullable(false)]
        [AsMoney()]
        public _Decimal StandardCost { get; private set; }

        [Nullable(false)]
        [AsMoney()]
        public _Decimal ListPrice { get; private set; }

        [Nullable(true)]
        [AsNVarChar(5)]
        public _String Size { get; private set; }

        [Nullable(true)]
        [AsDecimal(8, 2)]
        public _Decimal Weight { get; private set; }

        [Nullable(true)]
        public _Int32 ProductCategoryID { get; private set; }

        [Nullable(true)]
        public _Int32 ProductModelID { get; private set; }

        [Nullable(false)]
        [AsDateTime]
        public _DateTime SellStartDate { get; private set; }

        [Nullable(true)]
        [AsDateTime]
        public _DateTime SellEndDate { get; private set; }

        [Nullable(true)]
        [AsDateTime]
        public _DateTime DiscontinuedDate { get; private set; }

        [Nullable(true)]
        [AsVarBinaryMax]
        public _Binary ThumbNailPhoto { get; private set; }

        [Nullable(true)]
        [AsNVarChar(50)]
        public _String ThumbnailPhotoFileName { get; private set; }
    }
}

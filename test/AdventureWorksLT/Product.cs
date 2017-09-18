using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class Product : BaseModel<Product.Key>
    {
        public sealed class Key : KeyBase
        {
            public Key(_Int32 productID)
            {
                ProductID = productID;
            }

            public _Int32 ProductID { get; private set; }
        }

        public static readonly Property<_Int32> _ProductID = RegisterColumn((Product x) => x.ProductID);
        public static readonly Property<_String> _Name = RegisterColumn((Product x) => x.Name);
        public static readonly Property<_String> _ProductNumber = RegisterColumn((Product x) => x.ProductNumber);
        public static readonly Property<_String> _Color = RegisterColumn((Product x) => x.Color);
        public static readonly Property<_Decimal> _StandardCost = RegisterColumn((Product x) => x.StandardCost);
        public static readonly Property<_Decimal> _ListPrice = RegisterColumn((Product x) => x.ListPrice);
        public static readonly Property<_String> _Size = RegisterColumn((Product x) => x.Size);
        public static readonly Property<_Decimal> _Weight = RegisterColumn((Product x) => x.Weight);
        public static readonly Property<_Int32> _ProductCategoryID = RegisterColumn((Product x) => x.ProductCategoryID);
        public static readonly Property<_Int32> _ProductModelID = RegisterColumn((Product x) => x.ProductModelID);
        public static readonly Property<_DateTime> _SellStartDate = RegisterColumn((Product x) => x.SellStartDate, x => x.AsDateTime());
        public static readonly Property<_DateTime> _SellEndDate = RegisterColumn((Product x) => x.SellEndDate, x => x.AsDateTime());
        public static readonly Property<_DateTime> _DiscontinuedDate = RegisterColumn((Product x) => x.DiscontinuedDate, x => x.AsDateTime());
        public static readonly Property<_Binary> _ThumbNailPhoto = RegisterColumn((Product x) => x.ThumbNailPhoto);
        public static readonly Property<_String> _ThumbnailPhotoFileName = RegisterColumn((Product x) => x.ThumbnailPhotoFileName);

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

        [Required]
        [AsNVarChar(25)]
        public _String ProductNumber { get; private set; }

        [AsNVarChar(15)]
        public _String Color { get; private set; }

        [Required]
        [AsMoney()]
        public _Decimal StandardCost { get; private set; }

        [Required]
        [AsMoney()]
        public _Decimal ListPrice { get; private set; }

        [AsNVarChar(5)]
        public _String Size { get; private set; }

        [AsDecimal(8, 2)]
        public _Decimal Weight { get; private set; }

        public _Int32 ProductCategoryID { get; private set; }

        public _Int32 ProductModelID { get; private set; }

        [Required]
        [AsDateTime]
        public _DateTime SellStartDate { get; private set; }

        [AsDateTime]
        public _DateTime SellEndDate { get; private set; }

        [AsDateTime]
        public _DateTime DiscontinuedDate { get; private set; }

        [AsVarBinaryMax]
        public _Binary ThumbNailPhoto { get; private set; }

        [AsNVarChar(50)]
        public _String ThumbnailPhotoFileName { get; private set; }
    }
}

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

        public class Ref : Model<Key>
        {
            public static readonly Mounter<_Int32> _ProductID = RegisterColumn((Ref _) => _.ProductID);

            public Ref()
            {
                _primaryKey = new Key(ProductID);
            }

            private readonly Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get { return _primaryKey; }
            }

            public _Int32 ProductID { get; private set; }
        }

        public static readonly Mounter<_Int32> _ProductID = RegisterColumn((Product _) => _.ProductID, Ref._ProductID);
        public static readonly Mounter<_String> _Name = RegisterColumn((Product _) => _.Name);
        public static readonly Mounter<_String> _ProductNumber = RegisterColumn((Product _) => _.ProductNumber);
        public static readonly Mounter<_String> _Color = RegisterColumn((Product _) => _.Color);
        public static readonly Mounter<_Decimal> _StandardCost = RegisterColumn((Product _) => _.StandardCost);
        public static readonly Mounter<_Decimal> _ListPrice = RegisterColumn((Product _) => _.ListPrice);
        public static readonly Mounter<_String> _Size = RegisterColumn((Product _) => _.Size);
        public static readonly Mounter<_Decimal> _Weight = RegisterColumn((Product _) => _.Weight);
        public static readonly Mounter<_Int32> _ProductCategoryID = RegisterColumn((Product _) => _.ProductCategoryID);
        public static readonly Mounter<_Int32> _ProductModelID = RegisterColumn((Product _) => _.ProductModelID);
        public static readonly Mounter<_DateTime> _SellStartDate = RegisterColumn((Product _) => _.SellStartDate, x => x.AsDateTime());
        public static readonly Mounter<_DateTime> _SellEndDate = RegisterColumn((Product _) => _.SellEndDate, x => x.AsDateTime());
        public static readonly Mounter<_DateTime> _DiscontinuedDate = RegisterColumn((Product _) => _.DiscontinuedDate, x => x.AsDateTime());
        public static readonly Mounter<_Binary> _ThumbNailPhoto = RegisterColumn((Product _) => _.ThumbNailPhoto);
        public static readonly Mounter<_String> _ThumbnailPhotoFileName = RegisterColumn((Product _) => _.ThumbnailPhotoFileName);

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

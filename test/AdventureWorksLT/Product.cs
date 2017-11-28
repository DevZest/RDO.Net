using DevZest.Data;
using DevZest.Data.Annotations;
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
            public static readonly Mounter<_Int32> _ProductID;

            static Ref()
            {
                _ProductID = RegisterColumn((Ref _) => _.ProductID);
            }

            private Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get
                {
                    if (_primaryKey == null)
                        _primaryKey = new Key(ProductID);
                    return _primaryKey;
                }
            }

            public _Int32 ProductID { get; private set; }
        }

        public class Lookup : ModelExtension
        {
            static Lookup()
            {
                RegisterColumn((Lookup _) => _.Name, _Name);
                RegisterColumn((Lookup _) => _.ProductNumber, _ProductNumber);
            }

            public _String Name { get; private set; }

            public _String ProductNumber { get; private set; }
        }

        public static readonly Mounter<_String> _Name;
        public static readonly Mounter<_String> _ProductNumber;
        public static readonly Mounter<_String> _Color;
        public static readonly Mounter<_Decimal> _StandardCost;
        public static readonly Mounter<_Decimal> _ListPrice;
        public static readonly Mounter<_String> _Size;
        public static readonly Mounter<_Decimal> _Weight;
        public static readonly Mounter<_DateTime> _SellStartDate;
        public static readonly Mounter<_DateTime> _SellEndDate;
        public static readonly Mounter<_DateTime> _DiscontinuedDate;
        public static readonly Mounter<_Binary> _ThumbNailPhoto;
        public static readonly Mounter<_String> _ThumbnailPhotoFileName;

        static Product()
        {
            RegisterColumn((Product _) => _.ProductID, Ref._ProductID);
            _Name = RegisterColumn((Product _) => _.Name);
            _ProductNumber = RegisterColumn((Product _) => _.ProductNumber);
            _Color = RegisterColumn((Product _) => _.Color);
            _StandardCost = RegisterColumn((Product _) => _.StandardCost);
            _ListPrice = RegisterColumn((Product _) => _.ListPrice);
            _Size = RegisterColumn((Product _) => _.Size);
            _Weight = RegisterColumn((Product _) => _.Weight);
            RegisterColumn((Product _) => _.ProductCategoryID, AdventureWorksLT.ProductCategory.Ref._ProductCategoryID);
            RegisterColumn((Product _) => _.ProductModelID, AdventureWorksLT.ProductModel.Ref._ProductModelID);
            _SellStartDate = RegisterColumn((Product _) => _.SellStartDate);
            _SellEndDate = RegisterColumn((Product _) => _.SellEndDate);
            _DiscontinuedDate = RegisterColumn((Product _) => _.DiscontinuedDate);
            _ThumbNailPhoto = RegisterColumn((Product _) => _.ThumbNailPhoto);
            _ThumbnailPhotoFileName = RegisterColumn((Product _) => _.ThumbnailPhotoFileName);
        }

        private Key _primaryKey;
        public override Key PrimaryKey
        {
            get
            {
                if (_primaryKey == null)
                    _primaryKey = new Key(ProductID);
                return _primaryKey;
            }
        }

        private ProductCategory.Key _productCategory;
        public ProductCategory.Key ProductCategory
        {
            get
            {
                if (_productCategory == null)
                    _productCategory = new ProductCategory.Key(ProductCategoryID);
                return _productCategory;
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

using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class Product : BaseModel<Product.Key>
    {
        public sealed class Key : PrimaryKey
        {
            public Key(_Int32 productID)
            {
                ProductID = productID;
            }

            public _Int32 ProductID { get; private set; }
        }

        public class Ref : Model<Key>
        {
            static Ref()
            {
                RegisterColumn((Ref _) => _.ProductID, _ProductID);
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

        public class Lookup : ModelExtender
        {
            static Lookup()
            {
                RegisterColumn((Lookup _) => _.Name, _Name);
                RegisterColumn((Lookup _) => _.ProductNumber, _ProductNumber);
            }

            public _String Name { get; private set; }

            public _String ProductNumber { get; private set; }
        }

        public static readonly Mounter<_Int32> _ProductID;
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
            _ProductID = RegisterColumn((Product _) => _.ProductID);
            _Name = RegisterColumn((Product _) => _.Name);
            _ProductNumber = RegisterColumn((Product _) => _.ProductNumber);
            _Color = RegisterColumn((Product _) => _.Color);
            _StandardCost = RegisterColumn((Product _) => _.StandardCost);
            _ListPrice = RegisterColumn((Product _) => _.ListPrice);
            _Size = RegisterColumn((Product _) => _.Size);
            _Weight = RegisterColumn((Product _) => _.Weight);
            RegisterColumn((Product _) => _.ProductCategoryID, AdventureWorksLT.ProductCategory._ProductCategoryID);
            RegisterColumn((Product _) => _.ProductModelID, AdventureWorksLT.ProductModel._ProductModelID);
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
        [DbColumn(Description = "Primary key for Product records.")]
        public _Int32 ProductID { get; private set; }

        [UdtName]
        [DbColumn(Description = "Name of the product.")]
        public _String Name { get; private set; }

        [Required]
        [AsNVarChar(25)]
        [DbColumn(Description = "Unique product identification number.")]
        public _String ProductNumber { get; private set; }

        [AsNVarChar(15)]
        [DbColumn(Description = "Product color.")]
        public _String Color { get; private set; }

        [Required]
        [AsMoney()]
        [DbColumn(Description = "Standard cost of the product.")]
        public _Decimal StandardCost { get; private set; }

        [Required]
        [AsMoney()]
        [DbColumn(Description = "Selling price.")]
        public _Decimal ListPrice { get; private set; }

        [AsNVarChar(5)]
        [DbColumn(Description = "Product size.")]
        public _String Size { get; private set; }

        [AsDecimal(8, 2)]
        [DbColumn(Description = "Product weight.")]
        public _Decimal Weight { get; private set; }

        [DbColumn(Description = "Product is a member of this product category. Foreign key to ProductCategory.ProductCategoryID.")]
        public _Int32 ProductCategoryID { get; private set; }

        [DbColumn(Description = "Product is a member of this product model. Foreign key to ProductModel.ProductModelID.")]
        public _Int32 ProductModelID { get; private set; }

        [Required]
        [AsDateTime]
        [DbColumn(Description = "Date the product was available for sale.")]
        public _DateTime SellStartDate { get; private set; }

        [AsDateTime]
        [DbColumn(Description = "Date the product was no longer available for sale.")]
        public _DateTime SellEndDate { get; private set; }

        [AsDateTime]
        [DbColumn(Description = "Date the product was discontinued.")]
        public _DateTime DiscontinuedDate { get; private set; }

        [AsVarBinaryMax]
        [DbColumn(Description = "Small image of the product.")]
        public _Binary ThumbNailPhoto { get; private set; }

        [AsNVarChar(50)]
        [DbColumn(Description = "Small image file name.")]
        public _String ThumbnailPhotoFileName { get; private set; }
    }
}

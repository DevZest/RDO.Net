using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class Product : BaseModel<Product.PK>
    {
        [DbPrimaryKey("PK_Product_ProductID", Description = "Primary key (clustered) constraint")]
        public sealed class PK : PrimaryKey
        {
            public static IDataValues Const(int productID)
            {
                return DataValues.Create(_Int32.Const(productID));
            }

            public PK(_Int32 productID)
                : base(productID)
            {
            }

            public _Int32 ProductID
            {
                get { return GetColumn<_Int32>(0); }
            }
        }

        public sealed class Key : Model<PK>
        {
            static Key()
            {
                RegisterColumn((Key _) => _.ProductID, _ProductID);
            }

            protected override PK CreatePrimaryKey()
            {
                return new PK(ProductID);
            }

            public _Int32 ProductID { get; private set; }
        }

        public class Ref : Ref<PK>
        {
            static Ref()
            {
                Register((Ref _) => _.ProductID, _ProductID);
            }

            public _Int32 ProductID { get; private set; }

            protected override PK GetForeignKey()
            {
                return new PK(ProductID);
            }
        }

        public class Lookup : Projection
        {
            static Lookup()
            {
                Register((Lookup _) => _.Name, _Name);
                Register((Lookup _) => _.ProductNumber, _ProductNumber);
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

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ProductID);
        }

        private ProductCategory.PK _fk_productCategory;
        public ProductCategory.PK FK_ProductCategory
        {
            get { return _fk_productCategory ?? (_fk_productCategory = new ProductCategory.PK(ProductCategoryID)); }
        }

        private ProductModel.PK _fk_productModel;
        public ProductModel.PK FK_ProductModel
        {
            get { return _fk_productModel ?? (_fk_productModel = new ProductModel.PK(ProductModelID)); }
        }

        [Identity(1, 1)]
        [DbColumn(Description = "Primary key for Product records.")]
        public _Int32 ProductID { get; private set; }

        [UdtName]
        [DbColumn(Description = "Name of the product.")]
        [Unique(Name = "AK_Product_Name", Description = "Unique nonclustered constraint.")]
        public _String Name { get; private set; }

        [Required]
        [AsNVarChar(25)]
        [DbColumn(Description = "Unique product identification number.")]
        [Unique(Name = "AK_Product_ProductNumber", Description = "Unique nonclustered constraint.")]
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

        private _Boolean _ck_Product_ListPrice;
        [Check(typeof(UserMessages), nameof(UserMessages.CK_Product_ListPrice), Name = nameof(CK_Product_ListPrice), Description = "Check constraint [ListPrice] >= (0.00)")]
        private _Boolean CK_Product_ListPrice
        {
            get { return _ck_Product_ListPrice ?? (_ck_Product_ListPrice = ListPrice >= _Decimal.Const(0)); }
        }

        private _Boolean _ck_Product_SellEndDate;
        [Check(typeof(UserMessages), nameof(UserMessages.CK_Product_SellEndDate), Name = nameof(CK_Product_SellEndDate), Description = "Check constraint [SellEndDate] >= [SellStartDate] OR [SellEndDate] IS NULL")]
        private _Boolean CK_Product_SellEndDate
        {
            get { return _ck_Product_SellEndDate ?? (_ck_Product_SellEndDate = SellEndDate >= SellStartDate | SellEndDate.IsNull()); }
        }

        private _Boolean _ck_Product_StandardCost;
        [Check(typeof(UserMessages), nameof(UserMessages.CK_Product_StandardCost), Name = nameof(CK_Product_StandardCost), Description = "Check constraint [StandardCost] >= (0.00)")]
        private _Boolean CK_Product_StandardCost
        {
            get { return _ck_Product_StandardCost ?? (_ck_Product_StandardCost = StandardCost >= _Decimal.Const(0)); }
        }

        private _Boolean _ck_Product_Weight;
        [Check(typeof(UserMessages), nameof(UserMessages.CK_Product_Weight), Name = nameof(CK_Product_Weight), Description = "Check constraint [Weight] >= (0.00)")]
        private _Boolean CK_Product_Weight
        {
            get { return _ck_Product_Weight ?? (_ck_Product_Weight = Weight >= _Decimal.Const(0)); }
        }
    }
}
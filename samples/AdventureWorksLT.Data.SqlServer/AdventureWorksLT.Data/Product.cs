using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    [CheckConstraint(nameof(CK_Product_ListPrice), typeof(UserMessages), nameof(UserMessages.CK_Product_ListPrice), Description = "Check constraint [ListPrice] >= (0.00)")]
    [CheckConstraint(nameof(CK_Product_SellEndDate), typeof(UserMessages), nameof(UserMessages.CK_Product_SellEndDate), Description = "Check constraint [SellEndDate] >= [SellStartDate] OR [SellEndDate] IS NULL")]
    [CheckConstraint(nameof(CK_Product_Weight), typeof(UserMessages), nameof(UserMessages.CK_Product_Weight), Description = "Check constraint [Weight] >= (0.00)")]
    [UniqueConstraint(nameof(AK_Product_Name), Description = "Unique nonclustered constraint.")]
    [UniqueConstraint(nameof(AK_Product_ProductNumber), Description = "Unique nonclustered constraint.")]
    public class Product : BaseModel<Product.PK>
    {
        [DbPrimaryKey("PK_Product_ProductID", Description = "Primary key (clustered) constraint")]
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 productID)
                : base(productID)
            {
            }
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                Register((Key _) => _.ProductID, _ProductID);
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

            protected override PK CreateForeignKey()
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

        public static readonly Mounter<_Int32> _ProductID = RegisterColumn((Product _) => _.ProductID);
        public static readonly Mounter<_String> _Name = RegisterColumn((Product _) => _.Name);
        public static readonly Mounter<_String> _ProductNumber = RegisterColumn((Product _) => _.ProductNumber);
        public static readonly Mounter<_String> _Color = RegisterColumn((Product _) => _.Color);
        public static readonly Mounter<_Decimal> _StandardCost = RegisterColumn((Product _) => _.StandardCost);
        public static readonly Mounter<_Decimal> _ListPrice = RegisterColumn((Product _) => _.ListPrice);
        public static readonly Mounter<_String> _Size = RegisterColumn((Product _) => _.Size);
        public static readonly Mounter<_Decimal> _Weight = RegisterColumn((Product _) => _.Weight);
        public static readonly Mounter<_Int32> _ProductCategoryID = RegisterColumn((Product _) => _.ProductCategoryID);
        public static readonly Mounter<_Int32> _ProductModelID = RegisterColumn((Product _) => _.ProductModelID);
        public static readonly Mounter<_DateTime> _SellStartDate = RegisterColumn((Product _) => _.SellStartDate);
        public static readonly Mounter<_DateTime> _SellEndDate = RegisterColumn((Product _) => _.SellEndDate);
        public static readonly Mounter<_DateTime> _DiscontinuedDate = RegisterColumn((Product _) => _.DiscontinuedDate);
        public static readonly Mounter<_Binary> _ThumbNailPhoto = RegisterColumn((Product _) => _.ThumbNailPhoto);
        public static readonly Mounter<_String> _ThumbnailPhotoFileName = RegisterColumn((Product _) => _.ThumbnailPhotoFileName);

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

        [Identity]
        [DbColumn(Description = "Primary key for Product records.")]
        public _Int32 ProductID { get; private set; }

        [UdtName]
        [DbColumn(Description = "Name of the product.")]
        public _String Name { get; private set; }

        [Required]
        [SqlNVarChar(25)]
        [DbColumn(Description = "Unique product identification number.")]
        public _String ProductNumber { get; private set; }

        [SqlNVarChar(15)]
        [DbColumn(Description = "Product color.")]
        public _String Color { get; private set; }

        [Required]
        [SqlMoney()]
        [DbColumn(Description = "Standard cost of the product.")]
        public _Decimal StandardCost { get; private set; }

        [Required]
        [SqlMoney()]
        [DbColumn(Description = "Selling price.")]
        public _Decimal ListPrice { get; private set; }

        [SqlNVarChar(5)]
        [DbColumn(Description = "Product size.")]
        public _String Size { get; private set; }

        [SqlDecimal(8, 2)]
        [DbColumn(Description = "Product weight.")]
        public _Decimal Weight { get; private set; }

        [DbColumn(Description = "Product is a member of this product category. Foreign key to ProductCategory.ProductCategoryID.")]
        public _Int32 ProductCategoryID { get; private set; }

        [DbColumn(Description = "Product is a member of this product model. Foreign key to ProductModel.ProductModelID.")]
        public _Int32 ProductModelID { get; private set; }

        [Required]
        [SqlDateTime]
        [DbColumn(Description = "Date the product was available for sale.")]
        public _DateTime SellStartDate { get; private set; }

        [SqlDateTime]
        [DbColumn(Description = "Date the product was no longer available for sale.")]
        public _DateTime SellEndDate { get; private set; }

        [SqlDateTime]
        [DbColumn(Description = "Date the product was discontinued.")]
        public _DateTime DiscontinuedDate { get; private set; }

        [SqlVarBinaryMax]
        [DbColumn(Description = "Small image of the product.")]
        public _Binary ThumbNailPhoto { get; private set; }

        [SqlNVarChar(50)]
        [DbColumn(Description = "Small image file name.")]
        public _String ThumbnailPhotoFileName { get; private set; }

        [_CheckConstraint]
        private _Boolean CK_Product_ListPrice
        {
            get { return ListPrice >= _Decimal.Const(0); }
        }

        [_CheckConstraint]
        private _Boolean CK_Product_SellEndDate
        {
            get { return SellEndDate >= SellStartDate | SellEndDate.IsNull(); }
        }

        private _Boolean CK_Product_StandardCost
        {
            get { return StandardCost >= _Decimal.Const(0); }
        }

        [_CheckConstraint]
        private _Boolean CK_Product_Weight
        {
            get { return Weight >= _Decimal.Const(0); }
        }

        [_UniqueConstraint]
        private ColumnSort[] AK_Product_Name => new ColumnSort[] { Name };

        [_UniqueConstraint]
        private ColumnSort[] AK_Product_ProductNumber => new ColumnSort[] { ProductNumber };
    }
}
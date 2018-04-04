using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrderDetail : BaseModel<SalesOrderDetail.Key>
    {
        [DbConstraint("PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID", Description = "Clustered index created by a primary key constraint.")]
        public sealed class Key : PrimaryKey
        {
            public Key(_Int32 salesOrderID, _Int32 salesOrderDetailID)
            {
                SalesOrderID = salesOrderID;
                SalesOrderDetailID = salesOrderDetailID;
            }

            public _Int32 SalesOrderID { get; private set; }

            public _Int32 SalesOrderDetailID { get; private set; }
        }

        public class Ref : Model<Key>
        {
            static Ref()
            {
                RegisterColumn((Ref _) => _.SalesOrderID, AdventureWorksLT.SalesOrder._SalesOrderID);
                RegisterColumn((Ref _) => _.SalesOrderDetailID, _SalesOrderDetailID);
            }

            private Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get { return _primaryKey ?? (_primaryKey = new Key(SalesOrderID, SalesOrderDetailID)); }
            }

            public _Int32 SalesOrderID { get; private set; }

            public _Int32 SalesOrderDetailID { get; private set; }
        }

        public static readonly Mounter<_Int32> _SalesOrderDetailID;
        public static readonly Mounter<_Int16> _OrderQty;
        public static readonly Mounter<_Decimal> _UnitPrice;
        public static readonly Mounter<_Decimal> _UnitPriceDiscount;
        public static readonly Mounter<_Decimal> _LineTotal;

        static SalesOrderDetail()
        {
            RegisterColumn((SalesOrderDetail _) => _.SalesOrderID, AdventureWorksLT.SalesOrder._SalesOrderID);
            _SalesOrderDetailID = RegisterColumn((SalesOrderDetail _) => _.SalesOrderDetailID);
            _OrderQty = RegisterColumn((SalesOrderDetail _) => _.OrderQty);
            RegisterColumn((SalesOrderDetail _) => _.ProductID, AdventureWorksLT.Product._ProductID);
            _UnitPrice = RegisterColumn((SalesOrderDetail _) => _.UnitPrice);
            _UnitPriceDiscount = RegisterColumn((SalesOrderDetail _) => _.UnitPriceDiscount);
            _LineTotal = RegisterColumn((SalesOrderDetail _) => _.LineTotal);
        }

        public SalesOrderDetail()
        {
        }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get { return _primaryKey ?? (_primaryKey = new Key(SalesOrderID, SalesOrderDetailID)); }
        }

        private SalesOrderHeader.Key _salesOrderHeader;
        public SalesOrderHeader.Key SalesOrderHeader
        {
            get { return _salesOrderHeader ?? (_salesOrderHeader = new SalesOrderHeader.Key(SalesOrderID)); }
        }

        private Product.Key _product;
        public Product.Key Product
        {
            get { return _product ?? (_product = new Product.Key(ProductID)); }
        }

        [DbColumn(Description = "Primary key. Foreign key to SalesOrderHeader.SalesOrderID.")]
        public _Int32 SalesOrderID { get; private set; }

        [Identity(1, 1)]
        [DbColumn(Description = "Primary key. One incremental unique number per product sold.")]
        public _Int32 SalesOrderDetailID { get; private set; }

        [Required]
        [DbColumn(Description = "Quantity ordered per product.")]
        public _Int16 OrderQty { get; private set; }

        [Required]
        [DbColumn(Description = "Product sold to customer. Foreign key to Product.ProductID.")]
        [DbIndex("IX_SalesOrderDetail_ProductID", Description = "Nonclustered index.")]
        public _Int32 ProductID { get; private set; }

        [Required]
        [AsMoney]
        [DbColumn(Description = "Selling price of a single product.")]
        public _Decimal UnitPrice { get; private set; }

        [Required]
        [AsMoney]
        [DefaultValue(typeof(decimal), "0", Name = "DF_SalesOrderDetail_UnitPriceDiscount")]
        [DbColumn(Description = "Discount amount.")]
        public _Decimal UnitPriceDiscount { get; private set; }

        [Required]
        [AsMoney]
        [DbColumn(Description = "Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.")]
        public _Decimal LineTotal { get; private set; }

        [Computation]
        private void ComputeLineTotal()
        {
            LineTotal.ComputedAs((UnitPrice * (_Decimal.Const(1) - UnitPriceDiscount) * OrderQty).IfNull(_Decimal.Const(0)));
        }

        private _Boolean _ck_SalesOrderDetail_OrderQty;
        [Check(typeof(UserMessages), nameof(UserMessages.CK_SalesOrderDetail_OrderQty), Name = nameof(CK_SalesOrderDetail_OrderQty), Description = "Check constraint [OrderQty] > (0)")]
        private _Boolean CK_SalesOrderDetail_OrderQty
        {
            get { return _ck_SalesOrderDetail_OrderQty ?? (_ck_SalesOrderDetail_OrderQty = OrderQty > _Decimal.Const(0)); }
        }

        private _Boolean _ck_SalesOrderDetail_UnitPrice;
        [Check(typeof(UserMessages), nameof(UserMessages.CK_SalesOrderDetail_UnitPrice), Name = nameof(CK_SalesOrderDetail_UnitPrice), Description = "heck constraint [UnitPrice] >= (0.00)")]
        private _Boolean CK_SalesOrderDetail_UnitPrice
        {
            get { return _ck_SalesOrderDetail_UnitPrice ?? (_ck_SalesOrderDetail_UnitPrice = UnitPrice >= _Decimal.Const(0)); }
        }

        private _Boolean _ck_SalesOrderDetail_UnitPriceDiscount;
        [Check(typeof(UserMessages), nameof(UserMessages.CK_SalesOrderDetail_UnitPriceDiscount), Name = nameof(CK_SalesOrderDetail_UnitPriceDiscount), Description = "Check constraint [UnitPriceDiscount] >= (0.00)")]
        private _Boolean CK_SalesOrderDetail_UnitPriceDiscount
        {
            get { return _ck_SalesOrderDetail_UnitPriceDiscount ?? (_ck_SalesOrderDetail_UnitPriceDiscount = UnitPriceDiscount >= _Decimal.Const(0)); }
        }
    }
}

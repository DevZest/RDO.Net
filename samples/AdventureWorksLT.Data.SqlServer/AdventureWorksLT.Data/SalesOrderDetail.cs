using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    [Computation(nameof(ComputeLineTotal))]
    [CheckConstraint(nameof(CK_SalesOrderDetail_OrderQty), typeof(UserMessages), nameof(UserMessages.CK_SalesOrderDetail_OrderQty), Description = "Check constraint [OrderQty] > (0)")]
    [CheckConstraint(nameof(CK_SalesOrderDetail_UnitPrice), typeof(UserMessages), nameof(UserMessages.CK_SalesOrderDetail_UnitPrice), Description = "Check constraint [UnitPrice] >= (0.00)")]
    [CheckConstraint(nameof(CK_SalesOrderDetail_UnitPriceDiscount), typeof(UserMessages), nameof(UserMessages.CK_SalesOrderDetail_UnitPriceDiscount), Description = "Check constraint [UnitPriceDiscount] >= (0.00)")]
    [DbIndex(nameof(IX_SalesOrderDetail_ProductID), Description = "Nonclustered index.")]
    public class SalesOrderDetail : BaseModel<SalesOrderDetail.PK>
    {
        [DbPrimaryKey("PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID", Description = "Clustered index created by a primary key constraint.")]
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 salesOrderID, _Int32 salesOrderDetailID)
                : base(salesOrderID, salesOrderDetailID)
            {
            }
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                Register((Key _) => _.SalesOrderID, _SalesOrderID);
                Register((Key _) => _.SalesOrderDetailID, _SalesOrderDetailID);
            }

            protected override PK CreatePrimaryKey()
            {
                return new PK(SalesOrderID, SalesOrderDetailID);
            }

            public _Int32 SalesOrderID { get; private set; }

            public _Int32 SalesOrderDetailID { get; private set; }
        }

        public static readonly Mounter<_Int32> _SalesOrderID = RegisterColumn((SalesOrderDetail _) => _.SalesOrderID);
        public static readonly Mounter<_Int32> _SalesOrderDetailID = RegisterColumn((SalesOrderDetail _) => _.SalesOrderDetailID);
        public static readonly Mounter<_Int16> _OrderQty = RegisterColumn((SalesOrderDetail _) => _.OrderQty);
        public static readonly Mounter<_Int32> _ProductID = RegisterColumn((SalesOrderDetail _) => _.ProductID);
        public static readonly Mounter<_Decimal> _UnitPrice = RegisterColumn((SalesOrderDetail _) => _.UnitPrice);
        public static readonly Mounter<_Decimal> _UnitPriceDiscount = RegisterColumn((SalesOrderDetail _) => _.UnitPriceDiscount);
        public static readonly Mounter<_Decimal> _LineTotal = RegisterColumn((SalesOrderDetail _) => _.LineTotal);

        public SalesOrderDetail()
        {
        }

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(SalesOrderID, SalesOrderDetailID);
        }

        private SalesOrderHeader.PK _fk_salesOrderHeader;
        public SalesOrderHeader.PK FK_SalesOrderHeader
        {
            get { return _fk_salesOrderHeader ?? (_fk_salesOrderHeader = new SalesOrderHeader.PK(SalesOrderID)); }
        }

        private Product.PK _fk_product;
        public Product.PK FK_Product
        {
            get { return _fk_product ?? (_fk_product = new Product.PK(ProductID)); }
        }

        [DbColumn(Description = "Primary key. Foreign key to SalesOrderHeader.SalesOrderID.")]
        public _Int32 SalesOrderID { get; private set; }

        [Identity]
        [DbColumn(Description = "Primary key. One incremental unique number per product sold.")]
        public _Int32 SalesOrderDetailID { get; private set; }

        [Required]
        [DbColumn(Description = "Quantity ordered per product.")]
        public _Int16 OrderQty { get; private set; }

        [Required]
        [DbColumn(Description = "Product sold to customer. Foreign key to Product.ProductID.")]
        public _Int32 ProductID { get; private set; }

        [Required]
        [SqlMoney]
        [DbColumn(Description = "Selling price of a single product.")]
        public _Decimal UnitPrice { get; private set; }

        [Required]
        [SqlMoney]
        [DefaultValue(typeof(decimal), "0", Name = "DF_SalesOrderDetail_UnitPriceDiscount")]
        [DbColumn(Description = "Discount amount.")]
        public _Decimal UnitPriceDiscount { get; private set; }

        [Required]
        [SqlMoney]
        [DbColumn(Description = "Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.")]
        public _Decimal LineTotal { get; private set; }

        [_Computation]
        private void ComputeLineTotal()
        {
            LineTotal.ComputedAs((UnitPrice * (_Decimal.Const(1) - UnitPriceDiscount) * OrderQty).IfNull(_Decimal.Const(0)));
        }

        [_CheckConstraint]
        private _Boolean CK_SalesOrderDetail_OrderQty
        {
            get { return OrderQty > _Decimal.Const(0); }
        }

        [_CheckConstraint]
        private _Boolean CK_SalesOrderDetail_UnitPrice
        {
            get { return UnitPrice >= _Decimal.Const(0); }
        }

        [_CheckConstraint]
        private _Boolean CK_SalesOrderDetail_UnitPriceDiscount
        {
            get { return UnitPriceDiscount >= _Decimal.Const(0); }
        }

        [_DbIndex]
        private ColumnSort[] IX_SalesOrderDetail_ProductID => new ColumnSort[] { ProductID };
    }
}

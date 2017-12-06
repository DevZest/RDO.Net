using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrderDetail : BaseModel<SalesOrderDetail.Key>
    {
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
                get
                {
                    if (_primaryKey == null)
                        _primaryKey = new Key(SalesOrderID, SalesOrderDetailID);
                    return _primaryKey;
                }
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
            LineTotal.ComputedAs((UnitPrice * (_Decimal.Const(1) - UnitPriceDiscount) * OrderQty).IfNull(_Decimal.Const(0)));
        }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get
            {
                if (_primaryKey == null)
                    _primaryKey = new Key(SalesOrderID, SalesOrderDetailID);
                return _primaryKey;
            }
        }

        private SalesOrder.Key _salesOrder;
        public SalesOrder.Key SalesOrder
        {
            get
            {
                if (_salesOrder == null)
                    _salesOrder = new SalesOrder.Key(SalesOrderID);
                return _salesOrder;
            }
        }

        private Product.Key _product;
        public Product.Key Product
        {
            get
            {
                if (_product == null)
                    _product = new Product.Key(ProductID);
                return _product;
            }
        }

        [Description("Primary key. Foreign key to SalesOrderHeader.SalesOrderID.")]
        public _Int32 SalesOrderID { get; private set; }

        [Identity(1, 1)]
        [Description("Primary key. One incremental unique number per product sold.")]
        public _Int32 SalesOrderDetailID { get; private set; }

        [Required]
        [Description("Quantity ordered per product.")]
        public _Int16 OrderQty { get; private set; }

        [Required]
        [Description("Product sold to customer. Foreign key to Product.ProductID.")]
        public _Int32 ProductID { get; private set; }

        [Required]
        [AsMoney]
        [Description("Selling price of a single product.")]
        public _Decimal UnitPrice { get; private set; }

        [Required]
        [AsMoney]
        [DefaultValue(typeof(decimal), "0")]
        [Description("Discount amount.")]
        public _Decimal UnitPriceDiscount { get; private set; }

        [Required]
        [AsMoney]
        [Description("Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.")]
        public _Decimal LineTotal { get; private set; }
    }
}

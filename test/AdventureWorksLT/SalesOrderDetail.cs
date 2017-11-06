using DevZest.Data;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrderDetail : BaseModel<SalesOrderDetail.Key>
    {
        public sealed class Key : KeyBase
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
            public static readonly Mounter<_Int32> _SalesOrderDetailID;

            static Ref()
            {
                RegisterColumn((Ref _) => _.SalesOrderID, AdventureWorksLT.SalesOrder.Ref._SalesOrderID);
                _SalesOrderDetailID = RegisterColumn((Ref _) => _.SalesOrderDetailID);
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

        public static readonly Mounter<_Int16> _OrderQty;
        public static readonly Mounter<_Decimal> _UnitPrice;
        public static readonly Mounter<_Decimal> _UnitPriceDiscount;
        public static readonly Mounter<_Decimal> _LineTotal;

        static SalesOrderDetail()
        {
            RegisterColumn((SalesOrderDetail _) => _.SalesOrderID, AdventureWorksLT.SalesOrder.Ref._SalesOrderID);
            RegisterColumn((SalesOrderDetail _) => _.SalesOrderDetailID, Ref._SalesOrderDetailID);
            _OrderQty = RegisterColumn((SalesOrderDetail _) => _.OrderQty);
            RegisterColumn((SalesOrderDetail _) => _.ProductID, AdventureWorksLT.Product.Ref._ProductID);
            _UnitPrice = RegisterColumn((SalesOrderDetail _) => _.UnitPrice);
            _UnitPriceDiscount = RegisterColumn((SalesOrderDetail _) => _.UnitPriceDiscount, x => x.SetDefault(0));
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

        public _Int32 SalesOrderID { get; private set; }

        [Identity(1, 1)]
        public _Int32 SalesOrderDetailID { get; private set; }

        [Required]
        public _Int16 OrderQty { get; private set; }

        [Required]
        public _Int32 ProductID { get; private set; }

        [Required]
        [AsMoney]
        public _Decimal UnitPrice { get; private set; }

        [Required]
        [AsMoney]
        public _Decimal UnitPriceDiscount { get; private set; }

        [Required]
        [AsMoney]
        public _Decimal LineTotal { get; private set; }
    }
}

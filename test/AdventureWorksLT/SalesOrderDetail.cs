using DevZest.Data;
using DevZest.Data.SqlServer;
using System;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrderDetail : BaseModel<SalesOrderDetail.Key>
    {
        public sealed class Key : ModelKey
        {
            public Key(_Int32 salesOrderID, _Int32 salesOrderDetailID)
            {
                SalesOrderID = salesOrderID;
                SalesOrderDetailID = salesOrderDetailID;
            }

            public _Int32 SalesOrderID { get; private set; }

            public _Int32 SalesOrderDetailID { get; private set; }
        }

        public static readonly Accessor<SalesOrderDetail, _Int32> SalesOrderIDAccessor = RegisterColumn((SalesOrderDetail x) => x.SalesOrderID);
        public static readonly Accessor<SalesOrderDetail, _Int32> SalesOrderDetailIDAccessor = RegisterColumn((SalesOrderDetail x) => x.SalesOrderDetailID);
        public static readonly Accessor<SalesOrderDetail, _Int16> OrderQtyAccessor = RegisterColumn((SalesOrderDetail x) => x.OrderQty);
        public static readonly Accessor<SalesOrderDetail, _Int32> ProductIDAccessor = RegisterColumn((SalesOrderDetail x) => x.ProductID);
        public static readonly Accessor<SalesOrderDetail, _Decimal> UnitPriceAccessor = RegisterColumn((SalesOrderDetail x) => x.UnitPrice);
        public static readonly Accessor<SalesOrderDetail, _Decimal> UnitPriceDiscountAccessor = RegisterColumn((SalesOrderDetail x) => x.UnitPriceDiscount, x => x.DefaultValue(0));
        public static readonly Accessor<SalesOrderDetail, _Decimal> LineTotalAccessor = RegisterColumn((SalesOrderDetail x) => x.LineTotal);

        public SalesOrderDetail()
        {
            _primaryKey = new Key(SalesOrderID, SalesOrderDetailID);
            SalesOrderKey = new SalesOrder.Key(SalesOrderID);
            ProductKey = new Product.Key(ProductID);
        }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get { return _primaryKey; }
        }

        public SalesOrder.Key SalesOrderKey { get; private set; }

        public Product.Key ProductKey { get; private set; }

        public _Int32 SalesOrderID { get; private set; }

        [Identity(1, 1)]
        public _Int32 SalesOrderDetailID { get; private set; }

        [Nullable(false)]
        public _Int16 OrderQty { get; private set; }

        [Nullable(false)]
        public _Int32 ProductID { get; private set; }

        [Nullable(false)]
        [AsMoney]
        public _Decimal UnitPrice { get; private set; }

        [Nullable(false)]
        [AsMoney]
        public _Decimal UnitPriceDiscount { get; private set; }

        [Nullable(false)]
        [AsMoney]
        public _Decimal LineTotal { get; private set; }
    }
}

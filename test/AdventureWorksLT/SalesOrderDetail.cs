﻿using DevZest.Data;
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
            public static readonly Mounter<_Int32> _SalesOrderID = RegisterColumn((Ref _) => _.SalesOrderID);
            public static readonly Mounter<_Int32> _SalesOrderDetailID = RegisterColumn((Ref _) => _.SalesOrderDetailID);

            public Ref()
            {
                _primaryKey = new Key(SalesOrderID, SalesOrderDetailID);
            }

            private readonly Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get { return _primaryKey; }
            }

            public _Int32 SalesOrderID { get; private set; }

            public _Int32 SalesOrderDetailID { get; private set; }
        }

        public static readonly Mounter<_Int32> _SalesOrderID = RegisterColumn((SalesOrderDetail _) => _.SalesOrderID, Ref._SalesOrderID);
        public static readonly Mounter<_Int32> _SalesOrderDetailID = RegisterColumn((SalesOrderDetail _) => _.SalesOrderDetailID, Ref._SalesOrderDetailID);
        public static readonly Mounter<_Int16> _OrderQty = RegisterColumn((SalesOrderDetail _) => _.OrderQty);
        public static readonly Mounter<_Int32> _ProductID = RegisterColumn((SalesOrderDetail _) => _.ProductID);
        public static readonly Mounter<_Decimal> _UnitPrice = RegisterColumn((SalesOrderDetail _) => _.UnitPrice);
        public static readonly Mounter<_Decimal> _UnitPriceDiscount = RegisterColumn((SalesOrderDetail _) => _.UnitPriceDiscount, x => x.DefaultValue(0));
        public static readonly Mounter<_Decimal> _LineTotal = RegisterColumn((SalesOrderDetail _) => _.LineTotal);

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

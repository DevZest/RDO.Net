using DevZest.Data;
using DevZest.Data.SqlServer;
using System;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrder : BaseModel<SalesOrder.Key>
    {
        public sealed class Key : KeyBase
        {
            public Key(_Int32 salesOrderID)
            {
                SalesOrderID = salesOrderID;
            }

            public _Int32 SalesOrderID { get; private set; }
        }
        
        public class Ref : Model<Key>
        {
            public static readonly Mounter<_Int32> _SalesOrderID = RegisterColumn((Ref _) => _.SalesOrderID);

            public Ref()
            {
                _primaryKey = new Key(SalesOrderID);
            }

            private readonly Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get { return _primaryKey; }
            }

            public _Int32 SalesOrderID { get; private set; }
        }

        public static readonly Mounter<_Int32> _SalesOrderID = RegisterColumn((SalesOrder _) => _.SalesOrderID, Ref._SalesOrderID);
        public static readonly Mounter<_Byte> _RevisionNumber = RegisterColumn((SalesOrder _) => _.RevisionNumber, x => x.WithDefaultValue((byte?)0));
        public static readonly Mounter<_DateTime> _OrderDate = RegisterColumn((SalesOrder _) => _.OrderDate, x => x.WithDefault(Functions.GetDate()));
        public static readonly Mounter<_DateTime> _DueDate = RegisterColumn((SalesOrder _) => _.DueDate);
        public static readonly Mounter<_DateTime> _ShipDate = RegisterColumn((SalesOrder _) => _.ShipDate);
        public static readonly Mounter<_SalesOrderStatus> _Status = RegisterColumn((SalesOrder _) => _.Status, x => x.DefaultValue(SalesOrderStatus.InProcess));
        public static readonly Mounter<_Boolean> _OnlineOrderFlag = RegisterColumn((SalesOrder _) => _.OnlineOrderFlag, x => x.DefaultValue(true));
        public static readonly Mounter<_String> _SalesOrderNumber = RegisterColumn((SalesOrder _) => _.SalesOrderNumber);
        public static readonly Mounter<_String> _PurchaseOrderNumber = RegisterColumn((SalesOrder _) => _.PurchaseOrderNumber);
        public static readonly Mounter<_String> _AccountNumber = RegisterColumn((SalesOrder _) => _.AccountNumber);
        public static readonly Mounter<_Int32> _CustomerID = RegisterColumn((SalesOrder _) => _.CustomerID);
        public static readonly Mounter<_Int32> _ShipToAddressID = RegisterColumn((SalesOrder _) => _.ShipToAddressID);
        public static readonly Mounter<_Int32> _BillToAddressID = RegisterColumn((SalesOrder _) => _.BillToAddressID);
        public static readonly Mounter<_String> _ShipMethod = RegisterColumn((SalesOrder _) => _.ShipMethod);
        public static readonly Mounter<_String> _CreditCardApprovalCode = RegisterColumn((SalesOrder _) => _.CreditCardApprovalCode);
        public static readonly Mounter<_Decimal> _SubTotal = RegisterColumn((SalesOrder _) => _.SubTotal, x => x.DefaultValue(0));
        public static readonly Mounter<_Decimal> _TaxAmt = RegisterColumn((SalesOrder _) => _.TaxAmt, x => x.DefaultValue(0));
        public static readonly Mounter<_Decimal> _Freight = RegisterColumn((SalesOrder _) => _.Freight, x => x.DefaultValue(0));
        public static readonly Mounter<_Decimal> _TotalDue = RegisterColumn((SalesOrder _) => _.TotalDue, x => x.DefaultValue(0));
        public static readonly Mounter<_String> _Comment = RegisterColumn((SalesOrder _) => _.Comment);

        static SalesOrder()
        {
            RegisterChildModel((SalesOrder x) => x.SalesOrderDetails, (SalesOrderDetail x) => x.SalesOrderKey);
        }

        public SalesOrderDetail SalesOrderDetails { get; private set; }

        public SalesOrder()
        {
            _primaryKey = new Key(SalesOrderID);
            CustomerKey = new Customer.Key(CustomerID);
            ShipToAddressKey = new Address.Key(ShipToAddressID);
            BillToAddressKey = new Address.Key(BillToAddressID);

            SalesOrderNumber.ComputedAs((_String.Const("SO") + ((_String)SalesOrderID).AsNVarChar(23)).IfNull(_String.Const("*** ERROR ***")));
            TotalDue.ComputedAs((SubTotal + TaxAmt + Freight).IfNull(_Decimal.Const(0)));
        }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get { return _primaryKey; }
        }

        public Customer.Key CustomerKey { get; private set; }

        public Address.Key ShipToAddressKey { get; private set; }

        public Address.Key BillToAddressKey { get; private set; }

        [Identity(1, 1)]
        public _Int32 SalesOrderID { get; private set; }

        [Required]
        public _Byte RevisionNumber { get; private set; }

        [Required]
        [AsDateTime]
        public _DateTime OrderDate { get; private set; }

        [Required]
        [AsDateTime]
        public _DateTime DueDate { get; private set; }

        [AsDateTime]
        public _DateTime ShipDate { get; private set; }

        [Required]
        public _SalesOrderStatus Status { get; private set; }

        [Required]
        public _Boolean OnlineOrderFlag { get; private set; }

        [UdtOrderNumber]
        public _String SalesOrderNumber { get; private set; }

        [UdtOrderNumber]
        public _String PurchaseOrderNumber { get; private set; }

        [UdtAccountNumber]
        public _String AccountNumber { get; private set; }

        [Required]
        public _Int32 CustomerID { get; private set; }
        
        public _Int32 ShipToAddressID { get; private set; }

        public _Int32 BillToAddressID { get; private set; }

        [Required]
        [AsNVarChar(50)]
        public _String ShipMethod { get; private set; }

        [AsNVarChar(15)]
        public _String CreditCardApprovalCode { get; private set; }

        [Required]
        [AsMoney]
        public _Decimal SubTotal { get; private set; }

        [Required]
        [AsMoney]
        public _Decimal TaxAmt { get; private set; }

        [Required]
        [AsMoney]
        public _Decimal Freight { get; private set; }

        [Required]
        [AsMoney]
        public _Decimal TotalDue { get; private set; }

        [AsNVarCharMax]
        public _String Comment { get; private set; }
    }
}

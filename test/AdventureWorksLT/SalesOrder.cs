using DevZest.Data;
using DevZest.Data.SqlServer;
using System;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrder : BaseModel<SalesOrder.Key>
    {
        public sealed class Key : ModelKey
        {
            public Key(_Int32 salesOrderID)
            {
                SalesOrderID = salesOrderID;
            }

            public _Int32 SalesOrderID { get; private set; }
        }

        public static readonly Property<_Int32> _SalesOrderID = RegisterColumn((SalesOrder x) => x.SalesOrderID);
        public static readonly Property<_Byte> _RevisionNumber = RegisterColumn((SalesOrder x) => x.RevisionNumber, x => x.DefaultValue(0));
        public static readonly Property<_DateTime> _OrderDate = RegisterColumn((SalesOrder x) => x.OrderDate, x => x.Default(DevZest.Data.Functions.GetDate()));
        public static readonly Property<_DateTime> _DueDate = RegisterColumn((SalesOrder x) => x.DueDate);
        public static readonly Property<_DateTime> _ShipDate = RegisterColumn((SalesOrder x) => x.ShipDate);
        public static readonly Property<_SalesOrderStatus> _Status = RegisterColumn((SalesOrder x) => x.Status, x => x.DefaultValue(SalesOrderStatus.InProcess));
        public static readonly Property<_Boolean> _OnlineOrderFlag = RegisterColumn((SalesOrder x) => x.OnlineOrderFlag, x => x.DefaultValue(true));
        public static readonly Property<_String> _SalesOrderNumber = RegisterColumn((SalesOrder x) => x.SalesOrderNumber);
        public static readonly Property<_String> _PurchaseOrderNumber = RegisterColumn((SalesOrder x) => x.PurchaseOrderNumber);
        public static readonly Property<_String> _AccountNumber = RegisterColumn((SalesOrder x) => x.AccountNumber);
        public static readonly Property<_Int32> _CustomerID = RegisterColumn((SalesOrder x) => x.CustomerID);
        public static readonly Property<_Int32> _ShipToAddressID = RegisterColumn((SalesOrder x) => x.ShipToAddressID);
        public static readonly Property<_Int32> _BillToAddressID = RegisterColumn((SalesOrder x) => x.BillToAddressID);
        public static readonly Property<_String> _ShipMethod = RegisterColumn((SalesOrder x) => x.ShipMethod);
        public static readonly Property<_String> _CreditCardApprovalCode = RegisterColumn((SalesOrder x) => x.CreditCardApprovalCode);
        public static readonly Property<_Decimal> _SubTotal = RegisterColumn((SalesOrder x) => x.SubTotal, x => x.DefaultValue(0));
        public static readonly Property<_Decimal> _TaxAmt = RegisterColumn((SalesOrder x) => x.TaxAmt, x => x.DefaultValue(0));
        public static readonly Property<_Decimal> _Freight = RegisterColumn((SalesOrder x) => x.Freight, x => x.DefaultValue(0));
        public static readonly Property<_Decimal> _TotalDue = RegisterColumn((SalesOrder x) => x.TotalDue, x => x.DefaultValue(0));
        public static readonly Property<_String> _Comment = RegisterColumn((SalesOrder x) => x.Comment);

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

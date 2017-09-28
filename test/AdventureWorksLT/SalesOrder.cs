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
            public static readonly Mounter<_Int32> _SalesOrderID;

            static Ref()
            {
                _SalesOrderID = RegisterColumn((Ref _) => _.SalesOrderID);
            }

            private Key _primaryKey;
            public sealed override Key PrimaryKey
            {
                get
                {
                    if (_primaryKey == null)
                        _primaryKey = new Key(SalesOrderID);
                    return _primaryKey;
                }
            }

            public _Int32 SalesOrderID { get; private set; }
        }

        public static readonly Mounter<_Byte> _RevisionNumber;
        public static readonly Mounter<_DateTime> _OrderDate;
        public static readonly Mounter<_DateTime> _DueDate;
        public static readonly Mounter<_DateTime> _ShipDate;
        public static readonly Mounter<_SalesOrderStatus> _Status;
        public static readonly Mounter<_Boolean> _OnlineOrderFlag;
        public static readonly Mounter<_String> _SalesOrderNumber;
        public static readonly Mounter<_String> _PurchaseOrderNumber;
        public static readonly Mounter<_String> _AccountNumber;
        public static readonly Mounter<_String> _ShipMethod;
        public static readonly Mounter<_String> _CreditCardApprovalCode;
        public static readonly Mounter<_Decimal> _SubTotal;
        public static readonly Mounter<_Decimal> _TaxAmt;
        public static readonly Mounter<_Decimal> _Freight;
        public static readonly Mounter<_Decimal> _TotalDue;
        public static readonly Mounter<_String> _Comment;

        static SalesOrder()
        {
            RegisterColumn((SalesOrder _) => _.SalesOrderID, Ref._SalesOrderID);
            _RevisionNumber = RegisterColumn((SalesOrder _) => _.RevisionNumber, x => x.WithDefaultValue((byte?)0));
            _OrderDate = RegisterColumn((SalesOrder _) => _.OrderDate, x => x.WithDefault(Functions.GetDate()));
            _DueDate = RegisterColumn((SalesOrder _) => _.DueDate);
            _ShipDate = RegisterColumn((SalesOrder _) => _.ShipDate);
            _Status = RegisterColumn((SalesOrder _) => _.Status, x => x.DefaultValue(SalesOrderStatus.InProcess));
            _OnlineOrderFlag = RegisterColumn((SalesOrder _) => _.OnlineOrderFlag, x => x.DefaultValue(true));
            _SalesOrderNumber = RegisterColumn((SalesOrder _) => _.SalesOrderNumber);
            _PurchaseOrderNumber = RegisterColumn((SalesOrder _) => _.PurchaseOrderNumber);
            _AccountNumber = RegisterColumn((SalesOrder _) => _.AccountNumber);
            RegisterColumn((SalesOrder _) => _.CustomerID, AdventureWorksLT.Customer.Ref._CustomerID);
            RegisterColumn((SalesOrder _) => _.ShipToAddressID, Address.Ref._AddressID);
            RegisterColumn((SalesOrder _) => _.BillToAddressID, Address.Ref._AddressID);
            _ShipMethod = RegisterColumn((SalesOrder _) => _.ShipMethod);
            _CreditCardApprovalCode = RegisterColumn((SalesOrder _) => _.CreditCardApprovalCode);
            _SubTotal = RegisterColumn((SalesOrder _) => _.SubTotal, x => x.DefaultValue(0));
            _TaxAmt = RegisterColumn((SalesOrder _) => _.TaxAmt, x => x.DefaultValue(0));
            _Freight = RegisterColumn((SalesOrder _) => _.Freight, x => x.DefaultValue(0));
            _TotalDue = RegisterColumn((SalesOrder _) => _.TotalDue, x => x.DefaultValue(0));
            _Comment = RegisterColumn((SalesOrder _) => _.Comment);
            RegisterChildModel((SalesOrder x) => x.SalesOrderDetails, (SalesOrderDetail x) => x.SalesOrder);
        }

        public SalesOrderDetail SalesOrderDetails { get; private set; }

        public SalesOrder()
        {
            SalesOrderNumber.ComputedAs((_String.Const("SO") + ((_String)SalesOrderID).AsNVarChar(23)).IfNull(_String.Const("*** ERROR ***")));
            TotalDue.ComputedAs((SubTotal + TaxAmt + Freight).IfNull(_Decimal.Const(0)));
        }

        private Key _primaryKey;
        public sealed override Key PrimaryKey
        {
            get
            {
                if (_primaryKey == null)
                    _primaryKey = new Key(SalesOrderID);
                return _primaryKey;
            }
        }

        private Customer.Key _customer;
        public Customer.Key Customer
        {
            get
            {
                if (_customer == null)
                    _customer = new Customer.Key(CustomerID);
                return _customer;
            }
        }

        private Address.Key _shipToAddress;
        public Address.Key ShipToAddress
        {
            get
            {
                if (_shipToAddress == null)
                    _shipToAddress = new Address.Key(ShipToAddressID);
                return _shipToAddress;
            }
        }

        private CustomerAddress.Key _shipToCustomerAddress;
        public CustomerAddress.Key ShipToCustomerAddress
        {
            get
            {
                if (_shipToCustomerAddress == null)
                    _shipToCustomerAddress = new CustomerAddress.Key(CustomerID, ShipToAddressID);
                return _shipToCustomerAddress;
            }
        }

        private Address.Key _billToAddress;
        public Address.Key BillToAddress
        {
            get
            {
                if (_billToAddress == null)
                    _billToAddress = new Address.Key(BillToAddressID);
                return _billToAddress;
            }
        }

        private CustomerAddress.Key _billToCustomerAddress;
        public CustomerAddress.Key BillToCustomerAddress
        {
            get
            {
                if (_billToCustomerAddress == null)
                    _billToCustomerAddress = new CustomerAddress.Key(CustomerID, BillToAddressID);
                return _billToCustomerAddress;
            }
        }

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

using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrder : BaseModel<SalesOrder.Key>
    {
        [DbConstraint("PK_SalesOrderHeader_SalesOrderID", Description = "Clustered index created by a primary key constraint.")]
        public sealed class Key : PrimaryKey
        {
            public Key(_Int32 salesOrderID)
            {
                SalesOrderID = salesOrderID;
            }

            public _Int32 SalesOrderID { get; private set; }
        }
        
        public class Ref : Model<Key>
        {
            static Ref()
            {
                RegisterColumn((Ref _) => _.SalesOrderID, _SalesOrderID);
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

        public static readonly Mounter<_Int32> _SalesOrderID;
        public static readonly Mounter<_Byte> _RevisionNumber;
        public static readonly Mounter<_DateTime> _OrderDate;
        public static readonly Mounter<_DateTime> _DueDate;
        public static readonly Mounter<_DateTime> _ShipDate;
        public static readonly Mounter<_ByteEnum<SalesOrderStatus>> _Status;
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
            _SalesOrderID = RegisterColumn((SalesOrder _) => _.SalesOrderID);
            _RevisionNumber = RegisterColumn((SalesOrder _) => _.RevisionNumber);
            _OrderDate = RegisterColumn((SalesOrder _) => _.OrderDate);
            _DueDate = RegisterColumn((SalesOrder _) => _.DueDate);
            _ShipDate = RegisterColumn((SalesOrder _) => _.ShipDate);
            _Status = RegisterColumn((SalesOrder _) => _.Status);
            _OnlineOrderFlag = RegisterColumn((SalesOrder _) => _.OnlineOrderFlag);
            _SalesOrderNumber = RegisterColumn((SalesOrder _) => _.SalesOrderNumber);
            _PurchaseOrderNumber = RegisterColumn((SalesOrder _) => _.PurchaseOrderNumber);
            _AccountNumber = RegisterColumn((SalesOrder _) => _.AccountNumber);
            RegisterColumn((SalesOrder _) => _.CustomerID, AdventureWorksLT.Customer._CustomerID);
            RegisterColumn((SalesOrder _) => _.ShipToAddressID, Address._AddressID);
            RegisterColumn((SalesOrder _) => _.BillToAddressID, Address._AddressID);
            _ShipMethod = RegisterColumn((SalesOrder _) => _.ShipMethod);
            _CreditCardApprovalCode = RegisterColumn((SalesOrder _) => _.CreditCardApprovalCode);
            _SubTotal = RegisterColumn((SalesOrder _) => _.SubTotal);
            _TaxAmt = RegisterColumn((SalesOrder _) => _.TaxAmt);
            _Freight = RegisterColumn((SalesOrder _) => _.Freight);
            _TotalDue = RegisterColumn((SalesOrder _) => _.TotalDue);
            _Comment = RegisterColumn((SalesOrder _) => _.Comment);
            RegisterChildModel((SalesOrder x) => x.SalesOrderDetails, (SalesOrderDetail x) => x.SalesOrder);
        }

        public virtual SalesOrderDetail SalesOrderDetails { get; private set; }

        public SalesOrder()
        {
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
        [DbColumn(Description = "Primary key.")]
        public _Int32 SalesOrderID { get; private set; }

        [Required]
        [DefaultValue((byte)0, Name = "DF_SalesOrderHeader_RevisionNumber")]
        [DbColumn(Description = "Incremental number to track changes to the sales order over time.")]
        public _Byte RevisionNumber { get; private set; }

        [Required]
        [AsDateTime]
        [AutoDateTime(Name = "DF_SalesOrderHeader_OrderDate")]
        [DbColumn(Description = "Dates the sales order was created.")]
        public _DateTime OrderDate { get; private set; }

        [Required]
        [AsDateTime]
        [DbColumn(Description = "Date the order is due to the customer.")]
        public _DateTime DueDate { get; private set; }

        [AsDateTime]
        [DbColumn(Description = "Date the order was shipped to the customer.")]
        public _DateTime ShipDate { get; private set; }

        [Required]
        [DefaultValue(typeof(SalesOrderStatus), nameof(SalesOrderStatus.InProcess), Name = "DF_SalesOrderHeader_Status")]
        [DbColumn(Description = "Order current status. 1 = In process; 2 = Approved; 3 = Backordered; 4 = Rejected; 5 = Shipped; 6 = Cancelled")]
        public _ByteEnum<SalesOrderStatus> Status { get; private set; }

        [Required]
        [DefaultValue(true, Name = "DF_SalesOrderHeader_OnlineOrderFlag")]
        [DbColumn(Description = "0 = Order placed by sales person. 1 = Order placed online by customer.")]
        public _Boolean OnlineOrderFlag { get; private set; }

        [UdtOrderNumber]
        [DbColumn(Description = "Unique sales order identification number.")]
        [Unique(Name = "AK_SalesOrderHeader_SalesOrderNumber", Description = "Unique nonclustered constraint.")]
        public _String SalesOrderNumber { get; private set; }

        [UdtOrderNumber]
        [DbColumn(Description = "Customer purchase order number reference.")]
        public _String PurchaseOrderNumber { get; private set; }

        [UdtAccountNumber]
        [DbColumn(Description = "Financial accounting number reference.")]
        public _String AccountNumber { get; private set; }

        [Required]
        [DbColumn(Description = "Customer identification number. Foreign key to Customer.CustomerID.")]
        [DbIndex("IX_SalesOrderHeader_CustomerID", Description = "Nonclustered index.")]
        public _Int32 CustomerID { get; private set; }
        
        [DbColumn(Description = "The ID of the location to send goods.  Foreign key to the Address table.")]
        public _Int32 ShipToAddressID { get; private set; }

        [DbColumn(Description = "The ID of the location to send invoices.  Foreign key to the Address table.")]
        public _Int32 BillToAddressID { get; private set; }

        [Required]
        [AsNVarChar(50)]
        [DbColumn(Description = "Shipping method. Foreign key to ShipMethod.ShipMethodID.")]
        public _String ShipMethod { get; private set; }

        [AsNVarChar(15)]
        [DbColumn(Description = "Approval code provided by the credit card company.")]
        public _String CreditCardApprovalCode { get; private set; }

        [Required]
        [AsMoney]
        [DefaultValue(typeof(decimal), "0", Name = "DF_SalesOrderHeader_SubTotal")]
        [DbColumn(Description = "Sales subtotal. Computed as SUM(SalesOrderDetail.LineTotal)for the appropriate SalesOrderID.")]
        public _Decimal SubTotal { get; private set; }

        [Required]
        [AsMoney]
        [DefaultValue(typeof(decimal), "0", Name = "DF_SalesOrderHeader_TaxAmt")]
        [DbColumn(Description = "Tax amount.")]
        public _Decimal TaxAmt { get; private set; }

        [Required]
        [AsMoney]
        [DefaultValue(typeof(decimal), "0", Name = "DF_SalesOrderHeader_Freight")]
        [DbColumn(Description = "Shipping cost.")]
        public _Decimal Freight { get; private set; }

        [Required]
        [AsMoney]
        [DbColumn(Description = "Total due from customer. Computed as Subtotal + TaxAmt + Freight.")]
        public _Decimal TotalDue { get; private set; }

        [AsNVarCharMax]
        [DbColumn(Description = "Sales representative comments.")]
        public _String Comment { get; private set; }

        [Computation]
        private void ComputeSalesOrderNumber()
        {
            SalesOrderNumber.ComputedAs((_String.Const("SO") + ((_String)SalesOrderID).AsNVarChar(23)).IfNull(_String.Const("*** ERROR ***")));
        }

        [Computation]
        private void ComputeTotalDue()
        {
            TotalDue.ComputedAs((SubTotal + TaxAmt + Freight).IfNull(_Decimal.Const(0)));
        }

        private _Boolean _ck_SalesOrderHeader_DueDate;
        [Check("DueDate cannot be earlier than OrderDate.", Name = nameof(CK_SalesOrderHeader_DueDate), Description = "Check constraint [DueDate] >= [OrderDate]")]
        private _Boolean CK_SalesOrderHeader_DueDate
        {
            get { return _ck_SalesOrderHeader_DueDate ?? (_ck_SalesOrderHeader_DueDate = DueDate >= OrderDate); }
        }

        private _Boolean _ck_SalesOrderHeader_Freight;
        [Check("Freight cannot be a negtive value", Name = nameof(CK_SalesOrderHeader_Freight), Description = "Check constraint [Freight] >= (0.00)")]
        private _Boolean CK_SalesOrderHeader_Freight
        {
            get { return _ck_SalesOrderHeader_Freight ?? (_ck_SalesOrderHeader_Freight = Freight >= _Decimal.Const(0)); }
        }

        private _Boolean _ck_SalesOrderHeader_ShipDate;
        [Check("ShipDate cannot be earlier than OrderDate", Name = nameof(CK_SalesOrderHeader_ShipDate), Description = "Check constraint [ShipDate] >= [OrderDate] OR [ShipDate] IS NULL")]
        private _Boolean CK_SalesOrderHeader_ShipDate
        {
            get { return _ck_SalesOrderHeader_ShipDate ?? (_ck_SalesOrderHeader_ShipDate = ShipDate >= OrderDate | ShipDate.IsNull()); }
        }

        private _Boolean _ck_SalesOrderHeader_SubTotal;
        [Check("SubTotal cannot be a negative value.", Name = nameof(CK_SalesOrderHeader_SubTotal), Description = "Check constraint [SubTotal] >= (0.00)")]
        private _Boolean CK_SalesOrderHeader_SubTotal
        {
            get { return _ck_SalesOrderHeader_SubTotal ?? (_ck_SalesOrderHeader_SubTotal = SubTotal >= _Decimal.Const(0)); }
        }

        private _Boolean _ck_SalesOrderHeader_TaxAmt;
        [Check("TaxAmt cannot be a negtive value.", Name = nameof(CK_SalesOrderHeader_TaxAmt), Description = "Check constraint [TaxAmt] >= (0.00)")]
        private _Boolean CK_SalesOrderHeader_TaxAmt
        {
            get { return _ck_SalesOrderHeader_TaxAmt ?? (_ck_SalesOrderHeader_TaxAmt = TaxAmt >= _Decimal.Const(0)); }
        }

        private _Boolean _ck_SalesOrderHeader_Status;
        [Check("Invalid Status.", Name = nameof(CK_SalesOrderHeader_Status), Description = "Check constraint [Status] BETWEEN (1) AND (6)")]
        private _Boolean CK_SalesOrderHeader_Status
        {
            get { return _ck_SalesOrderHeader_Status ?? (_ck_SalesOrderHeader_Status = IsValid(Status)); }
        }

        private static _Boolean IsValid(_ByteEnum<SalesOrderStatus> status)
        {
            var byteExpr = (_Byte)status;
            return byteExpr >= _Byte.Const(1) & byteExpr <= _Byte.Const(6);
        }
    }
}

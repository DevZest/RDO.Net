using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrderHeader : BaseModel<SalesOrderHeader.PK>
    {
        [DbPrimaryKey("PK_SalesOrderHeader_SalesOrderID", Description = "Clustered index created by a primary key constraint.")]
        public sealed class PK : PrimaryKey
        {
            public static IDataValues ValueOf(int salesOrderID)
            {
                return DataValues.Create(_Int32.Const(salesOrderID));
            }

            public PK(_Int32 salesOrderID)
                : base(salesOrderID)
            {
            }

            public _Int32 SalesOrderID
            {
                get { return GetColumn<_Int32>(0); }
            }
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                Register((Key _) => _.SalesOrderID, _SalesOrderID);
            }

            protected override PK GetPrimaryKey()
            {
                return new PK(SalesOrderID);
            }

            public _Int32 SalesOrderID { get; private set; }
        }

        public static readonly Mounter<_Int32> _SalesOrderID = RegisterColumn((SalesOrderHeader _) => _.SalesOrderID);
        public static readonly Mounter<_Byte> _RevisionNumber = RegisterColumn((SalesOrderHeader _) => _.RevisionNumber);
        public static readonly Mounter<_DateTime> _OrderDate = RegisterColumn((SalesOrderHeader _) => _.OrderDate);
        public static readonly Mounter<_DateTime> _DueDate = RegisterColumn((SalesOrderHeader _) => _.DueDate);
        public static readonly Mounter<_DateTime> _ShipDate = RegisterColumn((SalesOrderHeader _) => _.ShipDate);
        public static readonly Mounter<_ByteEnum<SalesOrderStatus>> _Status = RegisterColumn((SalesOrderHeader _) => _.Status);
        public static readonly Mounter<_Boolean> _OnlineOrderFlag = RegisterColumn((SalesOrderHeader _) => _.OnlineOrderFlag);
        public static readonly Mounter<_String> _SalesOrderNumber = RegisterColumn((SalesOrderHeader _) => _.SalesOrderNumber);
        public static readonly Mounter<_String> _PurchaseOrderNumber = RegisterColumn((SalesOrderHeader _) => _.PurchaseOrderNumber);
        public static readonly Mounter<_String> _AccountNumber = RegisterColumn((SalesOrderHeader _) => _.AccountNumber);
        public static readonly Mounter<_Int32> _CustomerID = RegisterColumn((SalesOrderHeader _) => _.CustomerID);
        public static readonly Mounter<_Int32> _ShipToAddressID = RegisterColumn((SalesOrderHeader _) => _.ShipToAddressID);
        public static readonly Mounter<_Int32> _BillToAddressID = RegisterColumn((SalesOrderHeader _) => _.BillToAddressID);
        public static readonly Mounter<_String> _ShipMethod = RegisterColumn((SalesOrderHeader _) => _.ShipMethod);
        public static readonly Mounter<_String> _CreditCardApprovalCode = RegisterColumn((SalesOrderHeader _) => _.CreditCardApprovalCode);
        public static readonly Mounter<_Decimal> _SubTotal = RegisterColumn((SalesOrderHeader _) => _.SubTotal);
        public static readonly Mounter<_Decimal> _TaxAmt = RegisterColumn((SalesOrderHeader _) => _.TaxAmt);
        public static readonly Mounter<_Decimal> _Freight = RegisterColumn((SalesOrderHeader _) => _.Freight);
        public static readonly Mounter<_Decimal> _TotalDue = RegisterColumn((SalesOrderHeader _) => _.TotalDue);
        public static readonly Mounter<_String> _Comment = RegisterColumn((SalesOrderHeader _) => _.Comment);

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(SalesOrderID);
        }

        private Customer.PK _fk_customer;
        public Customer.PK FK_Customer
        {
            get { return _fk_customer ?? (_fk_customer = new Customer.PK(CustomerID)); }
        }

        private Address.PK _fk_shipToAddress;
        public Address.PK FK_ShipToAddress
        {
            get { return _fk_shipToAddress ?? (_fk_shipToAddress = new Address.PK(ShipToAddressID)); }
        }

        private CustomerAddress.PK _fk_shipToCustomerAddress;
        public CustomerAddress.PK FK_ShipToCustomerAddress
        {
            get { return _fk_shipToCustomerAddress ?? (_fk_shipToCustomerAddress = new CustomerAddress.PK(CustomerID, ShipToAddressID)); }
        }

        private Address.PK _fk_billToAddress;
        public Address.PK FK_BillToAddress
        {
            get { return _fk_billToAddress ?? (_fk_billToAddress = new Address.PK(BillToAddressID)); }
        }

        private CustomerAddress.PK _fk_billToCustomerAddress;
        public CustomerAddress.PK FK_BillToCustomerAddress
        {
            get { return _fk_billToCustomerAddress ?? (_fk_billToCustomerAddress = new CustomerAddress.PK(CustomerID, BillToAddressID)); }
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
        [Check(typeof(UserMessages), nameof(UserMessages.CK_SalesOrderHeader_DueDate), Name = nameof(CK_SalesOrderHeader_DueDate), Description = "Check constraint [DueDate] >= [OrderDate]")]
        private _Boolean CK_SalesOrderHeader_DueDate
        {
            get { return _ck_SalesOrderHeader_DueDate ?? (_ck_SalesOrderHeader_DueDate = DueDate >= OrderDate); }
        }

        private _Boolean _ck_SalesOrderHeader_Freight;
        [Check(typeof(UserMessages), nameof(UserMessages.CK_SalesOrderHeader_Freight), Name = nameof(CK_SalesOrderHeader_Freight), Description = "Check constraint [Freight] >= (0.00)")]
        private _Boolean CK_SalesOrderHeader_Freight
        {
            get { return _ck_SalesOrderHeader_Freight ?? (_ck_SalesOrderHeader_Freight = Freight >= _Decimal.Const(0)); }
        }

        private _Boolean _ck_SalesOrderHeader_ShipDate;
        [Check(typeof(UserMessages), nameof(UserMessages.CK_SalesOrderHeader_ShipDate), Name = nameof(CK_SalesOrderHeader_ShipDate), Description = "Check constraint [ShipDate] >= [OrderDate] OR [ShipDate] IS NULL")]
        private _Boolean CK_SalesOrderHeader_ShipDate
        {
            get { return _ck_SalesOrderHeader_ShipDate ?? (_ck_SalesOrderHeader_ShipDate = ShipDate >= OrderDate | ShipDate.IsNull()); }
        }

        private _Boolean _ck_SalesOrderHeader_SubTotal;
        [Check(typeof(UserMessages), nameof(UserMessages.CK_SalesOrderHeader_SubTotal), Name = nameof(CK_SalesOrderHeader_SubTotal), Description = "Check constraint [SubTotal] >= (0.00)")]
        private _Boolean CK_SalesOrderHeader_SubTotal
        {
            get { return _ck_SalesOrderHeader_SubTotal ?? (_ck_SalesOrderHeader_SubTotal = SubTotal >= _Decimal.Const(0)); }
        }

        private _Boolean _ck_SalesOrderHeader_TaxAmt;
        [Check(typeof(UserMessages), nameof(UserMessages.CK_SalesOrderHeader_TaxAmt), Name = nameof(CK_SalesOrderHeader_TaxAmt), Description = "Check constraint [TaxAmt] >= (0.00)")]
        private _Boolean CK_SalesOrderHeader_TaxAmt
        {
            get { return _ck_SalesOrderHeader_TaxAmt ?? (_ck_SalesOrderHeader_TaxAmt = TaxAmt >= _Decimal.Const(0)); }
        }

        private _Boolean _ck_SalesOrderHeader_Status;
        [Check(typeof(UserMessages), nameof(UserMessages.CK_SalesOrderHeader_Status), Name = nameof(CK_SalesOrderHeader_Status), Description = "Check constraint [Status] BETWEEN (1) AND (6)")]
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

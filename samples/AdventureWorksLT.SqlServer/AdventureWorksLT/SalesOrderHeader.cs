using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    [Computation(nameof(ComputeSalesOrderNumber))]
    [Computation(nameof(ComputeTotalDue))]
    [CheckConstraint(nameof(CK_SalesOrderHeader_DueDate), typeof(UserMessages), nameof(UserMessages.CK_SalesOrderHeader_DueDate), Description = "Check constraint [DueDate] >= [OrderDate]")]
    [CheckConstraint(nameof(CK_SalesOrderHeader_Freight), typeof(UserMessages), nameof(UserMessages.CK_SalesOrderHeader_Freight), Description = "Check constraint [Freight] >= (0.00)")]
    [CheckConstraint(nameof(CK_SalesOrderHeader_ShipDate), typeof(UserMessages), nameof(UserMessages.CK_SalesOrderHeader_ShipDate), Description = "Check constraint [ShipDate] >= [OrderDate] OR [ShipDate] IS NULL")]
    [CheckConstraint(nameof(CK_SalesOrderHeader_SubTotal), typeof(UserMessages), nameof(UserMessages.CK_SalesOrderHeader_SubTotal), Description = "Check constraint [SubTotal] >= (0.00)")]
    [CheckConstraint(nameof(CK_SalesOrderHeader_TaxAmt), typeof(UserMessages), nameof(UserMessages.CK_SalesOrderHeader_TaxAmt), Description = "Check constraint [TaxAmt] >= (0.00)")]
    [CheckConstraint(nameof(CK_SalesOrderHeader_Status), typeof(UserMessages), nameof(UserMessages.CK_SalesOrderHeader_Status), Description = "Check constraint [Status] BETWEEN (1) AND (6)")]
    [UniqueConstraint(nameof(AK_SalesOrderHeader_SalesOrderNumber), Description = "Unique nonclustered constraint.")]
    [DbIndex(nameof(IX_SalesOrderHeader_CustomerID), Description = "Nonclustered index.")]
    public class SalesOrderHeader : BaseModel<SalesOrderHeader.PK>
    {
        [DbPrimaryKey("PK_SalesOrderHeader_SalesOrderID", Description = "Clustered index created by a primary key constraint.")]
        public sealed class PK : CandidateKey
        {
            public PK(_Int32 salesOrderID)
                : base(salesOrderID)
            {
            }
        }

        public class Key : Key<PK>
        {
            static Key()
            {
                Register((Key _) => _.SalesOrderID, _SalesOrderID);
            }

            protected override PK CreatePrimaryKey()
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

        [Identity]
        [DbColumn(Description = "Primary key.")]
        public _Int32 SalesOrderID { get; private set; }

        [Required]
        [DefaultValue((byte)0, Name = "DF_SalesOrderHeader_RevisionNumber")]
        [DbColumn(Description = "Incremental number to track changes to the sales order over time.")]
        public _Byte RevisionNumber { get; private set; }

        [Required]
        [SqlDateTime]
        [AutoDateTime(Name = "DF_SalesOrderHeader_OrderDate")]
        [DbColumn(Description = "Dates the sales order was created.")]
        public _DateTime OrderDate { get; private set; }

        [Required]
        [SqlDateTime]
        [DbColumn(Description = "Date the order is due to the customer.")]
        public _DateTime DueDate { get; private set; }

        [SqlDateTime]
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
        public _String SalesOrderNumber { get; private set; }

        [UdtOrderNumber]
        [DbColumn(Description = "Customer purchase order number reference.")]
        public _String PurchaseOrderNumber { get; private set; }

        [UdtAccountNumber]
        [DbColumn(Description = "Financial accounting number reference.")]
        public _String AccountNumber { get; private set; }

        [Required]
        [DbColumn(Description = "Customer identification number. Foreign key to Customer.CustomerID.")]
        public _Int32 CustomerID { get; private set; }

        [DbColumn(Description = "The ID of the location to send goods.  Foreign key to the Address table.")]
        public _Int32 ShipToAddressID { get; private set; }

        [DbColumn(Description = "The ID of the location to send invoices.  Foreign key to the Address table.")]
        public _Int32 BillToAddressID { get; private set; }

        [Required]
        [SqlNVarChar(50)]
        [DbColumn(Description = "Shipping method. Foreign key to ShipMethod.ShipMethodID.")]
        public _String ShipMethod { get; private set; }

        [SqlNVarChar(15)]
        [DbColumn(Description = "Approval code provided by the credit card company.")]
        public _String CreditCardApprovalCode { get; private set; }

        [Required]
        [SqlMoney]
        [DefaultValue(typeof(decimal), "0", Name = "DF_SalesOrderHeader_SubTotal")]
        [DbColumn(Description = "Sales subtotal. Computed as SUM(SalesOrderDetail.LineTotal)for the appropriate SalesOrderID.")]
        public _Decimal SubTotal { get; private set; }

        [Required]
        [SqlMoney]
        [DefaultValue(typeof(decimal), "0", Name = "DF_SalesOrderHeader_TaxAmt")]
        [DbColumn(Description = "Tax amount.")]
        public _Decimal TaxAmt { get; private set; }

        [Required]
        [SqlMoney]
        [DefaultValue(typeof(decimal), "0", Name = "DF_SalesOrderHeader_Freight")]
        [DbColumn(Description = "Shipping cost.")]
        public _Decimal Freight { get; private set; }

        [Required]
        [SqlMoney]
        [DbColumn(Description = "Total due from customer. Computed as Subtotal + TaxAmt + Freight.")]
        public _Decimal TotalDue { get; private set; }

        [SqlNVarCharMax]
        [DbColumn(Description = "Sales representative comments.")]
        public _String Comment { get; private set; }

        [_Computation]
        private void ComputeSalesOrderNumber()
        {
            SalesOrderNumber.ComputedAs((_String.Const("SO") + ((_String)SalesOrderID).AsSqlNVarChar(23)).IfNull(_String.Const("*** ERROR ***")));
        }

        [_Computation]
        private void ComputeTotalDue()
        {
            TotalDue.ComputedAs((SubTotal + TaxAmt + Freight).IfNull(_Decimal.Const(0)));
        }

        [_CheckConstraint]
        private _Boolean CK_SalesOrderHeader_DueDate
        {
            get { return DueDate >= OrderDate; }
        }

        [_CheckConstraint]
        private _Boolean CK_SalesOrderHeader_Freight
        {
            get { return Freight >= _Decimal.Const(0); }
        }

        [_CheckConstraint]
        private _Boolean CK_SalesOrderHeader_ShipDate
        {
            get { return ShipDate >= OrderDate | ShipDate.IsNull(); }
        }

        [_CheckConstraint]
        private _Boolean CK_SalesOrderHeader_SubTotal
        {
            get { return SubTotal >= _Decimal.Const(0); }
        }

        [_CheckConstraint]
        private _Boolean CK_SalesOrderHeader_TaxAmt
        {
            get { return TaxAmt >= _Decimal.Const(0); }
        }

        [_CheckConstraint]
        private _Boolean CK_SalesOrderHeader_Status
        {
            get { return IsValid(Status); }
        }

        private static _Boolean IsValid(_ByteEnum<SalesOrderStatus> status)
        {
            var byteExpr = (_Byte)status;
            return byteExpr >= _Byte.Const(1) & byteExpr <= _Byte.Const(6);
        }

        [_UniqueConstraint]
        private ColumnSort[] AK_SalesOrderHeader_SalesOrderNumber => new ColumnSort[] { SalesOrderNumber };

        [_DbIndex]
        private ColumnSort[] IX_SalesOrderHeader_CustomerID => new ColumnSort[] { CustomerID };
    }
}

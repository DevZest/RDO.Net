Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer

Namespace DevZest.Samples.AdventureWorksLT
    Public Class SalesOrderHeader
        Inherits BaseModel(Of PK)

        <DbPrimaryKey("PK_SalesOrderHeader_SalesOrderID", Description:="Clustered index created by a primary key constraint.")>
        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Sub New(ByVal salesOrderID As _Int32)
                Me.SalesOrderID = salesOrderID
            End Sub

            Public ReadOnly Property SalesOrderID As _Int32
        End Class

        Public Shared Function GetKey(ByVal salesOrderID As Integer) As IDataValues
            Return DataValues.Create(_Int32.[Const](salesOrderID))
        End Function

        Public NotInheritable Class Key
            Inherits Model(Of PK)

            Shared Sub New()
                RegisterColumn(Function(ByVal __ As Key) __.SalesOrderID, _SalesOrderID)
            End Sub

            Protected Overrides Function CreatePrimaryKey() As PK
                Return New PK(SalesOrderID)
            End Function

            Private m_SalesOrderID As _Int32
            Public Property SalesOrderID As _Int32
                Get
                    Return m_SalesOrderID
                End Get
                Private Set
                    m_SalesOrderID = Value
                End Set
            End Property
        End Class

        Public Class Ref
            Inherits LeafProjection(Of PK)

            Shared Sub New()
                Register(Function(x As Ref) x.SalesOrderID, _SalesOrderID)
            End Sub

            Private m_SalesOrderID As _Int32
            Public Property SalesOrderID As _Int32
                Get
                    Return m_SalesOrderID
                End Get
                Private Set
                    m_SalesOrderID = Value
                End Set
            End Property

            Protected Overrides Function CreatePrimaryKey() As PK
                Return New PK(SalesOrderID)
            End Function
        End Class

        Public Shared ReadOnly _SalesOrderID As Mounter(Of _Int32)
        Public Shared ReadOnly _RevisionNumber As Mounter(Of _Byte)
        Public Shared ReadOnly _OrderDate As Mounter(Of _DateTime)
        Public Shared ReadOnly _DueDate As Mounter(Of _DateTime)
        Public Shared ReadOnly _ShipDate As Mounter(Of _DateTime)
        Public Shared ReadOnly _Status As Mounter(Of _ByteEnum(Of SalesOrderStatus))
        Public Shared ReadOnly _OnlineOrderFlag As Mounter(Of _Boolean)
        Public Shared ReadOnly _SalesOrderNumber As Mounter(Of _String)
        Public Shared ReadOnly _PurchaseOrderNumber As Mounter(Of _String)
        Public Shared ReadOnly _AccountNumber As Mounter(Of _String)
        Public Shared ReadOnly _ShipMethod As Mounter(Of _String)
        Public Shared ReadOnly _CreditCardApprovalCode As Mounter(Of _String)
        Public Shared ReadOnly _SubTotal As Mounter(Of _Decimal)
        Public Shared ReadOnly _TaxAmt As Mounter(Of _Decimal)
        Public Shared ReadOnly _Freight As Mounter(Of _Decimal)
        Public Shared ReadOnly _TotalDue As Mounter(Of _Decimal)
        Public Shared ReadOnly _Comment As Mounter(Of _String)

        Shared Sub New()
            _SalesOrderID = RegisterColumn(Function(x As SalesOrderHeader) x.SalesOrderID)
            _RevisionNumber = RegisterColumn(Function(x As SalesOrderHeader) x.RevisionNumber)
            _OrderDate = RegisterColumn(Function(x As SalesOrderHeader) x.OrderDate)
            _DueDate = RegisterColumn(Function(x As SalesOrderHeader) x.DueDate)
            _ShipDate = RegisterColumn(Function(x As SalesOrderHeader) x.ShipDate)
            _Status = RegisterColumn(Function(x As SalesOrderHeader) x.Status)
            _OnlineOrderFlag = RegisterColumn(Function(x As SalesOrderHeader) x.OnlineOrderFlag)
            _SalesOrderNumber = RegisterColumn(Function(x As SalesOrderHeader) x.SalesOrderNumber)
            _PurchaseOrderNumber = RegisterColumn(Function(x As SalesOrderHeader) x.PurchaseOrderNumber)
            _AccountNumber = RegisterColumn(Function(x As SalesOrderHeader) x.AccountNumber)
            RegisterColumn(Function(x As SalesOrderHeader) x.CustomerID, Customer._CustomerID)
            RegisterColumn(Function(x As SalesOrderHeader) x.ShipToAddressID, Address._AddressID)
            RegisterColumn(Function(x As SalesOrderHeader) x.BillToAddressID, Address._AddressID)
            _ShipMethod = RegisterColumn(Function(x As SalesOrderHeader) x.ShipMethod)
            _CreditCardApprovalCode = RegisterColumn(Function(x As SalesOrderHeader) x.CreditCardApprovalCode)
            _SubTotal = RegisterColumn(Function(x As SalesOrderHeader) x.SubTotal)
            _TaxAmt = RegisterColumn(Function(x As SalesOrderHeader) x.TaxAmt)
            _Freight = RegisterColumn(Function(x As SalesOrderHeader) x.Freight)
            _TotalDue = RegisterColumn(Function(x As SalesOrderHeader) x.TotalDue)
            _Comment = RegisterColumn(Function(x As SalesOrderHeader) x.Comment)
        End Sub

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(SalesOrderID)
        End Function

        Private m_FK_Customer As Customer.PK
        Public ReadOnly Property FK_Customer As Customer.PK
            Get
                If m_FK_Customer Is Nothing Then m_FK_Customer = New Customer.PK(CustomerID)
                Return m_FK_Customer
            End Get
        End Property

        Private m_FK_ShipToAddress As Address.PK
        Public ReadOnly Property FK_ShipToAddress As Address.PK
            Get
                If m_FK_ShipToAddress Is Nothing Then m_FK_ShipToAddress = New Address.PK(ShipToAddressID)
                Return m_FK_ShipToAddress
            End Get
        End Property

        Private m_FK_ShipToCustomerAddress As CustomerAddress.PK
        Public ReadOnly Property FK_ShipToCustomerAddress As CustomerAddress.PK
            Get
                If m_FK_ShipToCustomerAddress Is Nothing Then m_FK_ShipToCustomerAddress = New CustomerAddress.PK(CustomerID, ShipToAddressID)
                Return m_FK_ShipToCustomerAddress
            End Get
        End Property

        Private m_FK_BillToAddress As Address.PK
        Public ReadOnly Property FK_BillToAddress As Address.PK
            Get
                If m_FK_BillToAddress Is Nothing Then m_FK_BillToAddress = New Address.PK(BillToAddressID)
                Return m_FK_BillToAddress
            End Get
        End Property

        Private m_FK_BillToCustomerAddress As CustomerAddress.PK
        Public ReadOnly Property FK_BillToCustomerAddress As CustomerAddress.PK
            Get
                If m_FK_BillToCustomerAddress Is Nothing Then m_FK_BillToCustomerAddress = New CustomerAddress.PK(CustomerID, BillToAddressID)
                Return m_FK_BillToCustomerAddress
            End Get
        End Property

        Private m_SalesOrderID As _Int32
        <Identity(1, 1)>
        <DbColumn(Description:="Primary key.")>
        Public Property SalesOrderID As _Int32
            Get
                Return m_SalesOrderID
            End Get
            Private Set
                m_SalesOrderID = Value
            End Set
        End Property

        Private m_RevisionNumber As _Byte
        <Required>
        <DefaultValue(CByte(0), Name:="DF_SalesOrderHeader_RevisionNumber")>
        <DbColumn(Description:="Incremental number to track changes to the sales order over time.")>
        Public Property RevisionNumber As _Byte
            Get
                Return m_RevisionNumber
            End Get
            Private Set
                m_RevisionNumber = Value
            End Set
        End Property

        Private m_OrderDate As _DateTime
        <Required>
        <AsDateTime>
        <AutoDateTime(Name:="DF_SalesOrderHeader_OrderDate")>
        <DbColumn(Description:="Dates the sales order was created.")>
        Public Property OrderDate As _DateTime
            Get
                Return m_OrderDate
            End Get
            Private Set
                m_OrderDate = Value
            End Set
        End Property

        Private m_DueDate As _DateTime
        <Required>
        <AsDateTime>
        <DbColumn(Description:="Date the order is due to the customer.")>
        Public Property DueDate As _DateTime
            Get
                Return m_DueDate
            End Get
            Private Set
                m_DueDate = Value
            End Set
        End Property

        Private m_ShipDate As _DateTime
        <AsDateTime>
        <DbColumn(Description:="Date the order was shipped to the customer.")>
        Public Property ShipDate As _DateTime
            Get
                Return m_ShipDate
            End Get
            Private Set
                m_ShipDate = Value
            End Set
        End Property

        Private m_Status As _ByteEnum(Of SalesOrderStatus)
        <Required>
        <DefaultValue(GetType(SalesOrderStatus), NameOf(SalesOrderStatus.InProcess), Name:="DF_SalesOrderHeader_Status")>
        <DbColumn(Description:="Order current status. 1 = In process; 2 = Approved; 3 = Backordered; 4 = Rejected; 5 = Shipped; 6 = Cancelled")>
        Public Property Status As _ByteEnum(Of SalesOrderStatus)
            Get
                Return m_Status
            End Get
            Private Set
                m_Status = Value
            End Set
        End Property

        Private m_OnlineOrderFlag As _Boolean
        <Required>
        <DefaultValue(True, Name:="DF_SalesOrderHeader_OnlineOrderFlag")>
        <DbColumn(Description:="0 = Order placed by sales person. 1 = Order placed online by customer.")>
        Public Property OnlineOrderFlag As _Boolean
            Get
                Return m_OnlineOrderFlag
            End Get
            Private Set
                m_OnlineOrderFlag = Value
            End Set
        End Property

        Private m_SalesOrderNumber As _String
        <UdtOrderNumber>
        <DbColumn(Description:="Unique sales order identification number.")>
        <Unique(Name:="AK_SalesOrderHeader_SalesOrderNumber", Description:="Unique nonclustered constraint.")>
        Public Property SalesOrderNumber As _String
            Get
                Return m_SalesOrderNumber
            End Get
            Private Set
                m_SalesOrderNumber = Value
            End Set
        End Property

        Private m_PurchaseOrderNumber As _String
        <UdtOrderNumber>
        <DbColumn(Description:="Customer purchase order number reference.")>
        Public Property PurchaseOrderNumber As _String
            Get
                Return m_PurchaseOrderNumber
            End Get
            Private Set
                m_PurchaseOrderNumber = Value
            End Set
        End Property

        Private m_AccountNumber As _String
        <UdtAccountNumber>
        <DbColumn(Description:="Financial accounting number reference.")>
        Public Property AccountNumber As _String
            Get
                Return m_AccountNumber
            End Get
            Private Set
                m_AccountNumber = Value
            End Set
        End Property

        Private m_CustomerID As _Int32
        <Required>
        <DbColumn(Description:="Customer identification number. Foreign key to Customer.CustomerID.")>
        <DbIndex("IX_SalesOrderHeader_CustomerID", Description:="Nonclustered index.")>
        Public Property CustomerID As _Int32
            Get
                Return m_CustomerID
            End Get
            Private Set
                m_CustomerID = Value
            End Set
        End Property

        Private m_ShipToAddressID As _Int32
        <DbColumn(Description:="The ID of the location to send goods.  Foreign key to the Address table.")>
        Public Property ShipToAddressID As _Int32
            Get
                Return m_ShipToAddressID
            End Get
            Private Set
                m_ShipToAddressID = Value
            End Set
        End Property

        Private m_BillToAddressID As _Int32
        <DbColumn(Description:="The ID of the location to send invoices.  Foreign key to the Address table.")>
        Public Property BillToAddressID As _Int32
            Get
                Return m_BillToAddressID
            End Get
            Private Set
                m_BillToAddressID = Value
            End Set
        End Property

        Private m_ShipMethod As _String
        <Required>
        <AsNVarChar(50)>
        <DbColumn(Description:="Shipping method. Foreign key to ShipMethod.ShipMethodID.")>
        Public Property ShipMethod As _String
            Get
                Return m_ShipMethod
            End Get
            Private Set
                m_ShipMethod = Value
            End Set
        End Property

        Private m_CreditCardApprovalCode As _String
        <AsNVarChar(15)>
        <DbColumn(Description:="Approval code provided by the credit card company.")>
        Public Property CreditCardApprovalCode As _String
            Get
                Return m_CreditCardApprovalCode
            End Get
            Private Set
                m_CreditCardApprovalCode = Value
            End Set
        End Property

        Private m_SubToal As _Decimal
        <Required>
        <AsMoney>
        <DefaultValue(GetType(Decimal), "0", Name:="DF_SalesOrderHeader_SubTotal")>
        <DbColumn(Description:="Sales subtotal. Computed as SUM(SalesOrderDetail.LineTotal)for the appropriate SalesOrderID.")>
        Public Property SubTotal As _Decimal
            Get
                Return m_SubToal
            End Get
            Private Set
                m_SubToal = Value
            End Set
        End Property

        Private m_TaxAmt As _Decimal
        <Required>
        <AsMoney>
        <DefaultValue(GetType(Decimal), "0", Name:="DF_SalesOrderHeader_TaxAmt")>
        <DbColumn(Description:="Tax amount.")>
        Public Property TaxAmt As _Decimal
            Get
                Return m_TaxAmt
            End Get
            Private Set
                m_TaxAmt = Value
            End Set
        End Property

        Private m_Freight As _Decimal
        <Required>
        <AsMoney>
        <DefaultValue(GetType(Decimal), "0", Name:="DF_SalesOrderHeader_Freight")>
        <DbColumn(Description:="Shipping cost.")>
        Public Property Freight As _Decimal
            Get
                Return m_Freight
            End Get
            Private Set(value As _Decimal)
                m_Freight = value
            End Set
        End Property

        Private m_TotalDue As _Decimal
        <Required>
        <AsMoney>
        <DbColumn(Description:="Total due from customer. Computed as Subtotal + TaxAmt + Freight.")>
        Public Property TotalDue As _Decimal
            Get
                Return m_TotalDue
            End Get
            Private Set
                m_TotalDue = Value
            End Set
        End Property

        Private m_Comment As _String
        <AsNVarCharMax>
        <DbColumn(Description:="Sales representative comments.")>
        Public Property Comment As _String
            Get
                Return m_Comment
            End Get
            Private Set
                m_Comment = Value
            End Set
        End Property

        <Computation>
        Private Sub ComputeSalesOrderNumber()
            SalesOrderNumber.ComputedAs((_String.[Const]("SO") + (CType(SalesOrderID, _String)).AsNVarChar(23)).IfNull(_String.[Const]("*** ERROR ***")))
        End Sub

        <Computation>
        Private Sub ComputeTotalDue()
            TotalDue.ComputedAs((SubTotal + TaxAmt + Freight).IfNull(_Decimal.[Const](0)))
        End Sub

        Private m_CK_SalesOrderHeader_DueDate As _Boolean
        <Check(GetType(UserMessages), NameOf(UserMessages.CK_SalesOrderHeader_DueDate), Name:=NameOf(CK_SalesOrderHeader_DueDate), Description:="Check constraint [DueDate] >= [OrderDate]")>
        Private ReadOnly Property CK_SalesOrderHeader_DueDate As _Boolean
            Get
                If m_CK_SalesOrderHeader_DueDate Is Nothing Then m_CK_SalesOrderHeader_DueDate = DueDate >= OrderDate
                Return m_CK_SalesOrderHeader_DueDate
            End Get
        End Property

        Private m_CK_SalesOrderHeader_Freight As _Boolean
        <Check(GetType(UserMessages), NameOf(UserMessages.CK_SalesOrderHeader_Freight), Name:=NameOf(CK_SalesOrderHeader_Freight), Description:="Check constraint [Freight] >= (0.00)")>
        Private ReadOnly Property CK_SalesOrderHeader_Freight As _Boolean
            Get
                If m_CK_SalesOrderHeader_Freight Is Nothing Then m_CK_SalesOrderHeader_Freight = Freight >= _Decimal.Const(0)
                Return m_CK_SalesOrderHeader_Freight
            End Get
        End Property

        Private m_CK_SalesOrderHeader_ShipDate As _Boolean
        <Check(GetType(UserMessages), NameOf(UserMessages.CK_SalesOrderHeader_ShipDate), Name:=NameOf(CK_SalesOrderHeader_ShipDate), Description:="Check constraint [ShipDate] >= [OrderDate] OR [ShipDate] IS NULL")>
        Private ReadOnly Property CK_SalesOrderHeader_ShipDate As _Boolean
            Get
                If m_CK_SalesOrderHeader_ShipDate Is Nothing Then m_CK_SalesOrderHeader_ShipDate = ShipDate >= OrderDate Or ShipDate.IsNull()
                Return m_CK_SalesOrderHeader_ShipDate
            End Get
        End Property

        Private m_CK_SalesOrderHeader_SubTotal As _Boolean
        <Check(GetType(UserMessages), NameOf(UserMessages.CK_SalesOrderHeader_SubTotal), Name:=NameOf(CK_SalesOrderHeader_SubTotal), Description:="Check constraint [SubTotal] >= (0.00)")>
        Private ReadOnly Property CK_SalesOrderHeader_SubTotal As _Boolean
            Get
                If m_CK_SalesOrderHeader_SubTotal Is Nothing Then m_CK_SalesOrderHeader_SubTotal = SubTotal >= _Decimal.Const(0)
                Return m_CK_SalesOrderHeader_SubTotal
            End Get
        End Property

        Private m_CK_SalesOrderHeader_TaxAmt As _Boolean
        <Check(GetType(UserMessages), NameOf(UserMessages.CK_SalesOrderHeader_TaxAmt), Name:=NameOf(CK_SalesOrderHeader_TaxAmt), Description:="Check constraint [TaxAmt] >= (0.00)")>
        Private ReadOnly Property CK_SalesOrderHeader_TaxAmt As _Boolean
            Get
                If m_CK_SalesOrderHeader_TaxAmt Is Nothing Then m_CK_SalesOrderHeader_TaxAmt = TaxAmt >= _Decimal.Const(0)
                Return m_CK_SalesOrderHeader_TaxAmt
            End Get
        End Property

        Private m_CK_SalesOrderHeader_Status As _Boolean
        <Check(GetType(UserMessages), NameOf(UserMessages.CK_SalesOrderHeader_Status), Name:=NameOf(CK_SalesOrderHeader_Status), Description:="Check constraint [Status] BETWEEN (1) AND (6)")>
        Private ReadOnly Property CK_SalesOrderHeader_Status As _Boolean
            Get
                If m_CK_SalesOrderHeader_Status Is Nothing Then m_CK_SalesOrderHeader_Status = IsValid(Status)
                Return m_CK_SalesOrderHeader_Status
            End Get
        End Property

        Private Shared Function IsValid(status As _ByteEnum(Of SalesOrderStatus)) As _Boolean
            Dim byteExpr = CType(status, _Byte)
            Return byteExpr >= _Byte.[Const](1) And byteExpr <= _Byte.[Const](6)
        End Function
    End Class
End Namespace

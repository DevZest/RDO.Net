<Computation("ComputeLineTotal")>
<CheckConstraint("CK_SalesOrderDetail_OrderQty", GetType(My.UserMessages), NameOf(My.UserMessages.CK_SalesOrderDetail_OrderQty), Description:="Check constraint [OrderQty] > (0)")>
<CheckConstraint("CK_SalesOrderDetail_UnitPrice", GetType(My.UserMessages), NameOf(My.UserMessages.CK_SalesOrderDetail_UnitPrice), Description:="heck constraint [UnitPrice] >= (0.00)")>
<CheckConstraint("CK_SalesOrderDetail_UnitPriceDiscount", GetType(My.UserMessages), NameOf(My.UserMessages.CK_SalesOrderDetail_UnitPriceDiscount), Description:="Check constraint [UnitPriceDiscount] >= (0.00)")>
<DbIndex("IX_SalesOrderDetail_ProductID", Description:="Nonclustered index.")>
Public Class SalesOrderDetail
    Inherits BaseModel(Of PK)

    <DbPrimaryKey("PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID", Description:="Clustered index created by a primary key constraint.")>
    Public NotInheritable Class PK
        Inherits CandidateKey

        Public Sub New(salesOrderID As _Int32, salesOrderDetailID As _Int32)
            MyBase.New(salesOrderID, salesOrderDetailID)
        End Sub
    End Class

    Public Class Key
        Inherits Key(Of PK)

        Shared Sub New()
            Register(Function(x As Key) x.SalesOrderID, _SalesOrderID)
            Register(Function(x As Key) x.SalesOrderDetailID, _SalesOrderDetailID)
        End Sub

        Protected Overrides Function CreatePrimaryKey() As PK
            Return New PK(SalesOrderID, SalesOrderDetailID)
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

        Private m_SalesOrderDetailID As _Int32
        Public Property SalesOrderDetailID As _Int32
            Get
                Return m_SalesOrderDetailID
            End Get
            Private Set
                m_SalesOrderDetailID = Value
            End Set
        End Property
    End Class

    Public Shared ReadOnly _SalesOrderID As Mounter(Of _Int32) = RegisterColumn(Function(x As SalesOrderDetail) x.SalesOrderID)
    Public Shared ReadOnly _SalesOrderDetailID As Mounter(Of _Int32) = RegisterColumn(Function(x As SalesOrderDetail) x.SalesOrderDetailID)
    Public Shared ReadOnly _OrderQty As Mounter(Of _Int16) = RegisterColumn(Function(x As SalesOrderDetail) x.OrderQty)
    Public Shared ReadOnly _ProductID As Mounter(Of _Int32) = RegisterColumn(Function(x As SalesOrderDetail) x.ProductID)
    Public Shared ReadOnly _UnitPrice As Mounter(Of _Decimal) = RegisterColumn(Function(x As SalesOrderDetail) x.UnitPrice)
    Public Shared ReadOnly _UnitPriceDiscount As Mounter(Of _Decimal) = RegisterColumn(Function(x As SalesOrderDetail) x.UnitPriceDiscount)
    Public Shared ReadOnly _LineTotal As Mounter(Of _Decimal) = RegisterColumn(Function(x As SalesOrderDetail) x.LineTotal)

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(SalesOrderID, SalesOrderDetailID)
    End Function

    Private m_FK_SalesOrderHeader As SalesOrderHeader.PK
    Public ReadOnly Property FK_SalesOrderHeader As SalesOrderHeader.PK
        Get
            If m_FK_SalesOrderHeader Is Nothing Then
                m_FK_SalesOrderHeader = New SalesOrderHeader.PK(SalesOrderID)
            End If
            Return m_FK_SalesOrderHeader
        End Get
    End Property

    Private m_FK_Product As Product.PK
    Public ReadOnly Property FK_Product As Product.PK
        Get
            If m_FK_Product Is Nothing Then
                m_FK_Product = New Product.PK(ProductID)
            End If
            Return m_FK_Product
        End Get
    End Property

    Private m_SalesOrderID As _Int32
    <DbColumn(Description:="Primary key. Foreign key to SalesOrderHeader.SalesOrderID.")>
    Public Property SalesOrderID As _Int32
        Get
            Return m_SalesOrderID
        End Get
        Private Set
            m_SalesOrderID = Value
        End Set
    End Property

    Private m_SalesOrderDetailID As _Int32
    <Identity>
    <DbColumn(Description:="Primary key. One incremental unique number per product sold.")>
    Public Property SalesOrderDetailID As _Int32
        Get
            Return m_SalesOrderDetailID
        End Get
        Private Set
            m_SalesOrderDetailID = Value
        End Set
    End Property

    Private m_OrderQty As _Int16
    <Required>
    <DbColumn(Description:="Quantity ordered per product.")>
    Public Property OrderQty As _Int16
        Get
            Return m_OrderQty
        End Get
        Private Set
            m_OrderQty = Value
        End Set
    End Property

    Private m_ProductID As _Int32
    <Required>
    <DbColumn(Description:="Product sold to customer. Foreign key to Product.ProductID.")>
    Public Property ProductID As _Int32
        Get
            Return m_ProductID
        End Get
        Private Set
            m_ProductID = Value
        End Set
    End Property

    Private m_UnitPrice As _Decimal
    <Required>
    <SqlMoney>
    <DbColumn(Description:="Selling price of a single product.")>
    Public Property UnitPrice As _Decimal
        Get
            Return m_UnitPrice
        End Get
        Private Set
            m_UnitPrice = Value
        End Set
    End Property

    Private m_UnitPriceDiscount As _Decimal
    <Required>
    <SqlMoney>
    <DefaultValue(GetType(Decimal), "0", Name:="DF_SalesOrderDetail_UnitPriceDiscount")>
    <DbColumn(Description:="Discount amount.")>
    Public Property UnitPriceDiscount As _Decimal
        Get
            Return m_UnitPriceDiscount
        End Get
        Private Set
            m_UnitPriceDiscount = Value
        End Set
    End Property

    Private m_LineTotal As _Decimal
    <Required>
    <SqlMoney>
    <DbColumn(Description:="Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.")>
    Public Property LineTotal As _Decimal
        Get
            Return m_LineTotal
        End Get
        Private Set
            m_LineTotal = Value
        End Set
    End Property

    <_Computation>
    Private Sub ComputeLineTotal()
        LineTotal.ComputedAs((UnitPrice * (_Decimal.[Const](1) - UnitPriceDiscount) * OrderQty).IfNull(_Decimal.[Const](0)))
    End Sub

    <_CheckConstraint>
    Private ReadOnly Property CK_SalesOrderDetail_OrderQty As _Boolean
        Get
            Return OrderQty > _Decimal.Const(0)
        End Get
    End Property

    <_CheckConstraint>
    Private ReadOnly Property CK_SalesOrderDetail_UnitPrice As _Boolean
        Get
            Return UnitPrice >= _Decimal.Const(0)
        End Get
    End Property

    <_CheckConstraint>
    Private ReadOnly Property CK_SalesOrderDetail_UnitPriceDiscount As _Boolean
        Get
            Return UnitPriceDiscount >= _Decimal.Const(0)
        End Get
    End Property

    <_DbIndex>
    Private ReadOnly Property IX_SalesOrderDetail_ProductID As ColumnSort()
        Get
            Return New ColumnSort() {ProductID}
        End Get
    End Property
End Class

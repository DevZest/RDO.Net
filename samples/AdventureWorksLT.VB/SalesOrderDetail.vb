Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer

Namespace DevZest.Samples.AdventureWorksLT
    Public Class SalesOrderDetail
        Inherits BaseModel(Of SalesOrderDetail.PK)

        <DbPrimaryKey("PK_SalesOrderDetail_SalesOrderID_SalesOrderDetailID", Description:="Clustered index created by a primary key constraint.")>
        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Sub New(salesOrderID As _Int32, salesOrderDetailID As _Int32)
                Me.SalesOrderID = salesOrderID
                Me.SalesOrderDetailID = salesOrderDetailID
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

        Public Shared Function GetKey(ByVal salesOrderID As Integer, ByVal salesOrderDetailID As Integer) As IDataValues
            Return DataValues.Create(_Int32.[Const](salesOrderID), _Int32.[Const](salesOrderDetailID))
        End Function

        Public NotInheritable Class Key
            Inherits Model(Of PK)

            Shared Sub New()
                RegisterColumn(Function(x As Key) x.SalesOrderID, SalesOrderHeader._SalesOrderID)
                RegisterColumn(Function(x As Key) x.SalesOrderDetailID, _SalesOrderDetailID)
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

        Public Class Ref
            Inherits Ref(Of PK)

            Shared Sub New()
                Register(Function(ByVal __ As Ref) __.SalesOrderID, AdventureWorksLT.SalesOrderHeader._SalesOrderID)
                Register(Function(ByVal __ As Ref) __.SalesOrderDetailID, _SalesOrderDetailID)
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

            Private m_SalesOrderDetailID As _Int32
            Public Property SalesOrderDetailID As _Int32
                Get
                    Return m_SalesOrderDetailID
                End Get
                Private Set
                    m_SalesOrderDetailID = Value
                End Set
            End Property

            Protected Overrides Function GetForeignKey() As PK
                Return New PK(SalesOrderID, SalesOrderDetailID)
            End Function
        End Class

        Public Shared ReadOnly _SalesOrderDetailID As Mounter(Of _Int32)
        Public Shared ReadOnly _OrderQty As Mounter(Of _Int16)
        Public Shared ReadOnly _UnitPrice As Mounter(Of _Decimal)
        Public Shared ReadOnly _UnitPriceDiscount As Mounter(Of _Decimal)
        Public Shared ReadOnly _LineTotal As Mounter(Of _Decimal)

        Shared Sub New()
            RegisterColumn(Function(x As SalesOrderDetail) x.SalesOrderID, SalesOrderHeader._SalesOrderID)
            _SalesOrderDetailID = RegisterColumn(Function(x As SalesOrderDetail) x.SalesOrderDetailID)
            _OrderQty = RegisterColumn(Function(x As SalesOrderDetail) x.OrderQty)
            RegisterColumn(Function(x As SalesOrderDetail) x.ProductID, Product._ProductID)
            _UnitPrice = RegisterColumn(Function(x As SalesOrderDetail) x.UnitPrice)
            _UnitPriceDiscount = RegisterColumn(Function(x As SalesOrderDetail) x.UnitPriceDiscount)
            _LineTotal = RegisterColumn(Function(x As SalesOrderDetail) x.LineTotal)
        End Sub

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(SalesOrderID, SalesOrderDetailID)
        End Function

        Private m_FK_SalesOrderHeader As SalesOrderHeader.PK
        Public ReadOnly Property FK_SalesOrderHeader As SalesOrderHeader.PK
            Get
                If m_FK_SalesOrderHeader Is Nothing Then m_FK_SalesOrderHeader = New SalesOrderHeader.PK(SalesOrderID)
                Return m_FK_SalesOrderHeader
            End Get
        End Property

        Private m_FK_Product As Product.PK
        Public ReadOnly Property FK_Product As Product.PK
            Get
                If m_FK_Product Is Nothing Then m_FK_Product = New Product.PK(ProductID)
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
        <Identity(1, 1)>
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
        <DbIndex("IX_SalesOrderDetail_ProductID", Description:="Nonclustered index.")>
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
        <AsMoney>
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
        <AsMoney>
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
        <AsMoney>
        <DbColumn(Description:="Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.")>
        Public Property LineTotal As _Decimal
            Get
                Return m_LineTotal
            End Get
            Private Set
                m_LineTotal = Value
            End Set
        End Property

        <Computation>
        Private Sub ComputeLineTotal()
            LineTotal.ComputedAs((UnitPrice * (_Decimal.[Const](1) - UnitPriceDiscount) * OrderQty).IfNull(_Decimal.[Const](0)))
        End Sub

        Private m_CK_SalesOrderDetail_OrderQty As _Boolean
        <Check(GetType(UserMessages), NameOf(UserMessages.CK_SalesOrderDetail_OrderQty), Name:=NameOf(CK_SalesOrderDetail_OrderQty), Description:="Check constraint [OrderQty] > (0)")>
        Private ReadOnly Property CK_SalesOrderDetail_OrderQty As _Boolean
            Get
                If m_CK_SalesOrderDetail_OrderQty Is Nothing Then m_CK_SalesOrderDetail_OrderQty = OrderQty > _Decimal.Const(0)
                Return m_CK_SalesOrderDetail_OrderQty
            End Get
        End Property

        Private m_CK_SalesOrderDetail_UnitPrice As _Boolean
        <Check(GetType(UserMessages), NameOf(UserMessages.CK_SalesOrderDetail_UnitPrice), Name:=NameOf(CK_SalesOrderDetail_UnitPrice), Description:="heck constraint [UnitPrice] >= (0.00)")>
        Private ReadOnly Property CK_SalesOrderDetail_UnitPrice As _Boolean
            Get
                If m_CK_SalesOrderDetail_UnitPrice Is Nothing Then m_CK_SalesOrderDetail_UnitPrice = UnitPrice >= _Decimal.Const(0)
                Return m_CK_SalesOrderDetail_UnitPrice
            End Get
        End Property

        Private m_CK_SalesOrderDetail_UnitPriceDiscount As _Boolean

        <Check(GetType(UserMessages), NameOf(UserMessages.CK_SalesOrderDetail_UnitPriceDiscount), Name:=NameOf(CK_SalesOrderDetail_UnitPriceDiscount), Description:="Check constraint [UnitPriceDiscount] >= (0.00)")>
        Private ReadOnly Property CK_SalesOrderDetail_UnitPriceDiscount As _Boolean
            Get
                If m_CK_SalesOrderDetail_UnitPriceDiscount Is Nothing Then m_CK_SalesOrderDetail_UnitPriceDiscount = UnitPriceDiscount >= _Decimal.Const(0)
                Return m_CK_SalesOrderDetail_UnitPriceDiscount
            End Get
        End Property
    End Class
End Namespace

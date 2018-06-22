Namespace DevZest.Samples.AdventureWorksLT
    Public Class SalesOrderInfo
        Inherits SalesOrder

        Shared Sub New()
            RegisterColumnGroup(Function(x As SalesOrderInfo) x.LK_Customer)
            RegisterColumnGroup(Function(x As SalesOrderInfo) x.LK_ShipToAddress)
            RegisterColumnGroup(Function(x As SalesOrderInfo) x.LK_BillToAddress)
        End Sub

        Private m_LK_Customer As Customer.Lookup
        Public Property LK_Customer As Customer.Lookup
            Get
                Return m_LK_Customer
            End Get
            Private Set
                m_LK_Customer = Value
            End Set
        End Property

        Private m_LK_ShipToAddress As Address.Lookup
        Public Property LK_ShipToAddress As Address.Lookup
            Get
                Return m_LK_ShipToAddress
            End Get
            Private Set
                m_LK_ShipToAddress = Value
            End Set
        End Property

        Private m_LK_BillToAddress As Address.Lookup
        Public Property LK_BillToAddress As Address.Lookup
            Get
                Return m_LK_BillToAddress
            End Get
            Private Set
                m_LK_BillToAddress = Value
            End Set
        End Property

        Public Overloads ReadOnly Property SalesOrderDetails As SalesOrderInfoDetail
            Get
                Return CType(MyBase.SalesOrderDetails, SalesOrderInfoDetail)
            End Get
        End Property

        Protected NotOverridable Overrides Function CreateSalesOrderDetail() As SalesOrderDetail
            Return CreateSalesOrderDetailInfo()
        End Function

        Protected Overridable Function CreateSalesOrderDetailInfo() As SalesOrderInfoDetail
            Return New SalesOrderInfoDetail()
        End Function
    End Class
End Namespace

Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace DevZest.Samples.AdventureWorksLT
    <ExtraColumns(GetType(SalesOrderInfo.Ext))>
    Public Class SalesOrderInfo
        Inherits SalesOrder

        Public Class Ext
            Inherits BranchProjection

            Shared Sub New()
                Register(Function(x As Ext) x.Customer)
                Register(Function(x As Ext) x.ShipToAddress)
                Register(Function(x As Ext) x.BillToAddress)
            End Sub

            Private m_Customer As Customer.Lookup
            Public Property Customer As Customer.Lookup
                Get
                    Return m_Customer
                End Get
                Private Set
                    m_Customer = Value
                End Set
            End Property

            Private m_ShipToAddress As Address.Lookup
            Public Property ShipToAddress As Address.Lookup
                Get
                    Return m_ShipToAddress
                End Get
                Private Set
                    m_ShipToAddress = Value
                End Set
            End Property

            Private m_BillToAddress As Address.Lookup
            Public Property BillToAddress As Address.Lookup
                Get
                    Return m_BillToAddress
                End Get
                Private Set
                    m_BillToAddress = Value
                End Set
            End Property
        End Class

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

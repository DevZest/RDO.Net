Imports DevZest.Samples.AdventureWorksLT

<InvisibleToDbDesigner>
Public Class SalesOrder
    Inherits SalesOrderBase

    Shared Sub New()
        RegisterChildModel(Function(x As SalesOrder) x.SalesOrderDetails, Function(x As SalesOrderDetail) x.FK_SalesOrderHeader)
    End Sub

    Private m_SalesOrderDetails As SalesOrderDetail
    Public Property SalesOrderDetails As SalesOrderDetail
        Get
            Return m_SalesOrderDetails
        End Get
        Private Set
            m_SalesOrderDetails = Value
        End Set
    End Property

    Protected Overrides Function GetSalesOrderDetails() As SalesOrderDetail
        Return SalesOrderDetails
    End Function
End Class

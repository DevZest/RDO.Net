Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace DevZest.Samples.AdventureWorksLT
    Public Class SalesOrder
        Inherits SalesOrderHeader

        Shared Sub New()
            RegisterColumn(Function(x As SalesOrder) x.LineCount)
            RegisterChildModel(Function(x As SalesOrder) x.SalesOrderDetails, Function(x As SalesOrderDetail) x.FK_SalesOrderHeader, Function(x) x.CreateSalesOrderDetail())
        End Sub

        Private m_LineCount As _Int32
        Public Property LineCount As _Int32
            Get
                Return m_LineCount
            End Get
            Private Set
                m_LineCount = Value
            End Set
        End Property

        <ModelValidator>
        Private Function ValidateLineCount(ByVal dataRow As DataRow) As DataValidationError
            Return If(LineCount(dataRow) > 0, Nothing, New DataValidationError(UserMessages.Validation_SalesOrder_LineCount, LineCount))
        End Function

        <Computation(ComputationMode.Aggregate)>
        Private Sub ComputeLineCount()
            LineCount.ComputedAs(SalesOrderDetails.SalesOrderDetailID.CountRows())
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

        Protected Overridable Function CreateSalesOrderDetail() As SalesOrderDetail
            Return New SalesOrderDetail()
        End Function

        <Computation(ComputationMode.Aggregate)>
        Private Sub ComputeSubTotal()
            SubTotal.ComputedAs(SalesOrderDetails.LineTotal.Sum().IfNull(0), False)
        End Sub
    End Class
End Namespace

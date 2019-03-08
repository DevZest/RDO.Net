<Validator(SalesOrder._ValidateLineCount, SourceColumns:={NameOf(SalesOrder.LineCount)})>
<Computation(SalesOrder._ComputeLineCount, ComputationMode.Aggregate)>
    <Computation(SalesOrder._ComputeSubTotal, ComputationMode.Aggregate)>
        <InvisibleToDbDesigner>
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

    Friend Const _ValidateLineCount = NameOf(ValidateLineCount)
    <_Validator>
    Private Function ValidateLineCount(ByVal dataRow As DataRow) As DataValidationError
        Return If(LineCount(dataRow) > 0, Nothing, New DataValidationError(My.UserMessages.Validation_SalesOrder_LineCount, LineCount))
    End Function

    Friend Const _ComputeLineCount = NameOf(ComputeLineCount)
    <_Computation>
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

    Friend Const _ComputeSubTotal = NameOf(ComputeSubTotal)
    <_Computation>
    Private Sub ComputeSubTotal()
        SubTotal.ComputedAs(SalesOrderDetails.LineTotal.Sum().IfNull(0), False)
    End Sub
End Class

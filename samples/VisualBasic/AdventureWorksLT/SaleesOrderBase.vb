<CustomValidator(SalesOrder._VAL_LineCount)>
<Computation(SalesOrder._ComputeLineCount, ComputationMode.Aggregate)>
<Computation(SalesOrder._ComputeSubTotal, ComputationMode.Aggregate)>
<InvisibleToDbDesigner>
Public MustInherit Class SalesOrderBase
    Inherits SalesOrderHeader

    Shared Sub New()
        RegisterColumn(Function(x As SalesOrder) x.LineCount)
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

    Friend Const _VAL_LineCount = NameOf(VAL_LineCount)
    <_CustomValidator>
    Private ReadOnly Property VAL_LineCount As CustomValidatorEntry
        Get
            Dim validate =
                Function(dataRow As DataRow) As String
                    Return If(LineCount(dataRow) > 0, Nothing, My.UserMessages.Validation_SalesOrder_LineCount)
                End Function

            Dim getSourceColumns =
                Function() As IColumns
                    Return LineCount
                End Function

            Return New CustomValidatorEntry(validate, getSourceColumns)
        End Get
    End Property

    Protected MustOverride Function GetSalesOrderDetails() As SalesOrderDetail

    Friend Const _ComputeLineCount = NameOf(ComputeLineCount)
    <_Computation>
    Private Sub ComputeLineCount()
        LineCount.ComputedAs(GetSalesOrderDetails().SalesOrderDetailID.CountRows())
    End Sub

    Friend Const _ComputeSubTotal = NameOf(ComputeSubTotal)
    <_Computation>
    Private Sub ComputeSubTotal()
        SubTotal.ComputedAs(GetSalesOrderDetails().LineTotal.Sum().IfNull(0), False)
    End Sub
End Class

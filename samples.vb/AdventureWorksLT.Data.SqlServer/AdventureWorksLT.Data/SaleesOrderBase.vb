Imports DevZest.Data

<CustomValidator("VAL_LineCount")>
<Computation("ComputeLineCount", ComputationMode.Aggregate)>
<Computation("ComputeSubTotal", ComputationMode.Aggregate)>
<InvisibleToDbDesigner>
Public MustInherit Class SalesOrderBase(Of T As {SalesOrderDetail, New})
    Inherits SalesOrderHeader

    Shared Sub New()
        RegisterColumn(Function(x As SalesOrderBase(Of T)) x.LineCount)
        RegisterChildModel(Function(x As SalesOrderBase(Of T)) x.SalesOrderDetails, Function(x As T) x.FK_SalesOrderHeader)
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

    Private m_SalesOrderDetails As T
    Public Property SalesOrderDetails As T
        Get
            Return m_SalesOrderDetails
        End Get
        Private Set
            m_SalesOrderDetails = Value
        End Set
    End Property

    <_Computation>
    Private Sub ComputeLineCount()
        LineCount.ComputedAs(SalesOrderDetails.SalesOrderDetailID.CountRows())
    End Sub

    <_Computation>
    Private Sub ComputeSubTotal()
        SubTotal.ComputedAs(SalesOrderDetails.LineTotal.Sum().IfNull(0), False)
    End Sub
End Class

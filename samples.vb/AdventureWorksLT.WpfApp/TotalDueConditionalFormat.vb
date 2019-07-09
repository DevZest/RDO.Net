Imports DevZest.Data
Imports DevZest.Data.Presenters

Public NotInheritable Class TotalDueConditionalFormat
    Inherits RowBindingBehavior(Of TextBlock)
    Public Sub New(totalAmt As _Decimal)
        _totalAmt = totalAmt
    End Sub

    Private _totalAmt As _Decimal

    Protected Overrides Sub Setup(view As TextBlock, presenter As RowPresenter)
    End Sub

    Protected Overrides Sub Refresh(view As TextBlock, presenter As RowPresenter)
        view.FontWeight = If(presenter.GetValue(_totalAmt) >= 10000, FontWeights.Bold, FontWeights.Normal)
    End Sub

    Protected Overrides Sub Cleanup(view As TextBlock, presenter As RowPresenter)
    End Sub
End Class
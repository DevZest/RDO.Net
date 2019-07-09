Imports DevZest.Data.Presenters

Public NotInheritable Class TotalAmtConditionalFormat
    Inherits ScalarBindingBehavior(Of TextBlock)
    Public Sub New(calculation As Func(Of Decimal?))
        _calculation = calculation
    End Sub

    Private _calculation As Func(Of Decimal?)

    Protected Overrides Sub Setup(view As TextBlock, presenter As ScalarPresenter)
    End Sub

    Protected Overrides Sub Refresh(view As TextBlock, presenter As ScalarPresenter)
        view.FontWeight = If(_calculation() >= 10000, FontWeights.Bold, FontWeights.Normal)
    End Sub

    Protected Overrides Sub Cleanup(view As TextBlock, presenter As ScalarPresenter)
    End Sub
End Class
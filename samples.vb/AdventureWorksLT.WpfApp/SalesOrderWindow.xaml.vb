Imports DevZest.Data

Public Class SalesOrderWindow
    Public NotInheritable Class Commands
        Private Sub New()
        End Sub
        Public Shared ReadOnly Submit As New RoutedCommand(NameOf(Submit), GetType(SalesOrderWindow))
    End Class

    Public Sub New()
        InitializeComponent()
        CommandBindings.Add(New CommandBinding(Commands.Submit, AddressOf ExecSubmit, AddressOf CanExecSubmit))
    End Sub

    Private Sub ExecSubmit(sender As Object, e As ExecutedRoutedEventArgs)
        If _presenter.IsEditing Then
            _presenter.CurrentRow.EndEdit()
        End If
        If CurrentRowDetailPresenter.IsEditing Then
            CurrentRowDetailPresenter.CurrentRow.EndEdit()
        End If

        If Not _presenter.SubmitInput() Then
            Return
        End If

        Dim salesOrderId As Integer? = Nothing
        If App.Execute(AddressOf _presenter.SaveToDb, Me, "Saving...", salesOrderId) Then
            Close()
            _action?.Invoke(salesOrderId)
        End If
    End Sub

    Private Sub CanExecSubmit(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = _presenter.CanSubmitInput AndAlso CurrentRowDetailPresenter.CanSubmitInput
    End Sub

    Private _presenter As Presenter

    Private ReadOnly Property CurrentRowDetailPresenter() As DetailPresenter
        Get
            Return _presenter.CurrentRowDetailPresenter
        End Get
    End Property

    Private _action As Action(Of Integer?)
    Public Overloads Sub Show(data As DataSet(Of SalesOrderInfo), ownerWindow As Window, action As Action(Of Integer?))
        Debug.Assert(data.Count = 1)

        _presenter = New Presenter(Me, _addressLookupPopup)
        _presenter.Show(_dataView, data)
        Owner = ownerWindow
        Title = If(_presenter.IsNew, "New Sales Order", String.Format("Sales Order: {0}", _presenter.SalesOrderId))
        _action = action
        ShowDialog()
    End Sub
End Class

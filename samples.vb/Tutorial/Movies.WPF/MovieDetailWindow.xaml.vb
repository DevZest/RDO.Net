Imports DevZest.Data

Partial Public Class MovieDetailWindow
    Public NotInheritable Class Commands
        Private Sub New()
        End Sub

        Public Shared ReadOnly Submit As New RoutedCommand(NameOf(Submit), GetType(MovieDetailWindow))
    End Class

    Public Overloads Shared Function Show(data As DataSet(Of Movie), ownerWindow As Window) As Boolean
        Return New MovieDetailWindow().ShowDialog(data, ownerWindow)
    End Function

    Public Sub New()
        InitializeComponent()
        CommandBindings.Add(New CommandBinding(Commands.Submit, AddressOf ExecSubmit, AddressOf CanExecSubmit))
    End Sub

    Private _presenter As Presenter

    Private Async Sub ExecSubmit(sender As Object, e As ExecutedRoutedEventArgs)
        If _presenter.IsEditing Then
            _presenter.CurrentRow.EndEdit()
        End If

        If Not _presenter.SubmitInput() Then
            Return
        End If

        Await _presenter.SaveToDbAsync()
        DialogResult = True
        Close()
    End Sub

    Private Sub CanExecSubmit(sender As Object, e As CanExecuteRoutedEventArgs)
        e.CanExecute = _presenter.CanSubmitInput
    End Sub

    Private Overloads Function ShowDialog(data As DataSet(Of Movie), ownerWindow As Window) As Boolean
        Debug.Assert(data.Count = 1)
        _presenter = New Presenter()
        _presenter.Show(_dataView, data)
        Owner = ownerWindow
        Title = If(_presenter.IsNew, "New Movie", String.Format("Movie: {0}", _presenter.ID))
        Return ShowDialog().Value
    End Function

End Class

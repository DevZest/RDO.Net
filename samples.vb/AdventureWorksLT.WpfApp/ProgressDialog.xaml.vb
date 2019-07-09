Imports System.Threading

Public Structure ProgressDialogResult(Of T)
    Friend Sub New(value As T, exception As Exception)
        Me.Value = value
        Me.Exception = exception
    End Sub

    Public ReadOnly Value As T
    Public ReadOnly Exception As Exception
End Structure

Public Structure ProgressDialogResult
    Friend Sub New(exception As Exception)
        Me.Exception = exception
    End Sub

    Public ReadOnly Exception As Exception
End Structure

Public Class ProgressDialog
    Private Sub New()
        InitializeComponent()
    End Sub

    Private Property Exception() As Exception

    Public Property Label() As String
        Get
            Return _textLabel.Text
        End Get
        Set
            _textLabel.Text = Value
        End Set
    End Property

    Private _cancellationTokenSource As CancellationTokenSource

    Private Sub OnCancelButtonClick(sender As Object, e As EventArgs)
        _cancellationTokenSource.Cancel()
        _cancelButton.IsEnabled = False
    End Sub

    Public Shared Function Execute(Of T)(func As Func(Of CancellationToken, Task(Of T)), ownerWindow As Window, Optional windowTitle As String = Nothing, Optional label As String = Nothing) As ProgressDialogResult(Of T)
        Dim progressDialog = CreateProgressDialog(ownerWindow, windowTitle, label)
        Dim value = progressDialog.ShowDialog(func)
        Return New ProgressDialogResult(Of T)(value, progressDialog.Exception)
    End Function

    Private Overloads Function ShowDialog(Of T)(func As Func(Of CancellationToken, Task(Of T))) As T
        _cancellationTokenSource = New CancellationTokenSource()
        Dim task = func(_cancellationTokenSource.Token)
        Return ShowDialog(task)
    End Function

    Private MustInherit Class AsyncActionBase
        Protected Sub New(dialog As ProgressDialog)
            Me.Dialog = dialog
        End Sub

        Public Property Dialog() As ProgressDialog

        Public Async Sub Run()
            Try
                Await RunAsync()
            Catch ex As Exception
                Dialog.Exception = ex
            Finally
                Dialog.Close()
            End Try
        End Sub

        Protected MustOverride Function RunAsync() As Task
    End Class

    Private NotInheritable Class AsyncAction(Of T)
        Inherits AsyncActionBase
        Public Sub New(task As Task(Of T), dialog As ProgressDialog)
            MyBase.New(dialog)
            _task = task
        End Sub

        Private ReadOnly _task As Task(Of T)

        Public Property Result() As T

        Protected Overrides Async Function RunAsync() As Task
            Result = Await _task
        End Function
    End Class

    Private Overloads Function ShowDialog(Of T)(task As Task(Of T)) As T
        Dim asyncAction As New AsyncAction(Of T)(task, Me)
        ShowDialog(asyncAction)
        Return asyncAction.Result
    End Function

    Public Shared Function Execute(func As Func(Of CancellationToken, Task), ownerWindow As Window, Optional windowTitle As String = Nothing, Optional label As String = Nothing) As ProgressDialogResult
        Dim progressDialog = CreateProgressDialog(ownerWindow, windowTitle, label)
        progressDialog.ShowDialog(func)
        Return New ProgressDialogResult(progressDialog.Exception)
    End Function

    Private Shared Function CreateProgressDialog(ownerWindow As Window, windowTitle As String, label As String) As ProgressDialog
        Return New ProgressDialog() With {
            .Owner = ownerWindow,
            .Title = If(windowTitle, "Executing..."),
            .Label = If(label, "Please wait...")
        }
    End Function

    Private Overloads Sub ShowDialog(func As Func(Of CancellationToken, Task))
        _cancellationTokenSource = New CancellationTokenSource()
        Dim task = func(_cancellationTokenSource.Token)
        ShowDialog(task)
    End Sub

    Private NotInheritable Class AsyncAction
        Inherits AsyncActionBase
        Public Sub New(task As Task, dialog As ProgressDialog)
            MyBase.New(dialog)
            _task = task
        End Sub

        Private ReadOnly _task As Task

        Protected Overrides Async Function RunAsync() As Task
            Await _task
        End Function
    End Class

    Private Overloads Sub ShowDialog(task As Task)
        Dim asyncAction = New AsyncAction(task, Me)
        ShowDialog(asyncAction)
    End Sub

    Private _asyncAction As AsyncActionBase
    Private Overloads Sub ShowDialog(asyncAction As AsyncActionBase)
        _asyncAction = asyncAction
        ShowDialog()
    End Sub

    Protected Overrides Sub OnContentRendered(e As EventArgs)
        MyBase.OnContentRendered(e)
        _asyncAction.Run()
    End Sub
End Class

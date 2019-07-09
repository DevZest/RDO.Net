Imports System.Reflection
Imports System.IO
Imports System.Threading

''' <summary>
''' Interaction logic for App.xaml
''' </summary>
Partial Public Class App
    Inherits Application
    Public Shared ReadOnly ConnectionString As String = GetConnectionString()

    Private Shared Function GetConnectionString() As String
        Dim mdfFilename As String = "AdventureWorksLT.mdf"
        Dim outputFolder As String = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        Dim attachDbFilename As String = Path.Combine(outputFolder, mdfFilename)
        Return String.Format("Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename)
    End Function

    Private Shared Function CreateDb() As Db
        Return New Db(App.ConnectionString)
    End Function

    Public Shared Async Function ExecuteAsync(func As Func(Of Db, Task)) As Task
        Using db = CreateDb()
            Await func(db)
        End Using
    End Function

    Public Shared Async Function ExecuteAsync(Of T)(func As Func(Of Db, Task(Of T))) As Task(Of T)
        Using db = CreateDb()
            Return Await func(db)
        End Using
    End Function

    Public Shared Function Execute(func As Func(Of Db, CancellationToken, Task), ownerWindow As Window, Optional windowTitle As String = Nothing, Optional label As String = Nothing) As Boolean
        Return Execute(Function(ct) ExecuteAsync(Function(db) func(db, ct)), ownerWindow, windowTitle, label)
    End Function

    Public Shared Function Execute(func As Func(Of CancellationToken, Task), ownerWindow As Window, Optional windowTitle As String = Nothing, Optional label As String = Nothing) As Boolean
        Dim dialogResult = ProgressDialog.Execute(func, ownerWindow, windowTitle, label)
        Dim exception = dialogResult.Exception
        If exception IsNot Nothing Then
            MessageBox.Show(exception.Message, windowTitle)
            Return False
        End If
        Return True
    End Function

    Public Shared Function Execute(Of T)(func As Func(Of CancellationToken, Task(Of T)), ownerWindow As Window, windowTitle As String, ByRef result As T) As Boolean
        Return Execute(func, ownerWindow, windowTitle, Nothing, result)
    End Function

    Public Shared Function Execute(Of T)(func As Func(Of CancellationToken, Task(Of T)), ownerWindow As Window, windowTitle As String, label As String, ByRef result As T) As Boolean
        Dim dialogResult = ProgressDialog.Execute(func, ownerWindow, windowTitle, label)
        Dim exception = dialogResult.Exception
        If exception IsNot Nothing Then
            MessageBox.Show(exception.Message, windowTitle)
            result = Nothing
            Return False
        End If

        result = dialogResult.Value
        Return True
    End Function

    Public Shared Function Execute(Of T)(func As Func(Of Db, CancellationToken, Task(Of T)), ownerWindow As Window, ByRef result As T) As Boolean
        Return Execute(func, ownerWindow, Nothing, result)
    End Function

    Public Shared Function Execute(Of T)(func As Func(Of Db, CancellationToken, Task(Of T)), ownerWindow As Window, windowTitle As String, ByRef result As T) As Boolean
        Return Execute(func, ownerWindow, windowTitle, Nothing, result)
    End Function

    Public Shared Function Execute(Of T)(func As Func(Of Db, CancellationToken, Task(Of T)), ownerWindow As Window, windowTitle As String, label As String, ByRef result As T) As Boolean
        Return Execute(Function(ct) ExecuteAsync(Function(db) func(db, ct)), ownerWindow, windowTitle, label, result)
    End Function
End Class

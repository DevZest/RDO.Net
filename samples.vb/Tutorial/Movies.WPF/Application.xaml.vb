Imports System.IO
Imports System.Reflection

Class Application
    Private Shared ReadOnly ConnectionString As String = GetConnectionString()

    Private Shared Function GetConnectionString() As String
        Dim mdfFilename As String = "Movies.mdf"
        Dim outputFolder As String = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        Dim attachDbFilename As String = Path.Combine(outputFolder, mdfFilename)
        Return String.Format("Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename)
    End Function

    Private Shared Function CreateDb() As Db
        Return New Db(ConnectionString)
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
End Class

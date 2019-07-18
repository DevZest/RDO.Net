Imports System.IO
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class DbTests
    Private Shared Function GetConnectionString() As String
        Dim mdfFilename As String = "EmptyDb.mdf"
        Dim outputFolder As String = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        Dim attachDbFilename As String = Path.Combine(outputFolder, mdfFilename)
        Return String.Format("Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename)
    End Function

    Private Shared Function CreateDb() As Db
        Return New Db(GetConnectionString())
    End Function

    <TestMethod>
    Public Async Function Db_GetMovies() As Task
        Using db = Await MockMovie.CreateAsync(CreateDb())
            Dim result As Object = Await db.GetMoviesAsync("comedy")
            Assert.AreEqual(3, result.Count)
        End Using

        Using db = Await MockMovie.CreateAsync(CreateDb())
            Dim result As Object = Await db.GetMoviesAsync("ghost")
            Assert.AreEqual(2, result.Count)
        End Using
    End Function

    <TestMethod>
    Public Async Function Db_GetMovie() As Task
        Using db = Await MockMovie.CreateAsync(CreateDb())
            Dim result As Object = Await db.GetMovieAsync(1)
            Assert.AreEqual(1, result.Count)
        End Using
    End Function
End Class

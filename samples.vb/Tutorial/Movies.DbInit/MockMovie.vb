Imports DevZest.Data
Imports DevZest.Data.DbInit
Imports DevZest.Data.Primitives
Imports System.Threading

Public Class MockMovie
    Inherits DbMock(Of Db)

    Public Shared Function CreateAsync(db As Db, Optional progress As IProgress(Of DbInitProgress) = Nothing, Optional ct As CancellationToken = Nothing) As Task(Of Db)
        Return New MockMovie().MockAsync(db, progress, ct)
    End Function

    Private Shared Function GetMovies() As DataSet(Of Movie)
        Dim result As DataSet(Of Movie) = DataSet(Of Movie).Create().AddRows(4)
        Dim x As Movie = result.Entity
        x.SuspendIdentity()
        x.ID(0) = 1
        x.ID(1) = 2
        x.ID(2) = 3
        x.ID(3) = 4
        x.Title(0) = "When Harry Met Sally"
        x.Title(1) = "Ghostbusters"
        x.Title(2) = "Ghostbusters 2"
        x.Title(3) = "Rio Bravo"
        x.ReleaseDate(0) = Convert.ToDateTime("1989-02-12T00:00:00")
        x.ReleaseDate(1) = Convert.ToDateTime("1984-03-13T00:00:00")
        x.ReleaseDate(2) = Convert.ToDateTime("1986-02-23T00:00:00")
        x.ReleaseDate(3) = Convert.ToDateTime("1959-04-15T00:00:00")
        x.Genre(0) = "Romantic Comedy"
        x.Genre(1) = "Comedy"
        x.Genre(2) = "Comedy"
        x.Genre(3) = "Western"
        x.Price(0) = 7.99D
        x.Price(1) = 8.99D
        x.Price(2) = 9.99D
        x.Price(3) = 3.99D
        x.ResumeIdentity()
        Return result
    End Function

    Protected Overrides Sub Initialize()
        Mock(Db.Movie, AddressOf GetMovies)
    End Sub
End Class

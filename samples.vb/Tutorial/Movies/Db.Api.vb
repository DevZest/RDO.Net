Imports System.Threading
Imports DevZest.Data

Partial Class Db
    Public Function GetMoviesAsync(text As String, Optional ct As CancellationToken = Nothing) As Task(Of DataSet(Of Movie))
        Dim result As DbSet(Of Movie) = Movie

        If Not String.IsNullOrWhiteSpace(text) Then
            result = Filter(result, text)
        End If

        Return result.ToDataSetAsync(ct)
    End Function

    Private Shared Function Filter(movies As DbSet(Of Movie), text As _String) As DbSet(Of Movie)
        Return movies.Where(Function(x) x.Title.Contains(text) Or x.Genre.Contains(text))
    End Function

    Public Function GetMovieAsync(id As _Int32, Optional ct As CancellationToken = Nothing) As Task(Of DataSet(Of Movie))
        Return Movie.Where(Function(x) x.ID = id).ToDataSetAsync(ct)
    End Function

End Class

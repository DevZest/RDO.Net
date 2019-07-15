Imports System.IO
Imports DevZest.Data.DbDesign

<EmptyDb>
Public Class _DevDb
    Inherits DbSessionProvider(Of Db)

    Public Overrides Function Create(projectPath As String) As Db
        Dim dbFolder = Path.Combine(projectPath, "LocalDb")
        Dim attachDbFilename = Path.Combine(dbFolder, "Movies.mdf")
        File.Copy(Path.Combine(dbFolder, "EmptyDb.mdf"), attachDbFilename, True)
        File.Copy(Path.Combine(dbFolder, "EmptyDb_log.ldf"), Path.Combine(dbFolder, "Movies_log.ldf"), True)
        Dim connectionString = String.Format("Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename)
        Return New Db(connectionString)
    End Function
End Class
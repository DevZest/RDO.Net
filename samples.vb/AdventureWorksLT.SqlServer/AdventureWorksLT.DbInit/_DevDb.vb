#If DbInit Then

Imports System.IO
Imports DevZest.Data.DbInit

<EmptyDb>
Public Class _DevDb
    Inherits DbSessionProvider(Of Db)

    Public Overrides Function Create(projectPath As String) As Db
        Dim dbFolder = Path.Combine(projectPath, "LocalDb")
        Dim attachDbFilename = Path.Combine(dbFolder, "AdventureWorksLT.DbInit.mdf")
        File.Copy(Path.Combine(dbFolder, "EmptyDb.mdf"), attachDbFilename, True)
        File.Copy(Path.Combine(dbFolder, "EmptyDb_log.ldf"), Path.Combine(dbFolder, "AdventureWorksLT.DbInit_log.ldf"), True)
        Dim connectionString = String.Format("Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename)
        Return New Db(connectionString)
    End Function
End Class

#End If

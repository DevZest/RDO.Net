﻿Imports System.IO
Imports DevZest.Data.DbDesign

Public Class DevDb
    Inherits DbSessionProvider(Of Db)

    Public Overrides Function Create(projectPath As String) As Db
        Dim dbFolder = Path.Combine(projectPath, "LocalDb")
        Dim attachDbFilename = Path.Combine(dbFolder, "Movies.mdf")
        Dim connectionString = String.Format("Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename)
        Return New Db(connectionString)
    End Function
End Class
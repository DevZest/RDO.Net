Imports DevZest.Data
Imports System.Data.SqlClient
Imports DevZest.Data.SqlServer

Partial Public Class Db
    Inherits SqlSession
    Public Sub New(connectionString As String)
        Me.New(New SqlConnection(connectionString))
    End Sub

    Public Sub New(sqlConnection As SqlConnection)
        MyBase.New(sqlConnection)
    End Sub

    Private m_Movie As DbTable(Of Movie)
    Public ReadOnly Property Movie As DbTable(Of Movie)
        Get
            Return GetTable(m_Movie)
        End Get
    End Property
End Class
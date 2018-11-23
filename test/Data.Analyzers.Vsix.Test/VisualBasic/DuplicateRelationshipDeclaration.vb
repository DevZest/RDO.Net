Imports System.Data.SqlClient
Imports DevZest.Data.Annotations

Public Class DuplicateRelationshipDeclaration
    Inherits RelationshipDiagnosticsBase

    Protected Sub New(sqlConnection As SqlConnection)
        MyBase.New(sqlConnection)
    End Sub

    Private m_Addresses As DbTable(Of Address)
    Public ReadOnly Property Addresses As DbTable(Of Address)
        Get
            Return GetTable(m_Addresses)
        End Get
    End Property

    Private m_Customers As DbTable(Of Customer)
    <Relationship(NameOf(FK_Customer_Address))>
    <Relationship(NameOf(FK_Customer_Address))>
    Public ReadOnly Property Customers As DbTable(Of Customer)
        Get
            Return GetTable(m_Customers)
        End Get
    End Property

    <_Relationship>
    Private Function FK_Customer_Address(x As Customer) As KeyMapping
        Return x.FK_Address.Join(ModelOf(Addresses))
    End Function
End Class

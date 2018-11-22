Imports System.Data.SqlClient
Imports DevZest.Data.SqlServer

Public MustInherit Class ForeignKeyDiagnosticsBase
    Inherits SqlSession

    Protected Sub New(sqlConnection As SqlConnection)
        MyBase.New(sqlConnection)
    End Sub

    Public NotInheritable Class Address
        Inherits Model(Of PK)

        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Sub New(addressId As _Int32)
                MyBase.New(addressId)
            End Sub
        End Class

        Protected Overrides Function CreatePrimaryKey() As PK
            Return New PK(AddressId)
        End Function

        Public Shared ReadOnly _AddressId As Mounter(Of _Int32) = RegisterColumn(Function(x As Address) x.AddressId)

        Private m_AddressId As _Int32
        Public Property AddressId As _Int32
            Get
                Return m_AddressId
            End Get
            Private Set
                m_AddressId = Value
            End Set
        End Property
    End Class

    Public NotInheritable Class Customer
        Inherits Model(Of PK)

        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Sub New(customerId As _Int32)
                MyBase.New(customerId)
            End Sub
        End Class

        Protected Overrides Function CreatePrimaryKey() As PK
            Return New PK(CustomerId)
        End Function

        Public Shared ReadOnly _CustomerId As Mounter(Of _Int32) = RegisterColumn(Function(x As Customer) x.CustomerId)
        Public Shared ReadOnly _AddressId As Mounter(Of _Int32) = RegisterColumn(Function(x As Customer) x.AddressId)

        Private m_CustomerId As _Int32
        Public Property CustomerId As _Int32
            Get
                Return m_CustomerId
            End Get
            Private Set
                m_CustomerId = Value
            End Set
        End Property

        Private m_AddressId As _Int32
        Public Property AddressId As _Int32
            Get
                Return m_AddressId
            End Get
            Private Set
                m_AddressId = Value
            End Set
        End Property

        Private m_FK_Address As Address.PK
        Public ReadOnly Property FK_Address As Address.PK
            Get
                If m_FK_Address Is Nothing Then
                    m_FK_Address = New Address.PK(AddressId)
                End If
                Return m_FK_Address
            End Get
        End Property
    End Class
End Class

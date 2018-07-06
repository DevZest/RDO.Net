Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace DevZest.Samples.AdventureWorksLT
    Public Class CustomerAddress
        Inherits BaseModel(Of CustomerAddress.PK)

        <DbPrimaryKey("PK_CustomerAddress_CustomerID_AddressID", Description:="Primary key (clustered) constraint")>
        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Shared Function [Const](customerId As Integer, addressId As Integer) As IDataValues
                Return DataValues.Create(_Int32.Const(customerId), _Int32.Const(addressId))
            End Function

            Public Sub New(customerID As _Int32, addressID As _Int32)
                MyBase.New(customerID, addressID)
            End Sub

            Public ReadOnly Property CustomerID As _Int32
                Get
                    Return GetColumn(Of _Int32)(0)
                End Get
            End Property
        End Class

        Public NotInheritable Class Key
            Inherits Key(Of PK)

            Shared Sub New()
                RegisterColumn(Function(ByVal x As Key) x.CustomerID, _CustomerID)
                RegisterColumn(Function(ByVal x As Key) x.AddressID, _AddressID)
            End Sub

            Protected Overrides Function CreatePrimaryKey() As PK
                Return New PK(CustomerID, AddressID)
            End Function

            Private m_CustomerID As _Int32
            Public Property CustomerID As _Int32
                Get
                    Return m_CustomerID
                End Get
                Private Set
                    m_CustomerID = Value
                End Set
            End Property

            Private m_AddressID As _Int32
            Public Property AddressID As _Int32
                Get
                    Return m_AddressID
                End Get
                Private Set
                    m_AddressID = Value
                End Set
            End Property
        End Class

        Public Class Ref
            Inherits Ref(Of PK)

            Shared Sub New()
                Register(Function(ByVal x As Ref) x.CustomerID, _CustomerID)
                Register(Function(ByVal x As Ref) x.AddressID, _AddressID)
            End Sub

            Private m_CustomerID As _Int32
            Public Property CustomerID As _Int32
                Get
                    Return m_CustomerID
                End Get
                Private Set
                    m_CustomerID = Value
                End Set
            End Property

            Private m_AddressID As _Int32
            Public Property AddressID As _Int32
                Get
                    Return m_AddressID
                End Get
                Set
                    m_AddressID = Value
                End Set
            End Property

            Protected Overrides Function GetForeignKey() As PK
                Return New PK(CustomerID, AddressID)
            End Function
        End Class

        Public Class Lookup
            Inherits ColumnGroup

            Shared Sub New()
                Register(Function(ByVal x As Lookup) x.AddressType, _AddressType)
            End Sub

            Private m_AddressType As _String
            Public Property AddressType As _String
                Get
                    Return m_AddressType
                End Get
                Private Set
                    m_AddressType = Value
                End Set
            End Property
        End Class

        Public Shared ReadOnly _CustomerID As Mounter(Of _Int32)
        Public Shared ReadOnly _AddressID As Mounter(Of _Int32)
        Public Shared ReadOnly _AddressType As Mounter(Of _String)

        Shared Sub New()
            _CustomerID = RegisterColumn(Function(x As CustomerAddress) x.CustomerID)
            _AddressID = RegisterColumn(Function(x As CustomerAddress) x.AddressID)
            _AddressType = RegisterColumn(Function(x As CustomerAddress) x.AddressType)
        End Sub

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(CustomerID, AddressID)
        End Function

        Private m_FK_Customer As Customer.PK
        Public ReadOnly Property FK_Customer As Customer.PK
            Get
                If m_FK_Customer Is Nothing Then
                    m_FK_Customer = New Customer.PK(CustomerID)
                End If
                Return m_FK_Customer
            End Get
        End Property

        Private m_FK_Address As Address.PK
        Public ReadOnly Property FK_Address As Address.PK
            Get
                If m_FK_Address Is Nothing Then
                    m_FK_Address = New Address.PK(AddressID)
                End If
                Return m_FK_Address
            End Get
        End Property

        Private m_CustomerID As _Int32
        <DbColumn(Description:="Primary key. Foreign key to Customer.CustomerID.")>
        Public Property CustomerID As _Int32
            Get
                Return m_CustomerID
            End Get
            Set
                m_CustomerID = Value
            End Set
        End Property

        Private m_AddressID As _Int32
        <DbColumn(Description:="Primary key. Foreign key to Address.AddressID.")>
        Public Property AddressID As _Int32
            Get
                Return m_AddressID
            End Get
            Set
                m_AddressID = Value
            End Set
        End Property

        Private m_AddressType As _String
        <UdtName>
        <DbColumn(Description:="The kind of Address. One of: Archive, Billing, Home, Main Office, Primary, Shipping")>
        Public Property AddressType As _String
            Get
                Return m_AddressType
            End Get
            Private Set
                m_AddressType = Value
            End Set
        End Property

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class
End Namespace

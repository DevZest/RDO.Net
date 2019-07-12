<DbIndex("IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion", Description:="Nonclustered index.")>
<DbIndex("IX_Address_StateProvince", Description:="Nonclustered index.")>
Public Class Address
    Inherits BaseModel(Of Address.PK)

    <DbPrimaryKey("PK_Address_AddressID", Description:="Primary key (clustered) constraint")>
    Public NotInheritable Class PK
        Inherits CandidateKey

        Public Sub New(addressID As _Int32)
            MyBase.New(addressID)
        End Sub
    End Class

    Public Class Key
        Inherits Key(Of PK)

        Shared Sub New()
            Register(Function(x As Key) x.AddressID, _AddressID)
        End Sub

        Protected Overrides Function CreatePrimaryKey() As PK
            Return New PK(AddressID)
        End Function

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
            Register(Function(x As Ref) x.AddressID, _AddressID)
        End Sub

        Private m_AddressID As _Int32
        Public Property AddressID As _Int32
            Get
                Return m_AddressID
            End Get
            Private Set
                m_AddressID = Value
            End Set
        End Property

        Protected Overrides Function CreateForeignKey() As PK
            Return New PK(AddressID)
        End Function
    End Class

    Public Class Lookup
        Inherits Projection

        Shared Sub New()
            Register(Function(x As Lookup) x.AddressLine1, _AddressLine1)
            Register(Function(x As Lookup) x.AddressLine2, _AddressLine2)
            Register(Function(x As Lookup) x.City, _City)
            Register(Function(x As Lookup) x.StateProvince, _StateProvince)
            Register(Function(x As Lookup) x.CountryRegion, _CountryRegion)
            Register(Function(x As Lookup) x.PostalCode, _PostalCode)
        End Sub

        Private m_AddressLine1 As _String
        Public Property AddressLine1 As _String
            Get
                Return m_AddressLine1
            End Get
            Private Set
                m_AddressLine1 = Value
            End Set
        End Property

        Private m_AddressLine2 As _String
        Public Property AddressLine2 As _String
            Get
                Return m_AddressLine2
            End Get
            Private Set
                m_AddressLine2 = Value
            End Set
        End Property

        Private m_City As _String
        Public Property City As _String
            Get
                Return m_City
            End Get
            Private Set
                m_City = Value
            End Set
        End Property

        Private m_StateProvince As _String
        Public Property StateProvince As _String
            Get
                Return m_StateProvince
            End Get
            Private Set
                m_StateProvince = Value
            End Set
        End Property

        Private m_CountryRegion As _String
        Public Property CountryRegion As _String
            Get
                Return m_CountryRegion
            End Get
            Private Set
                m_CountryRegion = Value
            End Set
        End Property

        Private m_PostalCode As _String
        Public Property PostalCode As _String
            Get
                Return m_PostalCode
            End Get
            Private Set
                m_PostalCode = Value
            End Set
        End Property
    End Class

    Public Shared ReadOnly _AddressID As Mounter(Of _Int32) = RegisterColumn(Function(x As Address) x.AddressID)
    Public Shared ReadOnly _AddressLine1 As Mounter(Of _String) = RegisterColumn(Function(x As Address) x.AddressLine1)
    Public Shared ReadOnly _AddressLine2 As Mounter(Of _String) = RegisterColumn(Function(x As Address) x.AddressLine2)
    Public Shared ReadOnly _City As Mounter(Of _String) = RegisterColumn(Function(x As Address) x.City)
    Public Shared ReadOnly _StateProvince As Mounter(Of _String) = RegisterColumn(Function(x As Address) x.StateProvince)
    Public Shared ReadOnly _CountryRegion As Mounter(Of _String) = RegisterColumn(Function(x As Address) x.CountryRegion)
    Public Shared ReadOnly _PostalCode As Mounter(Of _String) = RegisterColumn(Function(x As Address) x.PostalCode)

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(AddressID)
    End Function

    Private m_AddressID As _Int32
    <Identity>
    <DbColumn(Description:="Primary key for Address records.")>
    Public Property AddressID As _Int32
        Get
            Return m_AddressID
        End Get
        Private Set
            m_AddressID = Value
        End Set
    End Property

    Private m_AddressLine1 As _String

    <Required>
    <SqlNVarChar(60)>
    <DbColumn(Description:="First street address line.")>
    Public Property AddressLine1 As _String
        Get
            Return m_AddressLine1
        End Get
        Private Set
            m_AddressLine1 = Value
        End Set
    End Property

    Private m_AddressLine2 As _String
    <SqlNVarChar(60)>
    <DbColumn(Description:="Second street address line.")>
    Public Property AddressLine2 As _String
        Get
            Return m_AddressLine2
        End Get
        Private Set
            m_AddressLine2 = Value
        End Set
    End Property

    Private m_City As _String
    <Required>
    <SqlNVarChar(30)>
    <DbColumn(Description:="Name of the city.")>
    Public Property City As _String
        Get
            Return m_City
        End Get
        Private Set
            m_City = Value
        End Set
    End Property

    Private m_StateProvince As _String
    <UdtName>
    <DbColumn(Description:="Name of state or province.")>
    Public Property StateProvince As _String
        Get
            Return m_StateProvince
        End Get
        Private Set
            m_StateProvince = Value
        End Set
    End Property

    Private m_CountryRegion As _String

    <UdtName>
    Public Property CountryRegion As _String
        Get
            Return m_CountryRegion
        End Get
        Private Set
            m_CountryRegion = Value
        End Set
    End Property

    Private m_PostalCode As _String
    <Required>
    <SqlNVarChar(15)>
    <DbColumn(Description:="Postal code for the street address.")>
    Public Property PostalCode As _String
        Get
            Return m_PostalCode
        End Get
        Private Set
            m_PostalCode = Value
        End Set
    End Property

    <_DbIndex>
    Private ReadOnly Property IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion As ColumnSort()
        Get
            Return New ColumnSort() {AddressLine1, AddressLine2, City, StateProvince, PostalCode, CountryRegion}
        End Get
    End Property

    <_DbIndex>
    Private ReadOnly Property IX_Address_StateProvince As ColumnSort()
        Get
            Return New ColumnSort() {StateProvince}
        End Get
    End Property
End Class


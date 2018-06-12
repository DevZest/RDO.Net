Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer

Namespace DevZest.Samples.AdventureWorksLT
    <DbCompositeIndex("IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion", Description:="Nonclustered index.")>
    Public Class Address
        Inherits BaseModel(Of Address.PK)

        <DbPrimaryKey("PK_Address_AddressID", Description:="Primary key (clustered) constraint")>
        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Sub New(addressID As _Int32)
                Me.AddressID = addressID
            End Sub

            Public ReadOnly Property AddressID As _Int32
        End Class

        Public Shared Function GetKey(addressId As Integer) As IDataValues
            Return DataValues.Create(_Int32.Const(addressId))
        End Function

        Public NotInheritable Class Key
            Inherits Model(Of PK)

            Shared Sub New()
                RegisterColumn(Function(x As Address) x.AddressID, Address._AddressID)
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
            Inherits LeafProjection(Of PK)

            Shared Sub New()
                Register(Function(x As Ref) x.AddressID, Address._AddressID)
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

            Protected Overrides Function CreatePrimaryKey() As PK
                Return New PK(AddressID)
            End Function
        End Class

        Public Class Lookup
            Inherits LeafProjection

            Shared Sub New()
                Register(Function(x As Lookup) x.AddressLine1, Address._AddressLine1)
                Register(Function(x As Lookup) x.AddressLine2, Address._AddressLine2)
                Register(Function(x As Lookup) x.City, Address._City)
                Register(Function(x As Lookup) x.StateProvince, Address._StateProvince)
                Register(Function(x As Lookup) x.CountryRegion, Address._CountryRegion)
                Register(Function(x As Lookup) x.PostalCode, Address._PostalCode)
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

        Public Shared ReadOnly _AddressID As Mounter(Of _Int32)
        Public Shared ReadOnly _AddressLine1 As Mounter(Of _String)
        Public Shared ReadOnly _AddressLine2 As Mounter(Of _String)
        Public Shared ReadOnly _City As Mounter(Of _String)
        Public Shared ReadOnly _StateProvince As Mounter(Of _String)
        Public Shared ReadOnly _CountryRegion As Mounter(Of _String)
        Public Shared ReadOnly _PostalCode As Mounter(Of _String)

        Shared Sub New()
            _AddressID = RegisterColumn(Function(x As Address) x.AddressID)
            _AddressLine1 = RegisterColumn(Function(x As Address) x.AddressLine1)
            _AddressLine2 = RegisterColumn(Function(x As Address) x.AddressLine2)
            _City = RegisterColumn(Function(x As Address) x.City)
            _StateProvince = RegisterColumn(Function(x As Address) x.StateProvince)
            _CountryRegion = RegisterColumn(Function(x As Address) x.CountryRegion)
            _PostalCode = RegisterColumn(Function(x As Address) x.PostalCode)
        End Sub

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(AddressID)
        End Function

        Private m_AddressID As _Int32
        <Identity(1, 1)>
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
        <AsNVarChar(60)>
        <DbColumn(Description:="First street address line.")>
        <DbCompositeIndexMember("IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion", Order:=1)>
        Public Property AddressLine1 As _String
            Get
                Return m_AddressLine1
            End Get
            Private Set
                m_AddressLine1 = Value
            End Set
        End Property

        Private m_AddressLine2 As _String
        <AsNVarChar(60)>
        <DbColumn(Description:="Second street address line.")>
        <DbCompositeIndexMember("IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion", Order:=2)>
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
        <AsNVarChar(30)>
        <DbColumn(Description:="Name of the city.")>
        <DbCompositeIndexMember("IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion", Order:=3)>
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
        <DbCompositeIndexMember("IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion", Order:=4)>
        <DbIndex("IX_Address_StateProvince", Description:="Nonclustered index.")>
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
        <DbCompositeIndexMember("IX_Address_AddressLine1_AddressLine2_City_StateProvince_PostalCode_CountryRegion", Order:=5)>
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
        <AsNVarChar(15)>
        <DbColumn(Description:="Postal code for the street address.")>
        Public Property PostalCode As _String
            Get
                Return m_PostalCode
            End Get
            Private Set
                m_PostalCode = Value
            End Set
        End Property
    End Class
End Namespace

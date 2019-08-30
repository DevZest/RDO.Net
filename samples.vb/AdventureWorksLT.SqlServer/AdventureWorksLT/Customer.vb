Imports DevZest.Data

<Computation("ComputeContactPerson")>
<DbIndex("IX_Customer_EmailAddress", Description:="Nonclustered index.")>
Public Class Customer
    Inherits BaseModel(Of PK)

    <DbPrimaryKey("PK_Customer_CustomerID", Description:="Primary key (clustered) constraint")>
    Public NotInheritable Class PK
        Inherits CandidateKey

        Public Sub New(customerID As _Int32)
            MyBase.New(customerID)
        End Sub
    End Class

    Public Class Key
        Inherits Key(Of PK)

        Shared Sub New()
            Register(Function(x As Key) x.CustomerID, _CustomerID)
        End Sub

        Protected Overrides Function CreatePrimaryKey() As PK
            Return New PK(CustomerID)
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
    End Class

    Public Class Ref
        Inherits Ref(Of PK)

        Shared Sub New()
            Register(Function(x As Ref) x.CustomerID, _CustomerID)
        End Sub

        Private m_CustomerID As _Int32
        Public Property CustomerID As _Int32
            Get
                Return m_CustomerID
            End Get
            Set
                m_CustomerID = Value
            End Set
        End Property

        Protected Overrides Function CreateForeignKey() As PK
            Return New PK(CustomerID)
        End Function
    End Class

    Public Class Lookup
        Inherits Projection

        Shared Sub New()
            Register(Function(x As Lookup) x.Title, _Title)
            Register(Function(x As Lookup) x.FirstName, _FirstName)
            Register(Function(x As Lookup) x.MiddleName, _MiddleName)
            Register(Function(x As Lookup) x.LastName, _LastName)
            Register(Function(x As Lookup) x.CompanyName, _CompanyName)
            Register(Function(x As Lookup) x.EmailAddress, _EmailAddress)
            Register(Function(x As Lookup) x.Phone, _Phone)
        End Sub

        Private m_Tilte As _String
        Public Property Title As _String
            Get
                Return m_Tilte
            End Get
            Private Set
                m_Tilte = Value
            End Set
        End Property

        Private m_FirstName As _String
        Public Property FirstName As _String
            Get
                Return m_FirstName
            End Get
            Private Set
                m_FirstName = Value
            End Set
        End Property

        Private m_MiddleName As _String
        Public Property MiddleName As _String
            Get
                Return m_MiddleName
            End Get
            Private Set
                m_MiddleName = Value
            End Set
        End Property

        Private m_LastName As _String
        Public Property LastName As _String
            Get
                Return m_LastName
            End Get
            Private Set
                m_LastName = Value
            End Set
        End Property

        Private m_CompanyName As _String
        Public Property CompanyName As _String
            Get
                Return m_CompanyName
            End Get
            Private Set
                m_CompanyName = Value
            End Set
        End Property

        Private m_EmailAddress As _String
        Public Property EmailAddress As _String
            Get
                Return m_EmailAddress
            End Get
            Private Set
                m_EmailAddress = Value
            End Set
        End Property

        Private m_Phone As _String
        Public Property Phone As _String
            Get
                Return m_Phone
            End Get
            Private Set
                m_Phone = Value
            End Set
        End Property
    End Class

    Public Shared ReadOnly _CustomerID As Mounter(Of _Int32) = RegisterColumn(Function(x As Customer) x.CustomerID)
    Public Shared ReadOnly _NameStyle As Mounter(Of _Boolean) = RegisterColumn(Function(x As Customer) x.NameStyle)
    Public Shared ReadOnly _Title As Mounter(Of _String) = RegisterColumn(Function(x As Customer) x.Title)
    Public Shared ReadOnly _FirstName As Mounter(Of _String) = RegisterColumn(Function(x As Customer) x.FirstName)
    Public Shared ReadOnly _MiddleName As Mounter(Of _String) = RegisterColumn(Function(x As Customer) x.MiddleName)
    Public Shared ReadOnly _LastName As Mounter(Of _String) = RegisterColumn(Function(x As Customer) x.LastName)
    Public Shared ReadOnly _Suffix As Mounter(Of _String) = RegisterColumn(Function(x As Customer) x.Suffix)
    Public Shared ReadOnly _CompanyName As Mounter(Of _String) = RegisterColumn(Function(x As Customer) x.CompanyName)
    Public Shared ReadOnly _SalesPerson As Mounter(Of _String) = RegisterColumn(Function(x As Customer) x.SalesPerson)
    Public Shared ReadOnly _EmailAddress As Mounter(Of _String) = RegisterColumn(Function(x As Customer) x.EmailAddress)
    Public Shared ReadOnly _Phone As Mounter(Of _String) = RegisterColumn(Function(x As Customer) x.Phone)
    Public Shared ReadOnly _PasswordHash As Mounter(Of _String) = RegisterColumn(Function(x As Customer) x.PasswordHash)
    Public Shared ReadOnly _PasswordSalt As Mounter(Of _String) = RegisterColumn(Function(x As Customer) x.PasswordSalt)

    Shared Sub New()
        RegisterLocalColumn(Function(x As Customer) x.ContactPerson)
    End Sub

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(CustomerID)
    End Function

    Private m_CustomerID As _Int32
    <Identity>
    <DbColumn(Description:="Primary key for Customer records.")>
    Public Property CustomerID As _Int32
        Get
            Return m_CustomerID
        End Get
        Private Set
            m_CustomerID = Value
        End Set
    End Property

    Private m_NameStyle As _Boolean
    <UdtNameStyle>
    <DefaultValue(False, Name:="DF_Customer_NameStyle", Description:="Default constraint value of 0")>
    <DbColumn(Description:="0 = The data in FirstName and LastName are stored in western style (first name, last name) order.  1 = Eastern style (last name, first name) order.")>
    Public Property NameStyle As _Boolean
        Get
            Return m_NameStyle
        End Get
        Private Set
            m_NameStyle = Value
        End Set
    End Property

    Private m_Title As _String
    <SqlNVarChar(8)>
    <DbColumn(Description:="A courtesy title. For example, Mr. or Ms.")>
    Public Property Title As _String
        Get
            Return m_Title
        End Get
        Private Set
            m_Title = Value
        End Set
    End Property

    Private m_FirstName As _String
    <UdtName>
    <DbColumn(Description:="First name of the person.")>
    Public Property FirstName As _String
        Get
            Return m_FirstName
        End Get
        Private Set
            m_FirstName = Value
        End Set
    End Property

    Private m_MiddleName As _String
    <UdtName>
    <DbColumn(Description:="Middle name or middle initial of the person.")>
    Public Property MiddleName As _String
        Get
            Return m_MiddleName
        End Get
        Private Set
            m_MiddleName = Value
        End Set
    End Property

    Private m_LastName As _String
    <UdtName>
    <DbColumn(Description:="Last name of the person.")>
    Public Property LastName As _String
        Get
            Return m_LastName
        End Get
        Private Set
            m_LastName = Value
        End Set
    End Property

    Private m_Suffix As _String
    <SqlNVarChar(10)>
    <DbColumn(Description:="Surname suffix. For example, Sr. or Jr.")>
    Public Property Suffix As _String
        Get
            Return m_Suffix
        End Get
        Private Set
            m_Suffix = Value
        End Set
    End Property

    Private m_CompanyName As _String
    <SqlNVarChar(128)>
    <DbColumn(Description:="The customer's organization.")>
    Public Property CompanyName As _String
        Get
            Return m_CompanyName
        End Get
        Private Set
            m_CompanyName = Value
        End Set
    End Property

    Private m_SalesPerson As _String
    <SqlNVarChar(256)>
    <DbColumn(Description:="The customer's sales person, an employee of AdventureWorks Cycles.")>
    Public Property SalesPerson As _String
        Get
            Return m_SalesPerson
        End Get
        Private Set
            m_SalesPerson = Value
        End Set
    End Property

    Private m_EmailAddress As _String
    <SqlNVarChar(256)>
    <EmailAddress>
    <DbColumn(Description:="E-mail address for the person.")>
    Public Property EmailAddress As _String
        Get
            Return m_EmailAddress
        End Get
        Set
            m_EmailAddress = Value
        End Set
    End Property

    Private m_Phone As _String
    <UdtPhone>
    <DbColumn(Description:="Phone number associated with the person.")>
    Public Property Phone As _String
        Get
            Return m_Phone
        End Get
        Private Set
            m_Phone = Value
        End Set
    End Property

    Private m_PasswordHash As _String
    <Required>
    <SqlVarChar(128)>
    <DbColumn(Description:="Password for the e-mail account.")>
    Public Property PasswordHash As _String
        Get
            Return m_PasswordHash
        End Get
        Private Set
            m_PasswordHash = Value
        End Set
    End Property

    Private m_PasswordSalt As _String
    <Required>
    <SqlVarChar(10)>
    <DbColumn(Description:="Random value concatenated with the password string before the password is hashed.")>
    Public Property PasswordSalt As _String
        Get
            Return m_PasswordSalt
        End Get
        Private Set
            m_PasswordSalt = Value
        End Set
    End Property

    Private m_contactPerson As LocalColumn(Of String)
    Public Property ContactPerson As LocalColumn(Of String)
        Get
            Return m_contactPerson
        End Get
        Private Set
            m_contactPerson = Value
        End Set
    End Property

    <_Computation>
    Private Sub ComputeContactPerson()
        ContactPerson.ComputedAs(LastName, FirstName, Title, AddressOf GetContactPerson, False)
    End Sub

    Private Shared Function GetContactPerson(ByVal dataRow As DataRow, ByVal lastName As _String, ByVal firstName As _String, ByVal title As _String) As String
        Return GetContactPerson(lastName(dataRow), firstName(dataRow), title(dataRow))
    End Function

    Public Shared Function GetContactPerson(ByVal lastName As String, ByVal firstName As String, ByVal title As String) As String
        Dim result As String = If(String.IsNullOrEmpty(lastName), String.Empty, lastName.ToUpper())

        If Not String.IsNullOrEmpty(firstName) Then
            If result.Length > 0 Then result += ", "
            result += firstName
        End If

        If Not String.IsNullOrEmpty(title) Then
            result += " ("
            result += title
            result += ")"
        End If

        Return result
    End Function

    <_DbIndex>
    Private ReadOnly Property IX_Customer_EmailAddress As ColumnSort()
        Get
            Return New ColumnSort() {EmailAddress}
        End Get
    End Property
End Class

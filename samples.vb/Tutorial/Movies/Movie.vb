Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer

Public Class Movie
    Inherits Model(Of Movie.PK)

    Public NotInheritable Class PK
        Inherits CandidateKey

        Public Sub New(id As _Int32)
            MyBase.New(id)
        End Sub
    End Class

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(ID)
    End Function

    Public Class Key
        Inherits Key(Of PK)

        Shared Sub New()
            Register(Function(x As Key) x.ID, _ID)
        End Sub

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ID)
        End Function

        Private m_ID As _Int32

        Public Property ID As _Int32
            Get
                Return m_ID
            End Get
            Private Set
                m_ID = Value
            End Set
        End Property
    End Class

    Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As Movie) x.ID)
    Public Shared ReadOnly _Title As Mounter(Of _String) = RegisterColumn(Function(x As Movie) x.Title)
    Public Shared ReadOnly _ReleaseDate As Mounter(Of _DateTime) = RegisterColumn(Function(x As Movie) x.ReleaseDate)
    Public Shared ReadOnly _Genre As Mounter(Of _String) = RegisterColumn(Function(x As Movie) x.Genre)
    Public Shared ReadOnly _Price As Mounter(Of _Decimal) = RegisterColumn(Function(x As Movie) x.Price)

    Private m_ID As _Int32
    <Identity>
    Public Property ID As _Int32
        Get
            Return m_ID
        End Get
        Private Set
            m_ID = Value
        End Set
    End Property

    Private m_Title As _String
    <StringLength(60, MinimumLength:=3)>
    <Required>
    <SqlNVarChar(60)>
    Public Property Title As _String
        Get
            Return m_Title
        End Get
        Private Set
            m_Title = Value
        End Set
    End Property

    Private m_ReleaseDate As _DateTime
    <Display(Name:="Release Date")>
    <SqlDate>
    <Required>
    Public Property ReleaseDate As _DateTime
        Get
            Return m_ReleaseDate
        End Get
        Private Set
            m_ReleaseDate = Value
        End Set
    End Property

    Private m_Genre As _String
    <RegularExpression("^[A-Z]+[a-zA-Z""'\s-]*$")>
    <Required>
    <StringLength(30)>
    <SqlNVarChar(30)>
    Public Property Genre As _String
        Get
            Return m_Genre
        End Get
        Private Set
            m_Genre = Value
        End Set
    End Property

    Private m_Price As _Decimal
    <SqlMoney>
    <Required>
    Public Property Price As _Decimal
        Get
            Return m_Price
        End Get
        Private Set
            m_Price = Value
        End Set
    End Property
End Class

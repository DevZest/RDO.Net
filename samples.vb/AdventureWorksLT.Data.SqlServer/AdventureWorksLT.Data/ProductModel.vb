Public Class ProductModel
    Inherits BaseModel(Of ProductModel.PK)

    <DbPrimaryKey("PK_ProductModel_ProductModelID", Description:="Clustered index created by a primary key constraint.")>
    Public NotInheritable Class PK
        Inherits CandidateKey

        Public Sub New(ByVal productModelID As _Int32)
            MyBase.New(productModelID)
        End Sub
    End Class

    Public Class Key
        Inherits Key(Of PK)

        Shared Sub New()
            Register(Function(x As Key) x.ProductModelID, _ProductModelID)
        End Sub

        Protected Overrides Function CreatePrimaryKey() As PK
            Return New PK(ProductModelID)
        End Function

        Private m_ProductModelID As _Int32
        Public Property ProductModelID As _Int32
            Get
                Return m_ProductModelID
            End Get
            Private Set
                m_ProductModelID = Value
            End Set
        End Property
    End Class

    Public Class Ref
        Inherits Ref(Of PK)

        Shared Sub New()
            Register(Function(x As Ref) x.ProductModelID, _ProductModelID)
        End Sub

        Private m_ProductModelID As _Int32
        Public Property ProductModelID As _Int32
            Get
                Return m_ProductModelID
            End Get
            Private Set
                m_ProductModelID = Value
            End Set
        End Property

        Protected Overrides Function CreateForeignKey() As PK
            Return New PK(ProductModelID)
        End Function
    End Class

    Public Class Lookup
        Inherits Projection

        Shared Sub New()
            Register(Function(x As Lookup) x.Name, _Name)
        End Sub

        Private m_Name As _String
        Public Property Name As _String
            Get
                Return m_Name
            End Get
            Private Set
                m_Name = Value
            End Set
        End Property
    End Class

    Public Shared ReadOnly _ProductModelID As Mounter(Of _Int32) = RegisterColumn(Function(ByVal __ As ProductModel) __.ProductModelID)
    Public Shared ReadOnly _Name As Mounter(Of _String) = RegisterColumn(Function(ByVal __ As ProductModel) __.Name)
    Public Shared ReadOnly _CatalogDescription As Mounter(Of _SqlXml) = RegisterColumn(Function(ByVal __ As ProductModel) __.CatalogDescription)

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(ProductModelID)
    End Function

    Private m_ProductModelID As _Int32
    <Identity>
    Public Property ProductModelID As _Int32
        Get
            Return m_ProductModelID
        End Get
        Private Set
            m_ProductModelID = Value
        End Set
    End Property

    Private m_Name As _String
    <UdtName>
    Public Property Name As _String
        Get
            Return m_Name
        End Get
        Private Set
            m_Name = Value
        End Set
    End Property

    Private m_CatalogDescription As _SqlXml
    Public Property CatalogDescription As _SqlXml
        Get
            Return m_CatalogDescription
        End Get
        Private Set
            m_CatalogDescription = Value
        End Set
    End Property
End Class

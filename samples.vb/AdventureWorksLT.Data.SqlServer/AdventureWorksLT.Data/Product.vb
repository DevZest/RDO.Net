<CheckConstraint("CK_Product_ListPrice", GetType(My.UserMessages), NameOf(My.UserMessages.CK_Product_ListPrice), Description:="Check constraint [ListPrice] >= (0.00)")>
<CheckConstraint("CK_Product_SellEndDate", GetType(My.UserMessages), NameOf(My.UserMessages.CK_Product_SellEndDate), Description:="Check constraint [SellEndDate] >= [SellStartDate] OR [SellEndDate] IS NULL")>
<CheckConstraint("CK_Product_StandardCost", GetType(My.UserMessages), NameOf(My.UserMessages.CK_Product_StandardCost), Description:="Check constraint [StandardCost] >= (0.00)")>
<CheckConstraint("CK_Product_Weight", GetType(My.UserMessages), NameOf(My.UserMessages.CK_Product_Weight), Description:="Check constraint [Weight] >= (0.00)")>
<UniqueConstraint("AK_Product_Name", Description:="Unique nonclustered constraint.")>
<UniqueConstraint("AK_Product_ProductNumber", Description:="Unique nonclustered constraint.")>
Public Class Product
    Inherits BaseModel(Of Product.PK)

    <DbPrimaryKey("PK_Product_ProductID", Description:="Primary key (clustered) constraint")>
    Public NotInheritable Class PK
        Inherits CandidateKey

        Public Sub New(productID As _Int32)
            MyBase.New(productID)
        End Sub
    End Class

    Public Class Key
        Inherits Key(Of PK)

        Shared Sub New()
            Register(Function(x As Key) x.ProductID, _ProductID)
        End Sub

        Protected Overrides Function CreatePrimaryKey() As PK
            Return New PK(ProductID)
        End Function

        Private m_ProductID As _Int32
        Public Property ProductID As _Int32
            Get
                Return m_ProductID
            End Get
            Private Set
                m_ProductID = Value
            End Set
        End Property
    End Class

    Public Class Ref
        Inherits Ref(Of PK)

        Shared Sub New()
            Register(Function(x As Ref) x.ProductID, _ProductID)
        End Sub

        Private m_ProductID As _Int32
        Public Property ProductID As _Int32
            Get
                Return m_ProductID
            End Get
            Private Set
                m_ProductID = Value
            End Set
        End Property

        Protected Overrides Function CreateForeignKey() As PK
            Return New PK(ProductID)
        End Function
    End Class

    Public Class Lookup
        Inherits Projection

        Shared Sub New()
            Register(Function(x As Lookup) x.Name, _Name)
            Register(Function(x As Lookup) x.ProductNumber, _ProductNumber)
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

        Private m_ProductNumber As _String
        Public Property ProductNumber As _String
            Get
                Return m_ProductNumber
            End Get
            Private Set
                m_ProductNumber = Value
            End Set
        End Property
    End Class

    Public Shared ReadOnly _ProductID As Mounter(Of _Int32) = RegisterColumn(Function(x As Product) x.ProductID)
    Public Shared ReadOnly _Name As Mounter(Of _String) = RegisterColumn(Function(x As Product) x.Name)
    Public Shared ReadOnly _ProductNumber As Mounter(Of _String) = RegisterColumn(Function(x As Product) x.ProductNumber)
    Public Shared ReadOnly _Color As Mounter(Of _String) = RegisterColumn(Function(x As Product) x.Color)
    Public Shared ReadOnly _StandardCost As Mounter(Of _Decimal) = RegisterColumn(Function(x As Product) x.StandardCost)
    Public Shared ReadOnly _ListPrice As Mounter(Of _Decimal) = RegisterColumn(Function(x As Product) x.ListPrice)
    Public Shared ReadOnly _Size As Mounter(Of _String) = RegisterColumn(Function(x As Product) x.Size)
    Public Shared ReadOnly _Weight As Mounter(Of _Decimal) = RegisterColumn(Function(x As Product) x.Weight)
    Public Shared ReadOnly _ProductCategoryID As Mounter(Of _Int32) = RegisterColumn(Function(x As Product) x.ProductCategoryID)
    Public Shared ReadOnly _ProductModelID As Mounter(Of _Int32) = RegisterColumn(Function(ByVal x As Product) x.ProductModelID)
    Public Shared ReadOnly _SellStartDate As Mounter(Of _DateTime) = RegisterColumn(Function(x As Product) x.SellStartDate)
    Public Shared ReadOnly _SellEndDate As Mounter(Of _DateTime) = RegisterColumn(Function(x As Product) x.SellEndDate)
    Public Shared ReadOnly _DiscontinuedDate As Mounter(Of _DateTime) = RegisterColumn(Function(x As Product) x.DiscontinuedDate)
    Public Shared ReadOnly _ThumbNailPhoto As Mounter(Of _Binary) = RegisterColumn(Function(x As Product) x.ThumbNailPhoto)
    Public Shared ReadOnly _ThumbnailPhotoFileName As Mounter(Of _String) = RegisterColumn(Function(x As Product) x.ThumbnailPhotoFileName)

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(ProductID)
    End Function

    Private m_FK_ProductCategory As ProductCategory.PK
    Public ReadOnly Property FK_ProductCategory As ProductCategory.PK
        Get
            If m_FK_ProductCategory Is Nothing Then
                m_FK_ProductCategory = New ProductCategory.PK(ProductCategoryID)
            End If
            Return m_FK_ProductCategory
        End Get
    End Property

    Private m_Fk_ProductModel As ProductModel.PK
    Public ReadOnly Property FK_ProductModel As ProductModel.PK
        Get
            If m_Fk_ProductModel Is Nothing Then
                m_Fk_ProductModel = New ProductModel.PK(ProductModelID)
            End If
            Return m_Fk_ProductModel
        End Get
    End Property

    Private m_ProductID As _Int32
    <Identity>
    <DbColumn(Description:="Primary key for Product records.")>
    Public Property ProductID As _Int32
        Get
            Return m_ProductID
        End Get
        Private Set
            m_ProductID = Value
        End Set
    End Property

    Private m_Name As _String
    <UdtName>
    <DbColumn(Description:="Name of the product.")>
    Public Property Name As _String
        Get
            Return m_Name
        End Get
        Private Set
            m_Name = Value
        End Set
    End Property

    Private m_ProductNumber As _String
    <Required>
    <SqlNVarChar(25)>
    <DbColumn(Description:="Unique product identification number.")>
    Public Property ProductNumber As _String
        Get
            Return m_ProductNumber
        End Get
        Private Set
            m_ProductNumber = Value
        End Set
    End Property

    Private m_Color As _String
    <SqlNVarChar(15)>
    <DbColumn(Description:="Product color.")>
    Public Property Color As _String
        Get
            Return m_Color
        End Get
        Private Set
            m_Color = Value
        End Set
    End Property

    Private m_StandardCost As _Decimal
    <Required>
    <SqlMoney()>
    <DbColumn(Description:="Standard cost of the product.")>
    Public Property StandardCost As _Decimal
        Get
            Return m_StandardCost
        End Get
        Private Set
            m_StandardCost = Value
        End Set
    End Property

    Private m_ListPrice As _Decimal
    <Required>
    <SqlMoney()>
    <DbColumn(Description:="Selling price.")>
    Public Property ListPrice As _Decimal
        Get
            Return m_ListPrice
        End Get
        Private Set
            m_ListPrice = Value
        End Set
    End Property

    Private m_Size As _String
    <SqlNVarChar(5)>
    <DbColumn(Description:="Product size.")>
    Public Property Size As _String
        Get
            Return m_Size
        End Get
        Private Set
            m_Size = Value
        End Set
    End Property

    Private m_Weight As _Decimal
    <SqlDecimal(8, 2)>
    <DbColumn(Description:="Product weight.")>
    Public Property Weight As _Decimal
        Get
            Return m_Weight
        End Get
        Private Set
            m_Weight = Value
        End Set
    End Property

    Private m_ProductCategoryID As _Int32
    <DbColumn(Description:="Product is a member of this product category. Foreign key to ProductCategory.ProductCategoryID.")>
    Public Property ProductCategoryID As _Int32
        Get
            Return m_ProductCategoryID
        End Get
        Private Set
            m_ProductCategoryID = Value
        End Set
    End Property

    Private m_ProductModelID As _Int32
    <DbColumn(Description:="Product is a member of this product model. Foreign key to ProductModel.ProductModelID.")>
    Public Property ProductModelID As _Int32
        Get
            Return m_ProductModelID
        End Get
        Private Set
            m_ProductModelID = Value
        End Set
    End Property

    Private m_SellStartDate As _DateTime
    <Required>
    <SqlDateTime>
    <DbColumn(Description:="Date the product was available for sale.")>
    Public Property SellStartDate As _DateTime
        Get
            Return m_SellStartDate
        End Get
        Private Set
            m_SellStartDate = Value
        End Set
    End Property

    Private m_SellEndDate As _DateTime
    <SqlDateTime>
    <DbColumn(Description:="Date the product was no longer available for sale.")>
    Public Property SellEndDate As _DateTime
        Get
            Return m_SellStartDate
        End Get
        Private Set
            m_SellStartDate = Value
        End Set
    End Property

    Private m_DiscontinuedDate As _DateTime
    <SqlDateTime>
    <DbColumn(Description:="Date the product was discontinued.")>
    Public Property DiscontinuedDate As _DateTime
        Get
            Return m_DiscontinuedDate
        End Get
        Private Set
            m_DiscontinuedDate = Value
        End Set
    End Property

    Private m_ThumbNailPhoto As _Binary
    <SqlVarBinaryMax>
    <DbColumn(Description:="Small image of the product.")>
    Public Property ThumbNailPhoto As _Binary
        Get
            Return m_ThumbNailPhoto
        End Get
        Private Set
            m_ThumbNailPhoto = Value
        End Set
    End Property

    Private m_ThumbnailPhotoFileName As _String
    <SqlNVarChar(50)>
    <DbColumn(Description:="Small image file name.")>
    Public Property ThumbnailPhotoFileName As _String
        Get
            Return m_ThumbnailPhotoFileName
        End Get
        Private Set
            m_ThumbnailPhotoFileName = Value
        End Set
    End Property

    <_CheckConstraint>
    Private ReadOnly Property CK_Product_ListPrice As _Boolean
        Get
            Return ListPrice >= _Decimal.Const(0)
        End Get
    End Property

    <_CheckConstraint>
    Private ReadOnly Property CK_Product_SellEndDate As _Boolean
        Get
            Return SellEndDate >= SellStartDate Or SellEndDate.IsNull()
        End Get
    End Property

    <_CheckConstraint>
    ReadOnly Property CK_Product_StandardCost As _Boolean
        Get
            Return StandardCost >= _Decimal.[Const](0)
        End Get
    End Property

    <_CheckConstraint>
    Private ReadOnly Property CK_Product_Weight As _Boolean
        Get
            Return Weight >= _Decimal.[Const](0)
        End Get
    End Property

    <_UniqueConstraint>
    Private ReadOnly Property AK_Product_Name As ColumnSort()
        Get
            Return New ColumnSort() {Name}
        End Get
    End Property

    <_UniqueConstraint>
    Private ReadOnly Property AK_Product_ProductNumber As ColumnSort()
        Get
            Return New ColumnSort() {ProductNumber}
        End Get
    End Property
End Class

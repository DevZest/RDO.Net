Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer

Namespace DevZest.Samples.AdventureWorksLT
    Public Class Product
        Inherits BaseModel(Of Product.PK)

        <DbPrimaryKey("PK_Product_ProductID", Description:="Primary key (clustered) constraint")>
        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Sub New(productID As _Int32)
                Me.ProductID = productID
            End Sub

            Public Property ProductID As _Int32
        End Class

        Public Shared Function GetKey(productId As Integer) As IDataValues
            Return DataValues.Create(_Int32.Const(productId))
        End Function

        Public NotInheritable Class Key
            Inherits Model(Of PK)

            Shared Sub New()
                RegisterColumn(Function(x As Key) x.ProductID, _ProductID)
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

            Protected Overrides Function GetForeignKey() As PK
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

        Public Shared ReadOnly _ProductID As Mounter(Of _Int32)
        Public Shared ReadOnly _Name As Mounter(Of _String)
        Public Shared ReadOnly _ProductNumber As Mounter(Of _String)
        Public Shared ReadOnly _Color As Mounter(Of _String)
        Public Shared ReadOnly _StandardCost As Mounter(Of _Decimal)
        Public Shared ReadOnly _ListPrice As Mounter(Of _Decimal)
        Public Shared ReadOnly _Size As Mounter(Of _String)
        Public Shared ReadOnly _Weight As Mounter(Of _Decimal)
        Public Shared ReadOnly _SellStartDate As Mounter(Of _DateTime)
        Public Shared ReadOnly _SellEndDate As Mounter(Of _DateTime)
        Public Shared ReadOnly _DiscontinuedDate As Mounter(Of _DateTime)
        Public Shared ReadOnly _ThumbNailPhoto As Mounter(Of _Binary)
        Public Shared ReadOnly _ThumbnailPhotoFileName As Mounter(Of _String)

        Shared Sub New()
            _ProductID = RegisterColumn(Function(x As Product) x.ProductID)
            _Name = RegisterColumn(Function(x As Product) x.Name)
            _ProductNumber = RegisterColumn(Function(x As Product) x.ProductNumber)
            _Color = RegisterColumn(Function(x As Product) x.Color)
            _StandardCost = RegisterColumn(Function(x As Product) x.StandardCost)
            _ListPrice = RegisterColumn(Function(x As Product) x.ListPrice)
            _Size = RegisterColumn(Function(x As Product) x.Size)
            _Weight = RegisterColumn(Function(x As Product) x.Weight)
            RegisterColumn(Function(x As Product) x.ProductCategoryID, ProductCategory._ProductCategoryID)
            RegisterColumn(Function(ByVal x As Product) x.ProductModelID, ProductModel._ProductModelID)
            _SellStartDate = RegisterColumn(Function(x As Product) x.SellStartDate)
            _SellEndDate = RegisterColumn(Function(x As Product) x.SellEndDate)
            _DiscontinuedDate = RegisterColumn(Function(x As Product) x.DiscontinuedDate)
            _ThumbNailPhoto = RegisterColumn(Function(x As Product) x.ThumbNailPhoto)
            _ThumbnailPhotoFileName = RegisterColumn(Function(x As Product) x.ThumbnailPhotoFileName)
        End Sub

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ProductID)
        End Function

        Private m_FK_ProductCategory As ProductCategory.PK
        Public ReadOnly Property FK_ProductCategory As ProductCategory.PK
            Get
                If m_FK_ProductCategory Is Nothing Then m_FK_ProductCategory = New ProductCategory.PK(ProductCategoryID)
                Return m_FK_ProductCategory
            End Get
        End Property

        Private m_Fk_ProductModel As ProductModel.PK
        Public ReadOnly Property FK_ProductModel As ProductModel.PK
            Get
                If m_Fk_ProductModel Is Nothing Then m_Fk_ProductModel = New ProductModel.PK(ProductModelID)
                Return m_Fk_ProductModel
            End Get
        End Property

        Private m_ProductID As _Int32
        <Identity(1, 1)>
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
        <Unique(Name:="AK_Product_Name", Description:="Unique nonclustered constraint.")>
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
        <AsNVarChar(25)>
        <DbColumn(Description:="Unique product identification number.")>
        <Unique(Name:="AK_Product_ProductNumber", Description:="Unique nonclustered constraint.")>
        Public Property ProductNumber As _String
            Get
                Return m_ProductNumber
            End Get
            Private Set
                m_ProductNumber = Value
            End Set
        End Property

        Private m_Color As _String
        <AsNVarChar(15)>
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
        <AsMoney()>
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
        <AsMoney()>
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
        <AsNVarChar(5)>
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
        <AsDecimal(8, 2)>
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
        <AsDateTime>
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
        <AsDateTime>
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
        <AsDateTime>
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
        <AsVarBinaryMax>
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
        <AsNVarChar(50)>
        <DbColumn(Description:="Small image file name.")>
        Public Property ThumbnailPhotoFileName As _String
            Get
                Return m_ThumbnailPhotoFileName
            End Get
            Private Set
                m_ThumbnailPhotoFileName = Value
            End Set
        End Property

        Private m_CK_Product_ListPrice As _Boolean
        <Check(GetType(UserMessages), NameOf(UserMessages.CK_Product_ListPrice), Name:=NameOf(CK_Product_ListPrice), Description:="Check constraint [ListPrice] >= (0.00)")>
        Private ReadOnly Property CK_Product_ListPrice As _Boolean
            Get
                If m_CK_Product_ListPrice Is Nothing Then m_CK_Product_ListPrice = ListPrice >= _Decimal.Const(0)
                Return m_CK_Product_ListPrice
            End Get
        End Property

        Private m_Ck_Product_SellEndDate As _Boolean
        <Check(GetType(UserMessages), NameOf(UserMessages.CK_Product_SellEndDate), Name:=NameOf(CK_Product_SellEndDate), Description:="Check constraint [SellEndDate] >= [SellStartDate] OR [SellEndDate] IS NULL")>
        Private ReadOnly Property CK_Product_SellEndDate As _Boolean
            Get
                If m_Ck_Product_SellEndDate Is Nothing Then m_Ck_Product_SellEndDate = SellEndDate >= SellStartDate Or SellEndDate.IsNull()
                Return m_Ck_Product_SellEndDate
            End Get
        End Property

        Private m_CK_Product_StandardCost As _Boolean
        <Check(GetType(UserMessages), NameOf(UserMessages.CK_Product_StandardCost), Name:=NameOf(CK_Product_StandardCost), Description:="Check constraint [StandardCost] >= (0.00)")>
        Private ReadOnly Property CK_Product_StandardCost As _Boolean
            Get
                If m_CK_Product_StandardCost Is Nothing Then m_CK_Product_StandardCost = StandardCost >= _Decimal.[Const](0)
                Return m_CK_Product_StandardCost
            End Get
        End Property

        Private m_CK_Product_Weight As _Boolean
        <Check(GetType(UserMessages), NameOf(UserMessages.CK_Product_Weight), Name:=NameOf(CK_Product_Weight), Description:="Check constraint [Weight] >= (0.00)")>
        Private ReadOnly Property CK_Product_Weight As _Boolean
            Get
                If m_CK_Product_Weight Is Nothing Then m_CK_Product_Weight = Weight >= _Decimal.[Const](0)
                Return m_CK_Product_Weight
            End Get
        End Property
    End Class
End Namespace

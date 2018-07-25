Imports DevZest.Data
Imports DevZest.Data.SqlServer
Imports DevZest.Data.Annotations

Namespace DevZest.Samples.AdventureWorksLT
    Public Class ProductCategory
        Inherits BaseModel(Of ProductCategory.PK)

        <DbPrimaryKey("PK_ProductCategory_ProductCategoryID", Description:="Primary key (clustered) constraint")>
        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Shared Function ValueOf(productCategoryID As Integer) As IDataValues
                Return DataValues.Create(_Int32.Const(productCategoryID))
            End Function

            Public Sub New(ByVal productCategoryID As _Int32)
                MyBase.New(productCategoryID)
            End Sub

            Public ReadOnly Property ProductCategoryID As _Int32
                Get
                    Return GetColumn(Of _Int32)(0)
                End Get
            End Property
        End Class

        Public Class Key
            Inherits Key(Of PK)

            Shared Sub New()
                Register(Function(x As Key) x.ProductCategoryID, _ProductCategoryID)
            End Sub

            Protected Overrides Function GetPrimaryKey() As PK
                Return New PK(ProductCategoryID)
            End Function

            Private m_ProductCategoryID As _Int32
            Public Property ProductCategoryID As _Int32
                Get
                    Return m_ProductCategoryID
                End Get
                Private Set
                    m_ProductCategoryID = Value
                End Set
            End Property
        End Class

        Public Class Ref
            Inherits Ref(Of PK)

            Shared Sub New()
                Register(Function(x As Ref) x.ProductCategoryID, _ProductCategoryID)
            End Sub

            Private m_ProductCategoryID As _Int32
            Public Property ProductCategoryID As _Int32
                Get
                    Return m_ProductCategoryID
                End Get
                Private Set
                    m_ProductCategoryID = Value
                End Set
            End Property

            Protected Overrides Function GetForeignKey() As PK
                Return New PK(ProductCategoryID)
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

        Public Shared ReadOnly _ProductCategoryID As Mounter(Of _Int32) = RegisterColumn(Function(x As ProductCategory) x.ProductCategoryID)
        Public Shared ReadOnly _ParentProductCategoryID As Mounter(Of _Int32) = RegisterColumn(Function(x As ProductCategory) x.ParentProductCategoryID)
        Public Shared ReadOnly _Name As Mounter(Of _String) = RegisterColumn(Function(x As ProductCategory) x.Name)

        Shared Sub New()
            RegisterChildModel(Function(x As ProductCategory) x.SubCategories, Function(x As ProductCategory) x.FK_ParentProductCategory)
        End Sub

        Private m_SubCategories As ProductCategory
        Public Property SubCategories As ProductCategory
            Get
                Return m_SubCategories
            End Get
            Private Set
                m_SubCategories = Value
            End Set
        End Property

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ProductCategoryID)
        End Function

        Private m_ProductCategoryID As _Int32
        <Identity(1, 1)>
        <DbColumn(Description:="Primary key for ProductCategory records.")>
        Public Property ProductCategoryID As _Int32
            Get
                Return m_ProductCategoryID
            End Get
            Private Set
                m_ProductCategoryID = Value
            End Set
        End Property

        Private m_ParentProductCategoryID As _Int32
        <DbColumn(Description:="Product category identification number of immediate ancestor category. Foreign key to ProductCategory.ProductCategoryID.")>
        Public Property ParentProductCategoryID As _Int32
            Get
                Return m_ParentProductCategoryID
            End Get
            Private Set
                m_ParentProductCategoryID = Value
            End Set
        End Property

        Private m_FK_productCategory As PK
        <DbColumn(Description:="Category description.")>
        Public ReadOnly Property FK_ParentProductCategory As PK
            Get
                If m_FK_productCategory Is Nothing Then m_FK_productCategory = New PK(ParentProductCategoryID)
                Return m_FK_productCategory
            End Get
        End Property

        Private m_Name As _String
        <UdtName>
        <Required>
        <AsNVarChar(50)>
        <Unique(Name:="AK_ProductCategory_Name", Description:="Unique nonclustered constraint.")>
        Public Property Name As _String
            Get
                Return m_Name
            End Get
            Private Set
                m_Name = Value
            End Set
        End Property
    End Class
End Namespace

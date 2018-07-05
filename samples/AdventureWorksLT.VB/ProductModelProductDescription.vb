Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer

Namespace DevZest.Samples.AdventureWorksLT
    Public Class ProductModelProductDescription
        Inherits BaseModel(Of PK)

        <DbPrimaryKey("PK_ProductModelProductDescription_ProductModelID_ProductDescriptionID_Culture", Description:="Primary key (clustered) constraint")>
        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Shared Function [Const](ByVal productModelID As Integer, ByVal productDescriptionID As Integer, ByVal culture As String) As IDataValues
                Return DataValues.Create(_Int32.[Const](productModelID), _Int32.[Const](productDescriptionID), _String.[Const](culture))
            End Function

            Public Sub New(productModelID As _Int32, productDescriptionID As _Int32, culture As _String)
                MyBase.New(productModelID, productDescriptionID, culture)
            End Sub

            Public ReadOnly Property ProductModelID As _Int32
                Get
                    Return GetColumn(Of _Int32)(0)
                End Get
            End Property

            Public ReadOnly Property ProductDescriptionID As _Int32
                Get
                    Return GetColumn(Of _Int32)(1)
                End Get
            End Property

            Public ReadOnly Property Culture As _String
                Get
                    Return GetColumn(Of _String)(2)
                End Get
            End Property
        End Class

        Public Shared ReadOnly _ProductModelID As Mounter(Of _Int32)
        Public Shared ReadOnly _ProductDescriptionID As Mounter(Of _Int32)
        Public Shared ReadOnly _Culture As Mounter(Of _String)

        Shared Sub New()
            _ProductModelID = RegisterColumn(Function(x As ProductModelProductDescription) x.ProductModelID)
            _ProductDescriptionID = RegisterColumn(Function(x As ProductModelProductDescription) x.ProductDescriptionID)
            _Culture = RegisterColumn(Function(x As ProductModelProductDescription) x.Culture)
        End Sub

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ProductModelID, ProductDescriptionID, Culture)
        End Function

        Private m_FK_ProductModel As ProductModel.PK
        Public ReadOnly Property FK_ProductModel As ProductModel.PK
            Get
                If m_FK_ProductModel Is Nothing Then m_FK_ProductModel = New ProductModel.PK(ProductModelID)
                Return m_FK_ProductModel
            End Get
        End Property

        Private m_FK_ProductDescription As ProductDescription.PK
        Public ReadOnly Property FK_ProductDescription As ProductDescription.PK
            Get
                If m_FK_ProductDescription Is Nothing Then m_FK_ProductDescription = New ProductDescription.PK(ProductDescriptionID)
                Return m_FK_ProductDescription
            End Get
        End Property

        Private m_ProductModelID As _Int32
        <DbColumn(Description:="Primary key. Foreign key to ProductModel.ProductModelID.")>
        Public Property ProductModelID As _Int32
            Get
                Return m_ProductModelID
            End Get
            Private Set
                m_ProductModelID = Value
            End Set
        End Property

        Private m_ProductDescriptionID As _Int32
        <DbColumn(Description:="Primary key. Foreign key to ProductDescription.ProductDescriptionID.")>
        Public Property ProductDescriptionID As _Int32
            Get
                Return m_ProductDescriptionID
            End Get
            Private Set
                m_ProductDescriptionID = Value
            End Set
        End Property

        Private m_Culture As _String
        <AsNChar(6)>
        <DbColumn(Description:="The culture for which the description is written.")>
        Public Property Culture As _String
            Get
                Return m_Culture
            End Get
            Private Set
                m_Culture = Value
            End Set
        End Property
    End Class
End Namespace

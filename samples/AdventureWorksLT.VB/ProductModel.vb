Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer

Namespace DevZest.Samples.AdventureWorksLT
    Public Class ProductModel
        Inherits BaseModel(Of ProductModel.PK)

        <DbPrimaryKey("PK_ProductModel_ProductModelID", Description:="Clustered index created by a primary key constraint.")>
        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Sub New(ByVal productModelID As _Int32)
                Me.ProductModelID = productModelID
            End Sub

            Public ReadOnly Property ProductModelID As _Int32
        End Class

        Public Shared Function GetKey(productModelId As Integer) As IDataValues
            Return DataValues.Create(_Int32.Const(productModelId))
        End Function

        Public NotInheritable Class Key
            Inherits Model(Of PK)

            Shared Sub New()
                RegisterColumn(Function(x As Key) x.ProductModelID, _ProductModelID)
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

            Protected Overrides Function CreatePrimaryKey() As PK
                Return New PK(ProductModelID)
            End Function
        End Class

        Public Class Lookup
            Inherits LookupBase

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

        Public Shared ReadOnly _ProductModelID As Mounter(Of _Int32)
        Public Shared ReadOnly _Name As Mounter(Of _String)
        Public Shared ReadOnly _CatalogDescription As Mounter(Of _SqlXml)

        Shared Sub New()
            _ProductModelID = RegisterColumn(Function(ByVal __ As ProductModel) __.ProductModelID)
            _Name = RegisterColumn(Function(ByVal __ As ProductModel) __.Name)
            _CatalogDescription = RegisterColumn(Function(ByVal __ As ProductModel) __.CatalogDescription)
        End Sub

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ProductModelID)
        End Function

        Private m_ProductModelID As _Int32
        <Identity(1, 1)>
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
End Namespace

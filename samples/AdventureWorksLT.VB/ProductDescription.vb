Imports System
Imports DevZest.Data
Imports DevZest.Data.SqlServer
Imports DevZest.Data.Annotations

Namespace DevZest.Samples.AdventureWorksLT
    Public Class ProductDescription
        Inherits BaseModel(Of ProductDescription.PK)

        <DbPrimaryKey("PK_ProductDescription_ProductDescriptionID", Description:="Primary key (clustered) constraint")>
        Public NotInheritable Class PK
            Inherits PrimaryKey

            Public Shared Function [Const](productDescriptionID As Integer) As IDataValues
                Return DataValues.Create(_Int32.Const(productDescriptionID))
            End Function

            Public Sub New(productDescriptionID As _Int32)
                MyBase.New(productDescriptionID)
            End Sub

            Public ReadOnly Property ProductDescriptionID As _Int32
                Get
                    Return GetColumn(Of _Int32)(0)
                End Get
            End Property
        End Class

        Public Class Key
            Inherits Key(Of PK)

            Shared Sub New()
                RegisterColumn(Function(x As Key) x.ProductDescriptionID, _ProductDescriptionID)
            End Sub

            Protected Overrides Function CreatePrimaryKey() As PK
                Return New PK(ProductDescriptionID)
            End Function

            Private m_ProductDescriptionID As _Int32
            Public Property ProductDescriptionID As _Int32
                Get
                    Return m_ProductDescriptionID
                End Get
                Private Set
                    m_ProductDescriptionID = Value
                End Set
            End Property
        End Class

        Public Class Ref
            Inherits Ref(Of PK)

            Shared Sub New()
                Register(Function(x As Ref) x.ProductDescriptionID, _ProductDescriptionID)
            End Sub

            Private m_ProductDescriptionID As _Int32
            Public Property ProductDescriptionID As _Int32
                Get
                    Return m_ProductDescriptionID
                End Get
                Private Set
                    m_ProductDescriptionID = Value
                End Set
            End Property

            Protected Overrides Function GetForeignKey() As PK
                Return New PK(ProductDescriptionID)
            End Function
        End Class

        Public Class Lookup
            Inherits Projection

            Shared Sub New()
                Register(Function(x As Lookup) x.Description, _Description)
            End Sub

            Private m_Description As _String
            Public Property Description As _String
                Get
                    Return m_Description
                End Get
                Private Set
                    m_Description = Value
                End Set
            End Property
        End Class

        Public Shared ReadOnly _ProductDescriptionID As Mounter(Of _Int32) = RegisterColumn(Function(x As ProductDescription) x.ProductDescriptionID)
        Public Shared ReadOnly _Description As Mounter(Of _String) = RegisterColumn(Function(x As ProductDescription) x.Description)

        Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
            Return New PK(ProductDescriptionID)
        End Function

        Private m_ProductDescriptionID As _Int32
        <Identity(1, 1)>
        <DbColumn(Description:="Primary key for ProductDescription records.")>
        Public Property ProductDescriptionID As _Int32
            Get
                Return m_ProductDescriptionID
            End Get
            Private Set
                m_ProductDescriptionID = Value
            End Set
        End Property

        Private m_Description As _String
        <Required>
        <AsNVarChar(400)>
        <DbColumn(Description:="Description of the product.")>
        Public Property Description As _String
            Get
                Return m_Description
            End Get
            Private Set
                m_Description = Value
            End Set
        End Property
    End Class
End Namespace

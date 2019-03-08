<Rule(SalesOrderInfoDetail._Rule_ProductNumber)>
<Rule(SalesOrderInfoDetail._Rule_ProductName)>
<InvisibleToDbDesigner>
Public Class SalesOrderInfoDetail
    Inherits SalesOrderDetail

    Shared Sub New()
        RegisterProjection(Function(x As SalesOrderInfoDetail) x.Product)
    End Sub

    Private m_Product As Product.Lookup
    Public Property Product As Product.Lookup
        Get
            Return m_Product
        End Get
        Private Set
            m_Product = Value
        End Set
    End Property

    Friend Const _Rule_ProductNumber = NameOf(Rule_ProductNumber)
    <_Rule>
    Private ReadOnly Property Rule_ProductNumber As Rule
        Get
            Dim validate =
                Function(dataRow As DataRow) As String
                    If ProductID(dataRow) Is Nothing Then Return Nothing
                    Dim productNumber = Product.ProductNumber
                    If String.IsNullOrEmpty(productNumber(dataRow)) Then Return String.Format(My.UserMessages.Validation_ValueIsRequired, productNumber.DisplayName)
                    Return Nothing
                End Function

            Dim getSourceColumns =
                Function() As IColumns
                    Return Product.ProductNumber
                End Function

            Return New Rule(validate, getSourceColumns)
        End Get
    End Property

    Friend Const _Rule_ProductName = NameOf(Rule_ProductName)
    <_Rule>
    Private ReadOnly Property Rule_ProductName As Rule
        Get
            Dim validate =
                Function(dataRow As DataRow) As String
                    If ProductID(dataRow) Is Nothing Then Return Nothing
                    Dim productName = Product.Name
                    If String.IsNullOrEmpty(productName(dataRow)) Then Return String.Format(My.UserMessages.Validation_ValueIsRequired, productName.DisplayName)
                    Return Nothing
                End Function

            Dim getSourceColumns =
                Function() As IColumns
                    Return Product.Name
                End Function

            Return New Rule(validate, getSourceColumns)
        End Get
    End Property
End Class

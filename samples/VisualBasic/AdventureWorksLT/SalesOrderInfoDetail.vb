<Rule(SalesOrderInfoDetail._ValidateProductNumber, SourceColumns:={NameOf(SalesOrderDetail.ProductID), NameOf(SalesOrderInfoDetail.Product) + "." + NameOf(Product.ProductNumber)})>
<Rule(SalesOrderInfoDetail._ValidateProductName, SourceColumns:={NameOf(SalesOrderDetail.ProductID), NameOf(SalesOrderInfoDetail.Product) + "." + NameOf(Product.Name)})>
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

    Friend Const _ValidateProductNumber = NameOf(ValidateProductNumber)
    <_Rule>
    Private Function ValidateProductNumber(ByVal dataRow As DataRow) As DataValidationError
        If ProductID(dataRow) Is Nothing Then Return Nothing
        Dim productNumber = Product.ProductNumber
        If String.IsNullOrEmpty(productNumber(dataRow)) Then Return New DataValidationError(String.Format(My.UserMessages.Validation_ValueIsRequired, productNumber.DisplayName), productNumber)
        Return Nothing
    End Function

    Friend Const _ValidateProductName = NameOf(ValidateProductName)
    <_Rule>
    Private Function ValidateProductName(ByVal dataRow As DataRow) As DataValidationError
        If ProductID(dataRow) Is Nothing Then Return Nothing
        Dim productName = Product.Name
        If String.IsNullOrEmpty(productName(dataRow)) Then Return New DataValidationError(String.Format(My.UserMessages.Validation_ValueIsRequired, productName.DisplayName), productName)
        Return Nothing
    End Function
End Class

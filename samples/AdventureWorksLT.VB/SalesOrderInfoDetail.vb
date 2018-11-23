<Validator(SalesOrderInfoDetail._ValidateProduct)>
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

    Friend Const _ValidateProduct = NameOf(ValidateProduct)
    <_Validator>
    Private Function ValidateProduct(ByVal dataRow As DataRow) As DataValidationError
        If ProductID(dataRow) Is Nothing Then Return Nothing
        Dim productNumber = Product.ProductNumber
        Dim productName = Product.Name
        If String.IsNullOrEmpty(productNumber(dataRow)) Then Return New DataValidationError(String.Format(UserMessages.Validation_ValueIsRequired, productNumber.DisplayName), productNumber)
        If String.IsNullOrEmpty(productName(dataRow)) Then Return New DataValidationError(String.Format(UserMessages.Validation_ValueIsRequired, productName.DisplayName), productName)
        Return Nothing
    End Function
End Class

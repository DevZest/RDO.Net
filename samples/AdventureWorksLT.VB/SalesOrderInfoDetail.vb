Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace DevZest.Samples.AdventureWorksLT
    Public Class SalesOrderInfoDetail
        Inherits SalesOrderDetail

        Shared Sub New()
            RegisterColumnGroup(Function(x As SalesOrderInfoDetail) x.LK_Product)
        End Sub

        Private m_LK_Product As Product.Lookup
        Public Property LK_Product As Product.Lookup
            Get
                Return m_LK_Product
            End Get
            Private Set
                m_LK_Product = Value
            End Set
        End Property

        <ModelValidator>
        Private Function ValidateProduct(ByVal dataRow As DataRow) As DataValidationError
            If ProductID(dataRow) Is Nothing Then Return Nothing
            Dim productNumber = LK_Product.ProductNumber
            Dim productName = LK_Product.Name
            If String.IsNullOrEmpty(productNumber(dataRow)) Then Return New DataValidationError(String.Format(UserMessages.Validation_ValueIsRequired, productNumber.DisplayName), productNumber)
            If String.IsNullOrEmpty(productName(dataRow)) Then Return New DataValidationError(String.Format(UserMessages.Validation_ValueIsRequired, productName.DisplayName), productName)
            Return Nothing
        End Function
    End Class
End Namespace

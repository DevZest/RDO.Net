Imports DevZest.Data
Imports DevZest.Data.Annotations

Namespace DevZest.Samples.AdventureWorksLT
    <ExtraColumns(GetType(Product.Lookup))>
    Public Class SalesOrderInfoDetail
        Inherits SalesOrderDetail

        <ModelValidator>
        Private Function ValidateProduct(ByVal dataRow As DataRow) As DataValidationError
            If ProductID(dataRow) Is Nothing Then Return Nothing
            Dim ext = GetExtraColumns(Of Product.Lookup)()
            Dim productNumber = ext.ProductNumber
            Dim productName = ext.Name
            If String.IsNullOrEmpty(productNumber(dataRow)) Then Return New DataValidationError(String.Format(UserMessages.Validation_ValueIsRequired, productNumber.DisplayName), productNumber)
            If String.IsNullOrEmpty(productName(dataRow)) Then Return New DataValidationError(String.Format(UserMessages.Validation_ValueIsRequired, productName.DisplayName), productName)
            Return Nothing
        End Function
    End Class
End Namespace

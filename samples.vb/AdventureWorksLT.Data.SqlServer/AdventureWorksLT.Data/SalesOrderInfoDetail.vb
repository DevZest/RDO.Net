Imports DevZest.Data

<CustomValidator("VAL_ProductNumber")>
<CustomValidator("VAL_ProductName")>
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

    <_CustomValidator>
    Private ReadOnly Property VAL_ProductNumber As CustomValidatorEntry
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

            Return New CustomValidatorEntry(validate, getSourceColumns)
        End Get
    End Property

    <_CustomValidator>
    Private ReadOnly Property VAL_ProductName As CustomValidatorEntry
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

            Return New CustomValidatorEntry(validate, getSourceColumns)
        End Get
    End Property
End Class

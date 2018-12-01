Imports DevZest.Data

Public Class ProductCategoryMockDb
    Inherits MockDb(Of Db)

    Private Shared Function MockData() As DataSet(Of ProductCategory)
        Return DataSet(Of ProductCategory).ParseJson(My.Strings.Mock_ProductCategory)
    End Function

    Protected Overrides Sub Initialize()
        Mock(Db.ProductCategory, Function() MockData())
    End Sub
End Class

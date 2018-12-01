Imports DevZest.Data

Public Class SalesOrderMockDb
    Inherits MockDb(Of Db)

    Private Shared Function Headers() As DataSet(Of SalesOrderHeader)
        Return DataSet(Of SalesOrderHeader).ParseJson(My.Strings.Mock_SalesOrderHeader)
    End Function

    Private Shared Function Details() As DataSet(Of SalesOrderDetail)
        Return DataSet(Of SalesOrderDetail).ParseJson(My.Strings.Mock_SalesOrderDetail)
    End Function

    Protected Overrides Sub Initialize()
        Mock(Db.SalesOrderHeader, Function() Headers())
        Mock(Db.SalesOrderDetail, Function() Details())
    End Sub
End Class

Imports DevZest.Data

''' <remarks><see cref="SalesOrder"/> And <see cref="SalesOrderDetail"/> are chosen for having foreing key to non-existing table(s) And
''' parent-child relationship.</remarks>
Public Class EmptySalesOrderMockDb
    Inherits MockDb(Of Db)

    ' The order of mocking table does Not matter, the dependencies will be sorted out automatically.
    Protected Overrides Sub Initialize()
        Mock(Db.SalesOrderDetail)
        Mock(Db.SalesOrderHeader)
    End Sub
End Class

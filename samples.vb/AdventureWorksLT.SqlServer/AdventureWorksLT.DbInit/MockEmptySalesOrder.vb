Imports System.Threading
Imports DevZest.Data

''' <remarks><see cref="SalesOrder"/> And <see cref="SalesOrderDetail"/> are chosen for having foreing key to non-existing table(s) And
''' parent-child relationship.</remarks>
Public Class MockEmptySalesOrder
    Inherits DbMock(Of Db)

    Public Shared Function CreateAsync(db As Db, Optional progress As IProgress(Of DbInitProgress) = Nothing, Optional ct As CancellationToken = Nothing) As Task(Of Db)
        Return New MockEmptySalesOrder().MockAsync(db, progress, ct)
    End Function

    ' The order of mocking table does Not matter, the dependencies will be sorted out automatically.
    Protected Overrides Sub Initialize()
        Mock(Db.SalesOrderDetail)
        Mock(Db.SalesOrderHeader)
    End Sub
End Class

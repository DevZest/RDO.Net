Imports System.Threading
Imports DevZest.Data.Primitives

Public Class MyMock
    Inherits DbMock(Of DbSession)

    Protected Overrides Sub Initialize()
    End Sub
End Class

Public Class MyMockWithoutWarning
    Inherits DbMock(Of DbSession)

    Public Shared Function CreateAsync(db As DbSession, Optional progress As IProgress(Of DbInitProgress) = Nothing, Optional ct As CancellationToken = Nothing) As Task(Of DbSession)
        Return New MyMockWithoutWarning().MockAsync(db, progress, ct)
    End Function

    Protected Overrides Sub Initialize()
    End Sub
End Class

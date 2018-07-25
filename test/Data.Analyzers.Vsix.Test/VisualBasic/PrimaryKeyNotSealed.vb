Public Class PrimaryKeyNotSealed
    Inherits PrimaryKey

    Public Sub New(id As _Int32)
        MyBase.New(id)
    End Sub

    Public ReadOnly Property ID As _Int32
        Get
            Return GetColumn(Of _Int32)(0)
        End Get
    End Property
End Class

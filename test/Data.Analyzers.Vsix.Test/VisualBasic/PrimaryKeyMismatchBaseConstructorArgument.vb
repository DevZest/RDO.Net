Public NotInheritable Class PrimaryKeyMismatchBaseConstructorArgument
    Inherits PrimaryKey

    Public Sub New(id1 As _Int32, id2 As _Int32)
        MyBase.New(id2, id1)
    End Sub

    Public ReadOnly Property ID1 As _Int32
        Get
            Return GetColumn(Of _Int32)(0)
        End Get
    End Property

    Public ReadOnly Property ID2 As _Int32
        Get
            Return GetColumn(Of _Int32)(1)
        End Get
    End Property
End Class

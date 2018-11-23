Public NotInheritable Class PrimaryKeyMismatchBaseConstructorArgument
    Inherits CandidateKey

    Public Sub New(id1 As _Int32, id2 As _Int32)
        MyBase.New(id2, id1)
    End Sub
End Class

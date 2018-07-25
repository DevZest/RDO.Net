Public NotInheritable Class PrimaryKeyInvalidConstructorParam
    Inherits PrimaryKey

    Public Sub New(id1 As Int32, id2 As _Int32, id3 As Int32)
        MyBase.New(id2)
    End Sub
End Class

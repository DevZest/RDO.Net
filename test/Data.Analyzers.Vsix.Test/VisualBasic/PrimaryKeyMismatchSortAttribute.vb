Imports DevZest.Data.Annotations

Public NotInheritable Class PrimaryKeyMismatchSortAttribute
    Inherits PrimaryKey

    Public Sub New(<Asc()> id1 As _Int32, id2 As _Int32)
        MyBase.New(id1, id2.Desc())
    End Sub
End Class

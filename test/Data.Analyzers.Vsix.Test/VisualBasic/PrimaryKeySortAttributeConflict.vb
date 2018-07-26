Imports DevZest.Data.Annotations

Public NotInheritable Class PrimaryKeySortAttributeConflict
    Inherits PrimaryKey

    Public Sub New(<Asc(), Desc()> id As _Int32)
        MyBase.New(id)
    End Sub

End Class

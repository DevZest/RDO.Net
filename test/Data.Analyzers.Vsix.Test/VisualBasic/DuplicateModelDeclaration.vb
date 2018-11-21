Imports DevZest.Data.Annotations

<CheckConstraint(DuplicateModelDeclaration._CK_AlwaysTrue, "CK")>
<CheckConstraint(DuplicateModelDeclaration._CK_AlwaysTrue, "CK")>
Public Class DuplicateModelDeclaration
    Inherits Model

    Friend Const _CK_AlwaysTrue = NameOf(CK_AlwaysTrue)
    <_CheckConstraint>
    Private ReadOnly Property CK_AlwaysTrue As _Boolean
        Get
            Return _Boolean.Const(True)
        End Get
    End Property
End Class

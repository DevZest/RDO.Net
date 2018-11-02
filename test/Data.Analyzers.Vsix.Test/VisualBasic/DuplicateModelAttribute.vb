﻿Imports DevZest.Data.Annotations

<CheckConstraint(DuplicateModelAttribute._CK_AlwaysTrue, "CK")>
<CheckConstraint(DuplicateModelAttribute._CK_AlwaysTrue, "CK")>
Public Class DuplicateModelAttribute
    Inherits Model

    Friend Const _CK_AlwaysTrue = NameOf(CK_AlwaysTrue)
    <_CheckConstraint>
    Private ReadOnly Property CK_AlwaysTrue As _Boolean
        Get
            Return _Boolean.Const(True)
        End Get
    End Property
End Class

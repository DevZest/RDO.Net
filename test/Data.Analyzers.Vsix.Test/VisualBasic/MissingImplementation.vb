Imports DevZest.Data.Annotations

<CheckConstraint("CK_AlwaysTrue", "CK")>
Public Class MissingImplementation
    Inherits Model

    Private Shared ReadOnly Property CK_AlwaysTrue As _Boolean
        Get
            Return _Boolean.Const(True)
        End Get
    End Property
End Class

Imports DevZest.Data.Annotations

Public Class ModelMemberAttributeRequiresArgument
    Inherits Model

    Shared Sub New()
        RegisterColumn(Function(x As ModelMemberAttributeRequiresArgument) x.ID)
    End Sub

    Private m_ID As _Int32
    <DbColumn>
    Public Property ID As _Int32
        Get
            Return m_ID
        End Get
        Private Set
            m_ID = Value
        End Set
    End Property

End Class

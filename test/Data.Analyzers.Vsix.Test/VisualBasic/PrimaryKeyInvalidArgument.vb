Public Class PrimaryKeyInvalidArgument
    Inherits Model(Of PrimaryKeyInvalidArgument.PK)

    Public NotInheritable Class PK
        Inherits PrimaryKey
        Public Sub New(id As _Int32)
            MyBase.New(id)
        End Sub
    End Class

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(1)
    End Function

    Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As PrimaryKeyInvalidArgument) x.ID)

    Private m_ID As _Int32
    Public Property ID As _Int32
        Get
            Return m_ID
        End Get
        Private Set
            m_ID = Value
        End Set
    End Property
End Class

Public Class PrimaryKeyArgumentNaming
    Inherits Model(Of PrimaryKeyArgumentNaming.PK)

    Public NotInheritable Class PK
        Inherits PrimaryKey
        Public Sub New(id2 As _Int32)
            MyBase.New(id2)
        End Sub
    End Class

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(ID)
    End Function

    Public Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As PrimaryKeyArgumentNaming) x.ID)

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

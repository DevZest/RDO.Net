Public Class RecursiveModel
    Inherits Model(Of PK)

    Public NotInheritable Class PK
        Inherits CandidateKey

        Public Sub New(id As _Int32)
            MyBase.New(id)
        End Sub
    End Class

    Protected NotOverridable Overrides Function CreatePrimaryKey() As PK
        Return New PK(ID)
    End Function

    Protected Shared ReadOnly _ID As Mounter(Of _Int32) = RegisterColumn(Function(x As RecursiveModel) x.ID)
    Protected Shared ReadOnly _ParentID As Mounter(Of _Int32) = RegisterColumn(Function(x As RecursiveModel) x.ParentID)

    Private m_ID As _Int32
    Public Property ID As _Int32
        Get
            Return m_ID
        End Get
        Private Set
            m_ID = Value
        End Set
    End Property

    Private m_ParentID As _Int32
    Public Property ParentID As _Int32
        Get
            Return m_ParentID
        End Get
        Private Set
            m_ParentID = Value
        End Set
    End Property

    Private m_Child As RecursiveModel
    Public Property Child As RecursiveModel
        Get
            Return m_Child
        End Get
        Set
            m_Child = Value
        End Set
    End Property

    Private m_FK_Parent As PK
    Public ReadOnly Property FK_Parent As PK
        Get
            If m_FK_Parent Is Nothing Then
                m_FK_Parent = New PK(ParentID)
            End If
            Return m_FK_Parent
        End Get
    End Property
End Class

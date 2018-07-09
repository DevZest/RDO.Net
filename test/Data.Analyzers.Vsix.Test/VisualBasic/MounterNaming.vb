Public Class MounterNaming
    Inherits Model

    Protected Shared ReadOnly _Column1 As Mounter(Of _Int32) = RegisterColumn(Function(x As MounterNaming) x.Column2)
    Protected Shared ReadOnly _Column2 As Mounter(Of _Int32)

    Shared Sub New()
        _Column2 = RegisterColumn(Function(x As MounterNaming) x.Column1)
    End Sub

    Private m_Column1 As _Int32
    Public Property Column1 As _Int32
        Get
            Return m_Column1
        End Get
        Private Set
            m_Column1 = Value
        End Set
    End Property

    Private m_Column2 As _Int32
    Public Property Column2 As _Int32
        Get
            Return m_Column2
        End Get
        Private Set
            m_Column2 = Value
        End Set
    End Property
End Class

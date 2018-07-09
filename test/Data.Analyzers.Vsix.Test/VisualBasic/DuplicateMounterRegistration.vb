Public Class DuplicateMounterRegistration
    Inherits Model

    Protected Shared ReadOnly _Column1 As Mounter(Of _Int32) = RegisterColumn(Function(x As DuplicateMounterRegistration) x.Column1)

    Shared Sub New()
        RegisterColumn(Function(x As DuplicateMounterRegistration) x.Column1)
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
End Class

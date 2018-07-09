Public Class InvalidRegisterMounterInvocation
    Inherits Model

    Shared Sub New()
        Dim _column1 = RegisterColumn(Function(x As InvalidRegisterMounterInvocation) x.Column1)
    End Sub

    Private Shared Sub AnotherMethod()
        RegisterColumn(Function(x As InvalidRegisterMounterInvocation) x.Column2)
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

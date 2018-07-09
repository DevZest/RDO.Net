Public Class InvalidRegisterMounterGetterParam
    Inherits Model

    Private Class AnotherModel
        Inherits Model

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

    Shared Sub New()
        RegisterColumn(Function(x As InvalidRegisterMounterGetterParam) x.Column1)
        RegisterColumn(Function(x As AnotherModel) x.Column1)
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

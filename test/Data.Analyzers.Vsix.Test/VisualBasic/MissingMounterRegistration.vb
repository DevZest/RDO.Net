Public Class MissingMounterRegistration
    Inherits Model

    Private m_Column As _Int32
    Public Property Column As _Int32
        Get
            Return m_Column
        End Get
        Private Set
            m_Column = Value
        End Set
    End Property

    Private m_NotAColumn As Int32
    Public Property NotAColumn As Int32
        Get
            Return m_NotAColumn
        End Get
        Private Set
            m_NotAColumn = Value
        End Set
    End Property
End Class

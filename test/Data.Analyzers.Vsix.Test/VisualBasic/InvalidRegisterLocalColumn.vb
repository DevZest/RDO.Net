Public Class InvalidRegisterLocalColumn
    Inherits Model

    Shared Sub New()
        RegisterColumn(Function(x As InvalidRegisterLocalColumn) x.Column1)
    End Sub

    Private m_Column1 As LocalColumn(Of String)
    Public Property Column1 As LocalColumn(Of String)
        Get
            Return m_Column1
        End Get
        Private Set
            m_Column1 = Value
        End Set
    End Property
End Class

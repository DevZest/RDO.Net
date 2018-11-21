Imports DevZest.Data.Annotations

Public Class ModelDesignerSpecInvalidType
    Inherits Model

    Shared Sub New()
        RegisterColumn(Function(x As ModelDesignerSpecInvalidType) x.ID)
        RegisterColumn(Function(x As ModelDesignerSpecInvalidType) x.Name)
    End Sub

    Private m_ID As _Int32
    <CreditCard>
    Public Property ID As _Int32
        Get
            Return m_ID
        End Get
        Private Set
            m_ID = Value
        End Set
    End Property

    Private m_Name As _String
    <Identity(1, 1)>
    Public Property Name As _String
        Get
            Return m_Name
        End Get
        Private Set
            m_Name = Value
        End Set
    End Property
End Class

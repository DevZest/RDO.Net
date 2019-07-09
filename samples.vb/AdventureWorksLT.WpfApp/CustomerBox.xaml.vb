Imports DevZest.Data

Public Class CustomerBox
    Public Sub New()
        InitializeComponent()
    End Sub

    Public Shared ReadOnly Property RefreshAction() As Action(Of CustomerBox, ColumnValueBag, Customer.Lookup)
        Get
            Return AddressOf Refresh
        End Get
    End Property

    Private Shared Sub Refresh(v As CustomerBox, valueBag As ColumnValueBag, e As Customer.Lookup)
        v._companyName.Text = valueBag.GetValue(e.CompanyName)
        v._contactPerson.Text = Customer.GetContactPerson(valueBag.GetValue(e.LastName), valueBag.GetValue(e.FirstName), valueBag.GetValue(e.Title))
        v._phone.Text = valueBag.GetValue(e.Phone)
        v._email.Text = valueBag.GetValue(e.EmailAddress)
    End Sub
End Class

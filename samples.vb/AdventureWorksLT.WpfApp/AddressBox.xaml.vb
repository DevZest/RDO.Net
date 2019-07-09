Imports DevZest.Data

Public Class AddressBox
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Shared Sub Refresh(v As AddressBox, valueBag As ColumnValueBag, e As Address.Lookup)
        v._addressLine1.Text = valueBag.GetValue(e.AddressLine1)
        v._addressLine2.Text = valueBag.GetValue(e.AddressLine2)
        v._city.Text = valueBag.GetValue(e.City)
        v._stateProvince.Text = valueBag.GetValue(e.StateProvince)
        v._countryRegion.Text = valueBag.GetValue(e.CountryRegion)
        v._postalCode.Text = valueBag.GetValue(e.PostalCode)
    End Sub

    Public Shared ReadOnly Property RefreshAction() As Action(Of AddressBox, ColumnValueBag, Address.Lookup)
        Get
            Return AddressOf Refresh
        End Get
    End Property
End Class

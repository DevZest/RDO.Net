# ForeignKeyBox

<xref:DevZest.Data.Views.ForeignKeyBox> represents a clickable box to display and edit foreign key data, as shown in `AdventureWorksLT.WpfApp` sample:

![image](/images/ForeignKeyBox.jpg)

## Features

* Data binding to display foreign key as lookup data. For example, instead of displaying the `AddressID` foreign key column, the lookup data of `AddressLine1`, `AddressLine2`, `City`, `StateProvince`, `CountryRegion` and `PostalCode` are displayed.
* Provides an clickable icon to clear underlying foreign key data.
* Clicking the <xref:DevZest.Data.Views.ForeignKeyBox> will launch a new view of lookup data, typically a drop-down list as popup window, or a model dialog window, as shown in preceding example.

## Usage

First, add <xref:DevZest.Data.Views.ForeignKeyBox> as row binding via <xref:DevZest.Data.Presenters.BindingFactory.BindToForeignKeyBox*>, as demonstrated in `AdventureWorksLT.WpfApp` sample:

# [C#](#tab/cs)

```csharp
public static RowCompositeBinding<SalesOrderHeaderBox> BindToSalesOrderHeaderBox(this SalesOrderInfo _, bool isNew,
    out RowBinding<ForeignKeyBox> shipToAddressBinding, out RowBinding<ForeignKeyBox> billToAddressBinding)
{
    var result = new RowCompositeBinding<SalesOrderHeaderBox>()
        .AddChild(_.FK_Customer.BindToForeignKeyBox(_.Customer, CustomerBox.RefreshAction), v => v._customer)
        .AddChild(shipToAddressBinding = _.FK_ShipToAddress.BindToForeignKeyBox(_.ShipToAddress, AddressBox.RefreshAction), v => v._shipTo)
        .AddChild(billToAddressBinding = _.FK_BillToAddress.BindToForeignKeyBox(_.BillToAddress, AddressBox.RefreshAction), v => v._billTo)
    ...
}
```

# [VB.Net](#tab/vb)

```vb
Public Function BindToSalesOrderHeaderBox(e As SalesOrderInfo, isNew As Boolean, ByRef shipToAddressBinding As RowBinding(Of ForeignKeyBox),
                                            ByRef billToAddressBinding As RowBinding(Of ForeignKeyBox)) As RowCompositeBinding(Of SalesOrderHeaderBox)
    shipToAddressBinding = e.FK_ShipToAddress.BindToForeignKeyBox(e.ShipToAddress, AddressBox.RefreshAction)
    billToAddressBinding = e.FK_BillToAddress.BindToForeignKeyBox(e.BillToAddress, AddressBox.RefreshAction)
    Dim result = New RowCompositeBinding(Of SalesOrderHeaderBox)() _
        .AddChild(e.FK_Customer.BindToForeignKeyBox(e.Customer, CustomerBox.RefreshAction), Function(v) v._customer) _
        .AddChild(shipToAddressBinding, Function(v) v._shipTo) _
        .AddChild(billToAddressBinding, Function(v) v._billTo) _
        ...
    Return result
End Function
```

***

Second, implement <xref:DevZest.Data.Views.ForeignKeyBox.ILookupService> to provide lookup data for foreign key, as demonstrated in `AdventureWorksLT.WpfApp` sample:

# [C#](#tab/cs)

```csharp
private class Presenter : DataPresenter<SalesOrderInfo>, ForeignKeyBox.ILookupService
{
    ...
    bool ForeignKeyBox.ILookupService.CanLookup(CandidateKey foreignKey)
    {
        if (foreignKey == _.FK_Customer)
            return true;
        else if (foreignKey == _.FK_BillToAddress)
            return true;
        else if (foreignKey == _.FK_ShipToAddress)
            return true;
        else
            return false;
    }

    void ForeignKeyBox.ILookupService.BeginLookup(ForeignKeyBox foreignKeyBox)
    {
        if (foreignKeyBox.ForeignKey == _.FK_Customer)
        {
            var dialogWindow = new CustomerLookupWindow();
            dialogWindow.Show(_ownerWindow, foreignKeyBox, CurrentRow.GetValue(_.CustomerID), _shipToAddressBinding[CurrentRow], _billToAddressBinding[CurrentRow]);
        }
        else if (foreignKeyBox.ForeignKey == _.FK_ShipToAddress || foreignKeyBox.ForeignKey == _.FK_BillToAddress)
            BeginLookupAddress(foreignKeyBox);
        else
            throw new NotSupportedException();
    }

    private void BeginLookupAddress(ForeignKeyBox foreignKeyBox)
    {
        var foreignKey = (Address.PK)foreignKeyBox.ForeignKey;
        if (_addressLookupPopup.FK == foreignKey)
            _addressLookupPopup.IsOpen = false;
        else
        {
            var customerID = CurrentRow.GetValue(_.CustomerID);
            if (customerID.HasValue)
            {
                var addressID = foreignKeyBox.ForeignKey == _.FK_ShipToAddress ? _.ShipToAddressID : _.BillToAddressID;
                _addressLookupPopup.Show(foreignKeyBox, CurrentRow.GetValue(addressID), customerID.Value);
            }
        }
    }
}
```

# [VB.Net](#tab/vb)

```vb
Private Class Presenter
    Inherits DataPresenter(Of SalesOrderInfo)
    Implements ForeignKeyBox.ILookupService
    ...
    Function CanLookup(foreignKey As CandidateKey) As Boolean Implements ForeignKeyBox.ILookupService.CanLookup
        If foreignKey Is Entity.FK_Customer Then
            Return True
        ElseIf foreignKey Is Entity.FK_BillToAddress Then
            Return True
        ElseIf foreignKey Is Entity.FK_ShipToAddress Then
            Return True
        Else
            Return False
        End If
    End Function

    Sub BeginLookup(foreignKeyBox As ForeignKeyBox) Implements ForeignKeyBox.ILookupService.BeginLookup
        If foreignKeyBox.ForeignKey Is Entity.FK_Customer Then
            Dim dialogWindow As New CustomerLookupWindow()
            dialogWindow.Show(_ownerWindow, foreignKeyBox, CurrentRow.GetValue(Entity.CustomerID), _shipToAddressBinding(CurrentRow), _billToAddressBinding(CurrentRow))
        ElseIf foreignKeyBox.ForeignKey Is Entity.FK_ShipToAddress OrElse foreignKeyBox.ForeignKey Is Entity.FK_BillToAddress Then
            BeginLookupAddress(foreignKeyBox)
        Else
            Throw New NotSupportedException()
        End If
    End Sub

    Private Sub BeginLookupAddress(foreignKeyBox As ForeignKeyBox)
        Dim foreignKey As Address.PK = CType(foreignKeyBox.ForeignKey, Address.PK)
        If _addressLookupPopup.FK Is foreignKey Then
            _addressLookupPopup.IsOpen = False
        Else
            Dim customerID = CurrentRow.GetValue(Entity.CustomerID)
            If customerID.HasValue Then
                Dim addressID = If(foreignKeyBox.ForeignKey Is Entity.FK_ShipToAddress, Entity.ShipToAddressID, Entity.BillToAddressID)
                _addressLookupPopup.Show(foreignKeyBox, CurrentRow.GetValue(addressID), customerID.Value)
            End If
        End If
    End Sub
    ...
End Class
```

***

## Implemented Commands

| Command | Instance | Input | Implementation |
|---------|----------|-------|----------------|
| <xref:DevZest.Data.Views.ForeignKeyBox.Commands.ClearValue> | <xref:DevZest.Data.Views.ForeignKeyBox> | Control template. | Clears underlying foreign key data. |

## Customizable Services

* <xref:DevZest.Data.Views.ForeignKeyBox.ICommandService>: Your data presenter can implement this service to change the commands implementation of this class.
* <xref:DevZest.Data.Views.ForeignKeyBox.ILookupService>: There is no default implementation. Your data presenter must implement this interface to lookup data from another view, as shown in preceding example.

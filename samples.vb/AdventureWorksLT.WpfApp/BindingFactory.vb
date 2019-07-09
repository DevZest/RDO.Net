Imports DevZest.Data.Presenters
Imports DevZest.Data.Views

Public Module BindingFactory
    <System.Runtime.CompilerServices.Extension>
    Public Function BindToSalesOrderHeaderBox(e As SalesOrderInfo, isNew As Boolean, ByRef shipToAddressBinding As RowBinding(Of ForeignKeyBox),
                                              ByRef billToAddressBinding As RowBinding(Of ForeignKeyBox)) As RowCompositeBinding(Of SalesOrderHeaderBox)
        shipToAddressBinding = e.FK_ShipToAddress.BindToForeignKeyBox(e.ShipToAddress, AddressBox.RefreshAction)
        billToAddressBinding = e.FK_BillToAddress.BindToForeignKeyBox(e.BillToAddress, AddressBox.RefreshAction)
        Dim result = New RowCompositeBinding(Of SalesOrderHeaderBox)() _
            .AddChild(e.FK_Customer.BindToForeignKeyBox(e.Customer, CustomerBox.RefreshAction), Function(v) v._customer) _
            .AddChild(shipToAddressBinding, Function(v) v._shipTo) _
            .AddChild(billToAddressBinding, Function(v) v._billTo) _
            .AddChild(e.OrderDate.BindToDatePicker(), Function(v) v._orderDate) _
            .AddChild(e.ShipDate.BindToDatePicker(), Function(v) v._shipDate) _
            .AddChild(e.DueDate.BindToDatePicker(), Function(v) v._dueDate) _
            .AddChild(e.PurchaseOrderNumber.BindToTextBox(), Function(v) v._purchaseOrderNumber) _
            .AddChild(e.AccountNumber.BindToTextBox(), Function(v) v._accountNumber) _
            .AddChild(e.ShipMethod.BindToTextBox(), Function(v) v._shipMethod) _
            .AddChild(e.CreditCardApprovalCode.BindToTextBox(), Function(v) v._creditCardApprovalCode) _
            .AddChild(e.Status.BindToComboBox(), Function(v) v._status) _
            .AddChild(e.OnlineOrderFlag.BindToCheckBox(), Function(v) v._onlineOrderFlag) _
            .AddChild(e.Comment.BindToTextBox(), Function(v) v._comment)
        If Not isNew Then
            result.AddChild(e.SalesOrderNumber.BindToTextBlock(), Function(v) v._salesOrderNumber)
        End If
        Return result
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function BindToSalesOrderFooterBox(Of T As {SalesOrderDetail, New})(e As SalesOrderBase(Of T)) As RowCompositeBinding(Of SalesOrderFooterBox)
        Return New RowCompositeBinding(Of SalesOrderFooterBox)() _
            .AddChild(e.SubTotal.BindToTextBlock("{0:C}"), Function(v) v._subTotal) _
            .AddChild(e.Freight.BindToTextBox(), Function(v) v._freight) _
            .AddChild(e.TaxAmt.BindToTextBox(), Function(v) v._taxAmt) _
            .AddChild(e.TotalDue.BindToTextBlock("{0:C}"), Function(v) v._totalDue)
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function BindToAddressBox(e As Address) As RowCompositeBinding(Of AddressBox)
        Return New RowCompositeBinding(Of AddressBox)() _
            .AddChild(e.AddressLine1.BindToTextBlock(), Function(v) v._addressLine1) _
            .AddChild(e.AddressLine2.BindToTextBlock(), Function(v) v._addressLine2) _
            .AddChild(e.City.BindToTextBlock(), Function(v) v._city) _
            .AddChild(e.StateProvince.BindToTextBlock(), Function(v) v._stateProvince) _
            .AddChild(e.CountryRegion.BindToTextBlock(), Function(v) v._countryRegion) _
            .AddChild(e.PostalCode.BindToTextBlock(), Function(v) v._postalCode)
    End Function
End Module
# DataView

<xref:DevZest.Data.Views.DataView> represents a control that displays scalar and DataSet data, as shown in `AdventureWorksLT.WpfApp` sample:

![image](/images/DataView.jpg)

## Features

<xref:DevZest.Data.Views.DataView> acts as root UI container for scalar and DataSet data presentation. In addition, it implements the following command handlers:

* Asynchronously data loading state.
* Scalar data editing.
* Delete selected row(s).
* Clipboard copy/paste.

## Usage

Typically, <xref:DevZest.Data.Views.DataView> is added to your XAML UI, then call <xref:DevZest.Data.Presenters.DataPresenter`1.Show*> method of your presenter class to show data to the this view.

You can also add <xref:DevZest.Data.Views.DataView> as row binding via <xref:DevZest.Data.Presenters.BindingFactory.BindToDataView*>, to display child DataSet as sub DataView.

## Implemented Commands

| Command | Input | Implementation |
|---------|-------|----------------|
| <xref:DevZest.Data.Views.DataView.Commands.CancelDataLoad> | Control template | Cancels data loading. |
| <xref:DevZest.Data.Views.DataView.Commands.RetryDataLoad> | Control template | Retries data loading. |
| <xref:DevZest.Data.Views.DataView.Commands.ToggleEditScalars> | | Toggles scalar editing mode. |
| <xref:DevZest.Data.Views.DataView.Commands.BeginEditScalars> | | Begins scalar editing mode. |
| <xref:DevZest.Data.Views.DataView.Commands.EndEditScalars> | | Ends scalar editing mode. |
| <xref:DevZest.Data.Views.DataView.Commands.CancelEditScalars> | | Cancels scalar editing mode. |
| <xref:DevZest.Data.Views.DataView.Commands.Delete> | Same as ApplicationCommands.Delete | Deletes selected row(s). |
| <xref:DevZest.Data.Views.DataView.Commands.Copy> | Same as ApplicationCommands.Copy | Copies selected row(s) to clipboard. |
| <xref:DevZest.Data.Views.DataView.Commands.PasteAppend> | Same as ApplicationCommands.Paste | Pastes append data from clipboard. |

## Customizable Services

* <xref:DevZest.Data.Views.DataView.ICommandService>: Your data presenter can implement this service to change the commands implementation of this class.
* <xref:DevZest.Data.Views.DataView.IPasteAppendService>: Your data presenter can implement this interface to verify data to be pasted, for example, retrieve lookup data for foreign key, as demonstrated in `AdventureWorksLT.WpfApp` sample:

# [C#](#tab/cs)

```csharp
bool DataView.IPasteAppendService.Verify(IReadOnlyList<ColumnValueBag> data)
{
    var foreignKeys = DataSet<Product.Ref>.Create();
    for (int i = 0; i < data.Count; i++)
    {
        var valueBag = data[i];
        var productId = valueBag.ContainsKey(_.ProductID) ? valueBag[_.ProductID] : null;
        foreignKeys.AddRow((_, dataRow) =>
        {
            _.ProductID.SetValue(dataRow, productId);
        });
    }

    if (!App.Execute((db, ct) => db.LookupAsync(foreignKeys, ct), Window.GetWindow(View), out var lookup))
        return false;

    Debug.Assert(lookup.Count == data.Count);
    var product = _.Product;
    for (int i = 0; i < lookup.Count; i++)
    {
        data[i].SetValue(product.Name, lookup._.Name[i]);
        data[i].SetValue(product.ProductNumber, lookup._.ProductNumber[i]);
    }
    return true;
}
```

# [VB.Net](#tab/vb)

```vb
Private Function Verify(data As IReadOnlyList(Of ColumnValueBag)) As Boolean Implements DataView.IPasteAppendService.Verify
    Dim foreignKeys = DevZest.Data.DataSet(Of Product.Ref).Create()
    For i = 0 To data.Count
        Dim valueBag = data(i)
        Dim productId = If(valueBag.ContainsKey(Entity.ProductID), valueBag(Entity.ProductID), Nothing)
        foreignKeys.AddRow(Sub(e, dataRow) e.ProductID.SetValue(dataRow, productId))
    Next

    Dim lookup As DataSet(Of Product.Lookup) = Nothing
    If Not App.Execute(Function(db, ct) db.LookupAsync(foreignKeys, ct), Window.GetWindow(View), lookup) Then
        Return False
    End If

    Debug.Assert(lookup.Count = data.Count)
    Dim product = Entity.Product
    For i = 0 To lookup.Count
        data(i).SetValue(product.Name, lookup.Entity.Name(i))
        data(i).SetValue(product.ProductNumber, lookup.Entity.ProductNumber(i))
    Next
    Return True
End Function
```

***

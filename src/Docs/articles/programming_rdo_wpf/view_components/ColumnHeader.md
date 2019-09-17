# ColumnHeader

<xref:DevZest.Data.Views.ColumnHeader> represents a clickable header to identify, resize and sort column of data, as shown in `AdventureWorksLT.WpfApp` sample:

![image](/images/ColumnHeader.jpg)

## Features

* Identifies column data with title.
* Resizes column width via mouse drag-and-drop.
* Sort by current column via single click.
* Sort by multiple columns via context menu.

## Usage

Add <xref:DevZest.Data.Views.ColumnHeader> as scalar binding via <xref:DevZest.Data.Presenters.BindingFactory.BindToColumnHeader*>, as demonstrated in `AdventureWorksLT.WpfApp` sample:

# [C#](#tab/cs)

```csharp
protected override void BuildTemplate(TemplateBuilder builder)
{
    builder
        ...
        .AddBinding(1, 0, _.SalesOrderID.BindToColumnHeader("ID"))
        .AddBinding(2, 0, _.SalesOrderNumber.BindToColumnHeader("Number"))
        .AddBinding(3, 0, _.DueDate.BindToColumnHeader("Due Date"))
        .AddBinding(4, 0, _.ShipDate.BindToColumnHeader("Ship Date"))
        .AddBinding(5, 0, _.Status.BindToColumnHeader("Status"))
        .AddBinding(6, 0, _.PurchaseOrderNumber.BindToColumnHeader("PO Number"))
        .AddBinding(7, 0, _.SubTotal.BindToColumnHeader("Sub Total"))
        .AddBinding(8, 0, _.TaxAmt.BindToColumnHeader("Tax Amt"))
        .AddBinding(9, 0, _.TotalDue.BindToColumnHeader("Total Due"))
        ...;
}
```

# [VB.Net](#tab/vb)

```vb
Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
    Dim e = Entity
    builder
        ...
        .AddBinding(1, 0, e.SalesOrderID.BindToColumnHeader("ID")) _
        .AddBinding(2, 0, e.SalesOrderNumber.BindToColumnHeader("Number")) _
        .AddBinding(3, 0, e.DueDate.BindToColumnHeader("Due Date")) _
        .AddBinding(4, 0, e.ShipDate.BindToColumnHeader("Ship Date")) _
        .AddBinding(5, 0, e.Status.BindToColumnHeader("Status")) _
        .AddBinding(6, 0, e.PurchaseOrderNumber.BindToColumnHeader("PO Number")) _
        .AddBinding(7, 0, e.SubTotal.BindToColumnHeader("Sub Total")) _
        .AddBinding(8, 0, e.TaxAmt.BindToColumnHeader("Tax Amt")) _
        .AddBinding(9, 0, e.TotalDue.BindToColumnHeader("Total Due")) _
        .AddBinding(0, 1, e.BindToCheckBox().WithStyle(Styles.CheckBox)) _
        ...
End Sub
```

***

## Resizing Implementation

Resizing is implemented via [ColumnHeader.IsResizeGripper](xref:DevZest.Data.Views.ColumnHeader.IsResizeGripperProperty) attached property. In the control template of <xref:DevZest.Data.Views.ColumnHeader>, an UIElement with this attached property value set to `true` will detect the mouse drag-and-drop and perform the column resizing operation.

## Implemented Commands

| Command | Instance | Input | Implementation |
|---------|----------|-------|----------------|
| <xref:DevZest.Data.Views.ColumnHeader.Commands.ToggleSortDirection> | <xref:DevZest.Data.Views.ColumnHeader> | Left mouse click | Toggles sort direction for current column. |
| <xref:DevZest.Data.Views.ColumnHeader.Commands.Sort>| <xref:DevZest.Data.Views.DataView> | Context menu of <xref:DevZest.Data.Views.ColumnHeader>. | Displays a dialog via context menu to sort by specifying column(s). |

## Customizable Services

* <xref:DevZest.Data.Views.ColumnHeader.ICommandService>: Your data presenter can implement this service to change the commands implementation of this class.
* <xref:DevZest.Data.Views.ColumnHeader.ISortService>: The default implementation sorts the local <xref:DevZest.Data.Presenters.RowPresenter> objects. Your data presenter can implement this interface to reload sorted data from the server, as shown in `AdventureWorksLT.WpfApp` sample (MainWindow.Presenter.cs/MainWindow.Presenter.vb):

# [C#](#tab/cs)

```csharp
partial class MainWindow
{
    ...
    private class Presenter : DataPresenter<SalesOrderHeader>, ColumnHeader.ISortService
    {
        ...
        private IReadOnlyList<IColumnComparer> _orderBy = new IColumnComparer[] { };
        IReadOnlyList<IColumnComparer> ColumnHeader.ISortService.OrderBy
        {
            get { return _orderBy; }
            set
            {
                _orderBy = value;
                RefreshAsync();
            }
        }

        private Task<DataSet<SalesOrderHeader>> LoadDataAsync(CancellationToken ct)
        {
            return App.ExecuteAsync(db => db.GetSalesOrderHeadersAsync(SearchText, _orderBy, ct));
        }

        public Task RefreshAsync()
        {
            return RefreshAsync(LoadDataAsync);
        }
        ...
    }
}
```

# [VB.Net](#tab/vb)

```vb
Partial Class MainWindow
    ...
    Private Class Presenter
        Inherits DataPresenter(Of SalesOrderHeader)
        Implements ColumnHeader.ISortService

        ...
        Private _orderBy As IReadOnlyList(Of IColumnComparer) = New IColumnComparer() {}
        Overloads Property OrderBy() As IReadOnlyList(Of IColumnComparer) Implements ColumnHeader.ISortService.OrderBy
            Get
                Return _orderBy
            End Get
            Set
                _orderBy = Value
                RefreshAsync()
            End Set
        End Property

        Private Function LoadDataAsync(ct As CancellationToken) As Task(Of DataSet(Of SalesOrderHeader))
            Return App.ExecuteAsync(Function(db) db.GetSalesOrderHeadersAsync(SearchText, _orderBy, ct))
        End Function

        Public Overloads Function RefreshAsync() As Task
            Return RefreshAsync(AddressOf LoadDataAsync)
        End Function
        ...
    End Class
End Class
```

***

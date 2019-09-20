# GridCell

<xref:DevZest.Data.Views.InPlaceEditor> represents the container of single child element that can perform either selection or editing operation, as demonstrated in `AdventureWorksLT.WpfApp` sample:

![image](/images/GridCell.jpg)

## Features

* Select single or consecutive grid cells.
* Copy value of selected grid cells to clipboard.
* In-place editing.

## Usage

Add <xref:DevZest.Data.Views.GridCell> as row binding via <xref:DevZest.Data.Presenters.BindingFactory.MergeIntoGridCell*> or <xref:DevZest.Data.Presenters.BindingFactory.AddToGridCell*>, as demonstrated in `AdventureWorksLT.WpfApp` sample:

# [C#](#tab/cs)

```csharp
protected override void BuildTemplate(TemplateBuilder builder)
{
    builder
        ...
        .AddBinding(1, 1, _.FK_Product.BindToForeignKeyBox(product, GetProductNumber).MergeIntoGridCell(product.ProductNumber.BindToTextBlock()).WithSerializableColumns(_.ProductID, product.ProductNumber))
        .AddBinding(2, 1, product.Name.BindToTextBlock().AddToGridCell().WithSerializableColumns(product.Name))
        .AddBinding(3, 1, _.UnitPrice.BindToTextBox().MergeIntoGridCell())
        .AddBinding(4, 1, _.UnitPriceDiscount.BindToTextBox(new PercentageConverter()).MergeIntoGridCell(_.UnitPriceDiscount.BindToTextBlock("{0:P}")))
        .AddBinding(5, 1, _.OrderQty.BindToTextBox().MergeIntoGridCell())
        .AddBinding(6, 1, _.LineTotal.BindToTextBlock("{0:C}").AddToGridCell().WithSerializableColumns(_.LineTotal));
}
```

# [VB.Net](#tab/vb)

```vb
Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
    Dim e = Entity
    builder
        ...
        .AddBinding(1, 1, e.FK_Product.BindToForeignKeyBox(product, AddressOf GetProductNumber).MergeIntoGridCell(product.ProductNumber.BindToTextBlock()).WithSerializableColumns(e.ProductID, product.ProductNumber)) _
        .AddBinding(2, 1, product.Name.BindToTextBlock().AddToGridCell().WithSerializableColumns(product.Name)) _
        .AddBinding(3, 1, e.UnitPrice.BindToTextBox().MergeIntoGridCell()) _
        .AddBinding(4, 1, e.UnitPriceDiscount.BindToTextBox(New PercentageConverter()).MergeIntoGridCell(e.UnitPriceDiscount.BindToTextBlock("{0:P}"))) _
        .AddBinding(5, 1, e.OrderQty.BindToTextBox().MergeIntoGridCell()) _
        .AddBinding(6, 1, e.LineTotal.BindToTextBlock("{0:C}").AddToGridCell().WithSerializableColumns(e.LineTotal))
End Sub
```

***

## Implemented Commands

| Command | Input | Implementation |
|---------|-------|----------------|
| <xref:DevZest.Data.Views.GridCell.Commands.ToggleMode> | F8 Key | Toggles between selection and editing mode. |
| <xref:DevZest.Data.Views.GridCell.Commands.ExitEditMode> | ESC Key | Exits editing mode. |
| <xref:DevZest.Data.Views.GridCell.Commands.Activate> | Left mouse click | Actives the grid cell for editing. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectTo> | SHIFT + Left mouse click | Selects consecutive grid cells. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectAll> | CTRL-A | Selects all grid cells. |
| <xref:DevZest.Data.Views.GridCell.Commands.Copy> | CTRL-C | Copies content of selected grid cells to clipboard. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectLeft> | LEFT key | Selects grid cell of left. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectRight> | RIGHT key | Selects grid cell of right. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectUp> | UP key | Selects grid cell of up. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectDown> | DOWN key | Selects grid cell of down. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectPageUp> | PageUp key | Selects grid cell of page up. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectPageDown> | PageDown key | Selects grid cell of page down. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectRowHome> | HOME key | Selects grid cells of first row. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectRowEnd> | END key | Selects grid cells of last row. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectHome> | CTRL-HOME key | Selects the first grid cell. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectEnd> | CTRL-END key | Selects the last grid cell. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectToLeft> | SHIFT-LEFT key | Selects consecutive grid cells to left. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectRight> | SHIFT-RIGHT key | Selects consecutive grid cells to right. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectUp> | SHIFT-UP key | Selects consecutive grid cells to up. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectDown> | SHIFT-DOWN key | Selects consecutive grid cells to down. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectPageUp> | SHIFT-PageUp key | Selects consecutive grid cells to page up. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectPageDown> | SHIFT-PageDown key | Selects consecutive grid cells to page down. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectRowHome> | SHIFT-HOME key | Selects consecutive grid cells to first row. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectRowEnd> | SHIFT-END key | Selects consecutive grid cells to last row. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectHome> | CTRL-SHIFT-HOME key | Selects consecutive grid cells to the first grid cell. |
| <xref:DevZest.Data.Views.GridCell.Commands.SelectEnd> | CTRL-SHIFT-END key | Selects consecutive grid cells to the last grid cell. |

## Customizable Services

* <xref:DevZest.Data.Views.GridCell.ICommandService>: Your data presenter can implement this service to change the commands implementation of this class.

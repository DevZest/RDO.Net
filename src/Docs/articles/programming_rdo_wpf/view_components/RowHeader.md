# RowHeader

<xref:DevZest.Data.Views.RowHeader> represents the control displayed as row header that can perform row selection and row resizing operation, as demonstrated in `AdventureWorksLT.WpfApp` sample:

![image](/images/RowHeader.jpg)

## Features

* Row selection.
* Row resizing via drag-and-drop.

## Usage

Put the <xref:DevZest.Data.Views.RowHeader> inside a <xref:DevZest.Data.Views.RowView>, it will be wired up and work automatically.

You can add <xref:DevZest.Data.Views.RowHeader> via row binding by calling <xref:DevZest.Data.Presenters.BindingFactory.BindTo*> API, as demonstrated in `AdventureWorksLT.WpfApp` sample:

# [C#](#tab/cs)

```csharp
protected override void BuildTemplate(TemplateBuilder builder)
{
    builder
        ...
        .AddBinding(0, 1, _.BindTo<RowHeader>())
        ...;
}
```

# [VB.Net](#tab/vb)

```vb
Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
    Dim e = Entity
    builder
        ...
        .AddBinding(0, 1, e.BindTo(Of RowHeader)()) _
        ...
End Sub
```

***

You can also add <xref:DevZest.Data.Views.RowHeader> via composite row binding.

## Resizing Implementation

Resizing is implemented via [RowHeader.IsResizeGripper](xref:DevZest.Data.Views.RowHeader.IsResizeGripperProperty) attached property. In the control template of <xref:DevZest.Data.Views.RowHeader>, an UIElement with this attached property value set to `true` will detect the mouse drag-and-drop and perform the row resizing operation.

## Styles

| Style | Description |
|-------|-------------|
| <xref:DevZest.Data.Views.RowHeader.Styles.Flat> | Displays <xref:DevZest.Data.Views.RowHeader> as flat. |

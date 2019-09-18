# RowView

<xref:DevZest.Data.Views.RowView> represents the container of elements for data row presentation, as shown in `AdventureWorksLT.WpfApp` sample:

![image](/images/RowView.jpg)

## Features

* Container of elements for data row presentation.
* Command handlers for row editing and expand/collapse.

## Usage

<xref:DevZest.Data.Views.RowView> is created and added into the visual tree automatically by RDO.WPF.

In your data presenter template builder, you can:

* Call [TemplateBuilder.RowView\<T\>](xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.RowView*) to specify the style for <xref:DevZest.Data.Views.RowView> or derived type, as demonstrated in `FileExplorer` sample:

```csharp
protected override void BuildTemplate(TemplateBuilder builder)
{
    builder
        ...
        .RowView<RowView>(RowView.Styles.Selectable)
        ...
}
```

* Call [TemplateBuilder.AddBehavior](xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.AddBehavior*) to add a behavior to <xref:DevZest.Data.Views.RowView>. A behavior is a <xref:DevZest.Data.Presenters.RowViewBehavior> object which can dynamically affect the look-and-feel of BlockView/RowView, for example, <xref:DevZest.Data.Presenters.RowViewAlternation> can set different background colors for odd and even rows, as demonstrated in `AdventureWorksLT.WpfApp` sample:

# [C#](#tab/cs)

```csharp
protected override void BuildTemplate(TemplateBuilder builder)
{
    builder
        ...
       .AddBehavior(new RowViewAlternation());
}
```

# [VB.Net](#tab/vb)

```vb
Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
    builder
        ...
        .AddBehavior(New RowViewAlternation())
End Sub
```

***

## Implemented Commands

| Command | Input | Implementation |
|---------|-------|----------------|
| <xref:DevZest.Data.Views.RowView.Commands.ToggleEdit> |  | Toggles editing mode. |
| <xref:DevZest.Data.Views.RowView.Commands.BeginEdit> | F2 Key  | Begins editing mode. |
| <xref:DevZest.Data.Views.RowView.Commands.CancelEdit> | ESC Key  | Cancels editing mode. |
| <xref:DevZest.Data.Views.RowView.Commands.EndEdit> | RETURN Key  | Ends editing mode. |
| <xref:DevZest.Data.Views.RowView.Commands.Expand> | + Key  | Expands current row. |
| <xref:DevZest.Data.Views.RowView.Commands.Collapse> | - Key  | Collapses current row. |

## Customizable Services

* <xref:DevZest.Data.Views.RowView.ICommandService>: Your data presenter can implement this service to change the commands implementation of this class.

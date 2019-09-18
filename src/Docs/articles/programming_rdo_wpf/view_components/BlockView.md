# BlockView

<xref:DevZest.Data.Views.BlockView> represents the container for flowing <xref:DevZest.Data.Views.RowView>, as shown in `FileExplorer` sample:

![image](/images/BlockView.jpg)

## Usage

<xref:DevZest.Data.Views.BlockView> is created and added into the visual tree automatically, when you specify `flowRepeatCount` parameter of <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.Layout*> call in your template builder, to define layout that rows will flow in <xref:DevZest.Data.Views.BlockView> first, then expand afterwards, as demonstrated in `FileExplorer` sample:

```csharp
protected override void BuildTemplate(TemplateBuilder builder)
{
    builder
        ...
        .Layout(Orientation.Vertical, 0)
        ...
}
```

In your data presenter template builder, you can also:

* Call [TemplateBuilder.BlockView\<T\>](xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.BlockView*) to specify the style for <xref:DevZest.Data.Views.BlockView> or derived type.

* Call [TemplateBuilder.AddBehavior](xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.AddBehavior*) to add a behavior to <xref:DevZest.Data.Views.BlockView>. A behavior is a <xref:DevZest.Data.Presenters.BlockViewBehavior> object which can dynamically affect the look-and-feel of BlockView/RowView, for example, like <xref:DevZest.Data.Presenters.RowViewAlternation> can set different background colors for odd and even rows, you can develop your own custom class derived from <xref:DevZest.Data.Presenters.BlockViewBehavior>.

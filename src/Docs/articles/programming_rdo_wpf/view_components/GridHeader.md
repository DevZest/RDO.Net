# GridHeader

<xref:DevZest.Data.Views.GridHeader> represents a button that can select/deselect all rows, as shown in `AdventureWorksLT.WpfApp` sample:

![image](/images/GridHeader.jpg)

## Features

* Toggle button with its `IsChecked` property synchronized with the selection of all rows, via scalar binding.

## Usage

Add <xref:DevZest.Data.Views.GridHeader> as scalar binding via <xref:DevZest.Data.Presenters.BindingFactory.BindToGridHeader*>, as demonstrated in `AdventureWorksLT.WpfApp` sample:

# [C#](#tab/cs)

```csharp
protected override void BuildTemplate(TemplateBuilder builder)
{
    builder
        ...
        .AddBinding(0, 0, this.BindToGridHeader())
        ...;
}
```

# [VB.Net](#tab/vb)

```vb
Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
    Dim e = Entity
    builder
        ...
        .AddBinding(0, 0, Me.BindToGridHeader()) _
        ...
End Sub
```

***

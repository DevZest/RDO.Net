---
uid: view_styles
---

# View Styles

WPF introduces styling, which is to XAML what CSS is to HTML. RDO.WPF provides <xref:DevZest.Data.Presenters.StyleId> class to consume style XAML conveniently by your presenter (C#/VB.Net code).

Taking `AdventureWorksLT.WpfApp` sample:

Step 1. Define static <xref:DevZest.Data.Presenters.StyleId> objects so that it can be used by your C#/VB.Net code later (MainWindow.Presenter.cs/MainWindow.Presenter.vb):

# [C#](#tab/cs)

```csharp
partial class MainWindow
{
    public static class Styles
    {
        public static readonly StyleId CheckBox = new StyleId(typeof(MainWindow));
        public static readonly StyleId LeftAlignedTextBlock = new StyleId(typeof(MainWindow));
        public static readonly StyleId RightAlignedTextBlock = new StyleId(typeof(MainWindow));
        public static readonly StyleId Label = new StyleId(typeof(MainWindow));
    }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
Partial Class MainWindow

    Public NotInheritable Class Styles
        Public Shared ReadOnly CheckBox As New StyleId(GetType(MainWindow))
        Public Shared ReadOnly LeftAlignedTextBlock As New StyleId(GetType(MainWindow))
        Public Shared ReadOnly RightAlignedTextBlock As New StyleId(GetType(MainWindow))
        Public Shared ReadOnly Label As New StyleId(GetType(MainWindow))
    End Class
    ...
End Class
```

***

Step 2. Define styles as XAML resource dictionary, name as the class passed to <xref:DevZest.Data.Presenters.StyleId> constructor plus `.Styles.xaml` (MainWindow.Styles.xaml):

[!code-xaml[MainWindowStyles](../../../../samples/AdventureWorksLT.WpfApp/MainWindow.Styles.xaml)]

Step 3. To resolve the styles resource correctly, define any class under the root namespace with <xref:DevZest.Data.Presenters.ResourceIdRelativeToAttribute> (AssemblyInfo.cs/AssemblyInfo.vb):

# [C#](#tab/cs)

```csharp
[assembly: ResourceIdRelativeTo(typeof(App))]
```

# [VB.Net](#tab/vb)

```vb
<Assembly: ResourceIdRelativeTo(GetType(App))>
```

***

Finally, the styles can be consumed by your presenter code (MainWindow.Presenter.cs/MainWindow.Presenter.vb):

# [C#](#tab/cs)

```csharp
partial class MainWindow
{
    ...
    private class Presenter : DataPresenter<SalesOrderHeader>, ColumnHeader.ISortService
    {
        ...
        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder...
            ...
            .AddBinding(0, 0, this.BindToCheckBox().WithStyle(Styles.CheckBox))
            ...
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
        ...
        Protected Overrides Sub BuildTemplate(builder As TemplateBuilder)
            builder...
                ...
                .AddBinding(0, 0, Me.BindToCheckBox().WithStyle(Styles.CheckBox)) _
                ...
        End Sub
        ...
    End Class
End Class
```

***

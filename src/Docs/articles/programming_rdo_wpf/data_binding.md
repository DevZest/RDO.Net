---
uid: data_binding
---

# Data Binding

Like any presentation framework, data binding plays a very important role in RDO.WPF. Data binding is represented by an object derived from <xref:DevZest.Data.Presenters.Primitives.Binding> class. There are two categories of binding objects in RDO.WPF, basic binding and composite binding:

| Basic Binding | Composite Binding | Binding Context |
|---------------|-------------------|-----------------|
| <xref:DevZest.Data.Presenters.RowBinding`1> | <xref:DevZest.Data.Presenters.RowCompositeBinding`1> | <xref:DevZest.Data.Presenters.RowPresenter> |
| <xref:DevZest.Data.Presenters.ScalarBinding`1> | <xref:DevZest.Data.Presenters.ScalarCompositeBinding`1> | <xref:DevZest.Data.Presenters.ScalarPresenter> |
| <xref:DevZest.Data.Presenters.BlockBinding`1> | <xref:DevZest.Data.Presenters.BlockCompositeBinding`1> | <xref:DevZest.Data.Presenters.BlockPresenter> |

These binding objects are consumed by the presenter to:

* Initialize the binding target UI element;
* Render and arrange the size and position of the binding target UI element in the container element;
* Refresh the binding target UI element if any part of the view is invalidated;
* Cleanup the binding target UI element if no longer needed;
* Validate and update the binding source if this is a two way binding.

## Basic Binding

You can provide 3 delegates as constructor parameter to binding objects, for setup, refresh and cleanup respectively:

| Binding | onSetup/onRefresh/onCleanup |
|---------|-----------------------------|
| <xref:DevZest.Data.Presenters.RowBinding`1> | System.Action<T, <xref:DevZest.Data.Presenters.RowPresenter>> |
| <xref:DevZest.Data.Presenters.ScalarBinding`1> | System.Action<T, <xref:DevZest.Data.Presenters.ScalarPresenter>> |
| <xref:DevZest.Data.Presenters.BlockBinding`1> | System.Action<T, <xref:DevZest.Data.Presenters.BlockPresenter>> |

In RDO.WPF, data binding objects are created by binding factory extension methods. The following example binds a column to a `TextBlock`:

```cs
public static RowBinding<TextBlock> BindToTextBlock(this Column source, string format = null, IFormatProvider formatProvider = null)
{
    if (source == null)
        throw new ArgumentNullException(nameof(source));

    return new RowBinding<TextBlock>(
        onRefresh: (v, p) =>
        {
            v.Text = p[source]?.ToString(format, formatProvider);
        });
}
```

You can find pre-defined binding factory extension methods in static <xref:DevZest.Data.Presenters.BindingFactory> class. You can also easily create your own if one does not exist or suit your needs.

>[!Note]
>Binding factory extension method should only handle binding between source data and target UI element. Do NOT put other presentation logic such as changing the look-and-feel of target UI element in the binding factory extension method.

## Two Way Binding

You can turn <xref:DevZest.Data.Presenters.RowBinding`1> into two way binding by using the following fluent APIs:

* [RowBinding\<T\>.BeginInput](xref:DevZest.Data.Presenters.RowBinding`1.BeginInput*): begins input setup by returning a uninitialized <xref:DevZest.Data.Presenters.RowInput`1> object;
* [RowInput\<T\>.WithFlushingValidator](xref:DevZest.Data.Presenters.RowInput`1.WithFlushingValidator*): initializes the <xref:DevZest.Data.Presenters.RowInput`1> object with validator for flushing data from target UI element to binding source.
* [RowInput\<T\>.WithFlush](xref:DevZest.Data.Presenters.RowInput`1.WithFlush*): initializes the <xref:DevZest.Data.Presenters.RowInput`1> object with delegate for flushing data from target UI element to binding source.
* [RowInput\<T\>.EndInput](xref:DevZest.Data.Presenters.RowInput`1.EndInput*): ends the initialization and returns to the owner <xref:DevZest.Data.Presenters.RowBinding`1> object.

The following example binds a nullable Int32 column to a `TextBox`:

```csharp
public static RowBinding<TextBox> BindToTextBox(this Column<Int32?> source, string flushErrorDescription = null)
{
    return new RowBinding<TextBox>(onSetup: (v, p) => v.Setup(), onCleanup: (v, p) => v.Cleanup(), onRefresh: (v, p) =>
    {
        if (!v.GetIsEditing())
            v.Text = p.GetValue(source).ToString();
    }).BeginInput(TextBox.TextProperty, TextBox.LostFocusEvent)
    .WithFlushingValidator(v =>
    {
        if (string.IsNullOrEmpty(v.Text))
            return null;
        Int32 result;
        return Int32.TryParse(v.Text, out result) ? null
        : GetInvalidInputErrorMessage(flushErrorDescription, typeof(Int32));
    })
    .WithFlush(source, v =>
    {
        if (string.IsNullOrEmpty(v.Text))
            return null;
        else
            return Int32.Parse(v.Text);
    })
    .EndInput();
}
```

The above fluent API call can also be simplified into one single <xref:DevZest.Data.Presenters.RowBinding`1.WithInput*> call, as shown in the following example binding a nullable boolean column to a `CheckBox`:

```cs
public static RowBinding<CheckBox> BindToCheckBox(this Column<bool?> source, string display = null)
{
    if (source == null)
        throw new ArgumentNullException(nameof(source));
    if (string.IsNullOrEmpty(display))
        display = source.DisplayName;
    return new RowBinding<CheckBox>(
        onSetup: (v, p) =>
        {
            if (v.Content == null)
                v.Content = display;
        },
        onRefresh: (v, p) => v.IsChecked = p.GetValue(source), onCleanup: null)
        .WithInput(CheckBox.IsCheckedProperty, source, v => v.IsChecked);
}
```

You can turn <xref:DevZest.Data.Presenters.ScalarBinding`1> into two way binding similarly. Two way binding is not supported by <xref:DevZest.Data.Presenters.BlockBinding`1>.

## Composite Binding

Composite binding object is a binding object which contains a collection of child bindings and consumed as single target UI element. Composite binding is provided to bypass the default grid layout system. You can define a user control with custom layout, and bind data sources to children of the user control. The composite binding of the user control, is then consumed by presenter as a whole.

For example, in AdventureWorkLT.WpfApp sample, there is an `AddressBox` user control:

[!code-xaml[AddressBox](../../../../samples/AdventureWorksLT.WpfApp/AddressBox.xaml)]

The following binding factory extension method binds `Address` model to `AddressBox` target UI element via [RowCompositeBinding\<T\>.AddChild](xref:DevZest.Data.Presenters.RowCompositeBinding`1.AddChild*) API:

```cs
public static RowCompositeBinding<AddressBox> BindToAddressBox(this Address _)
{
    return new RowCompositeBinding<AddressBox>()
        .AddChild(_.AddressLine1.BindToTextBlock(), v => v._addressLine1)
        .AddChild(_.AddressLine2.BindToTextBlock(), v => v._addressLine2)
        .AddChild(_.City.BindToTextBlock(), v => v._city)
        .AddChild(_.StateProvince.BindToTextBlock(), v => v._stateProvince)
        .AddChild(_.CountryRegion.BindToTextBlock(), v => v._countryRegion)
        .AddChild(_.PostalCode.BindToTextBlock(), v => v._postalCode);
}
```

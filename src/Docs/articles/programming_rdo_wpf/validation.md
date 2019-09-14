---
uid: rdo_wpf_validation
---

# Validation

RDO.WPF supports validation extensively, for both scalar data and underlying DataSet. You can specify various validators, and fully control how validation errors will be displayed in UI.

For complete examples, please refer to the `ValidationUI` sample.

## Validators

RDO.WPF supports the following validators:

* Flushing validator: Flushing validator reports data binding conversion error before model updated, for example, non-numeric characters entered for a number field. Flushing validator is implemented in <xref:data_binding> factory extension method, var `WithFlushValidator` API. For example, the following binding factory method binds a nullable Int32 column to a `TextBox`:

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

* Model validator: As explained in <xref:model_validation>, you can define validators in your model via validation attributes and custom validators.

* Async validator: Validation against external source such as web requests, database calls or some other kind of actions which require significant amount of time. For example, in a user registration form, to validate whether the entered email address is already registered. You can add async validator to your data presenter via `AddAsyncValidator` API of your template builder, as demonstrated in `ValidationUI` sample:

```csharp
protected override void BuildTemplate(TemplateBuilder builder)
{
    var userName = _userName.BindToTextBox();
    ...

    builder.AddAsyncValidator(userName.Input, ValidateUserNameFunc, "User Name")
        ...;
}

protected Func<Task<string>> ValidateUserNameFunc
{
    get { ... }
}
```

## Validation Mode

You can specify the following validation mode via <xref:DevZest.Data.Presenters.TemplateBuilder`1.WithScalarValidationMode*> and <xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.WithRowValidationMode*>:

| Mode | Description |
|------|-------------|
| Progressive | Validation error is not visible until value is edited. |
| Implicit | Validation error is always visible since the very beginning. |
| Explicit | Validation error is not visible until `Validate` is explicitly called. |

## Validation Presenter and View

Your presenter object's <xref:DevZest.Data.Presenters.BasePresenter.ScalarValidation> and <xref:DevZest.Data.Presenters.DataPresenter.RowValidation> property exposes all the validation logic. The validation errors will be automatically associated with the two way data binding target UI element based on the validation source (either a <xref:DevZest.Data.Presenters.IScalars> or a <xref:DevZest.Data.IColumns> object). These validation errors will then be displayed  via the following template provided by attached properties of static <xref:DevZest.Data.Presenters.Validation> class:

| Attached Property | Description |
|-------------------|-------------|
| Validation.Status | A value of <xref:DevZest.Data.Presenters.ValidationStatus>. |
| Validation.SucceededTemplate | Template for `Succeeded` status. |
| Validation.FlushingFailedTemplate | Template for `FlushingFailed` status. |
| Validation.FailedTemplate | Template for `Failed` status. |
| Validation.ValidatingTemplate | Template for `Validating` status. |

The following view components are provided specially for validation error display, as demonstrated in `ValidationUI` sample:

* <xref:DevZest.Data.Views.ValidationPlaceholder>: Displays validation errors for specified <xref:DevZest.Data.Presenters.IScalars> or <xref:DevZest.Data.IColumns>.
* <xref:DevZest.Data.Views.ValidationErrorsControl>: Displays all validation errors as list.

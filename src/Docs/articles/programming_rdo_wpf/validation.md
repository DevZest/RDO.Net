---
uid: rdo_wpf_validation
---

# Validation

RDO.WPF supports validation extensively, for both scalar data and underlying DataSet. You can specify various validators, and fully control how validation errors will be displayed in UI.

For complete examples, please refer to the `ValidationUI` sample.

## Validators

RDO.WPF supports the following validators:

### Flushing Validator

Flushing validator reports data binding conversion error before model updated, for example, non-numeric characters entered for a number field. Flushing validator is implemented in <xref:data_binding> factory extension method, var `WithFlushValidator` API. For example, the following binding factory method binds a nullable Int32 column to a `TextBox`:

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

### Model Validator

As explained in <xref:model_validation>, you can define validators in your model via validation attributes and custom validators.

### Scalar Validator

You can use <xref:DevZest.Data.Presenters.Scalar`1.AddValidator*> API to add scalar validator. Additionally, you can override <xref:DevZest.Data.Presenters.BasePresenter.ValidateScalars*> method to implement custom scalar validator cross multiple scalar data. The following code in `ValidationUI` sample demonstrates scalar validators:

```csharp
internal abstract class _RegistrationPresenter : _LoginPresenter
{
    protected Scalar<string> _userName { get; private set; }
    protected Scalar<string> _passwordConfirmation { get; private set; }
    ...

    protected _RegistrationPresenter()
    {
        _userName = NewScalar(string.Empty).AddValidator(ValidateUserNameRequired);
        ...
    }

    private static string ValidateUserNameRequired(string value)
    {
        return string.IsNullOrEmpty(value) ? "Field 'User Name' is required." : null;
    }

    protected override IScalarValidationErrors ValidateScalars(IScalarValidationErrors result)
    {
        if (_password.GetValue() != _passwordConfirmation.GetValue())
            result = result.Add(new ScalarValidationError("Passwords do not match.", _password.Union(_passwordConfirmation).Seal()));
        return result.Seal();
    }

    protected Func<Task<string>> ValidateUserNameFunc
    {
        get { return ValidateUserName; }
    }

    private Task<string> ValidateUserName()
    {
        ...
    }
}
```

### Async Validator

Validation against external source such as web requests, database calls or some other kind of actions which require significant amount of time. For example, in a user registration form, to validate whether the entered email address is already registered. You can add async validator to your data presenter via [TemplateBuilder\<T\>.AddAsyncValidator](xref:DevZest.Data.Presenters.TemplateBuilder`1.AddAsyncValidator*) or [DataPresenter.TemplateBuilder.AddAsyncValidator](xref:DevZest.Data.Presenters.DataPresenter.TemplateBuilder.AddAsyncValidator*) API, as demonstrated in `ValidationUI` sample:

```csharp
protected override void BuildTemplate(TemplateBuilder builder)
{
    var userName = _userName.BindToTextBox();
    ...

    builder.AddAsyncValidator(userName.Input, ValidateUserNameFunc, "User Name")
        ...;
}

...
protected Func<Task<string>> ValidateUserNameFunc
{
    get { return ValidateUserName; }
}

private Task<string> ValidateUserName()
{
    ...;
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

You can call <xref:DevZest.Data.Presenters.BasePresenter.CanSubmitInput> to determine whether data input (both scalar and row) can be submitted without any validation error. You can also try to submit data input via <xref:DevZest.Data.Presenters.BasePresenter.SubmitInput*> API.

You can access validation logic via Your presenter object's <xref:DevZest.Data.Presenters.BasePresenter.ScalarValidation> and <xref:DevZest.Data.Presenters.DataPresenter.RowValidation> property. You can get validation errors that will be displayed in UI via [ScalarValidation.VisibleErrors](xref:DevZest.Data.Presenters.ScalarValidation.VisibleErrors) and [RowPresenter.VisibleValidationErrors](xref:DevZest.Data.Presenters.RowPresenter.VisibleValidationErrors).

Visible validation errors are represented as <xref:DevZest.Data.IValidationErrors> object, which is a collection of objects derived from <xref:DevZest.Data.ValidationError>:

| Validation Error | Validator | Validation Source |
|------------------|-----------|-------------------|
| <xref:DevZest.Data.Presenters.FlushingError> | Flushing validator | `UIElement` |
| <xref:DevZest.Data.DataValidationError> | Model validator/async model validator | <xref:DevZest.Data.IColumns> |
| <xref:DevZest.Data.Presenters.ScalarValidationError> | Scalar validator/async scalar validator | <xref:DevZest.Data.Presenters.IScalars> |
| <xref:DevZest.Data.Presenters.AsyncValidationFault> | Async validator | <xref:DevZest.Data.Presenters.AsyncValidator> |

Visible validation errors will be automatically associated with an UI element in the view, based on the validation source, matched in the order of:

1. Target UI element of two way data binding;
2. RowView (row validation only);
3. DataView.

These validation errors will then be displayed  via the following template provided by attached properties of static <xref:DevZest.Data.Presenters.Validation> class:

| Attached Property | Description |
|-------------------|-------------|
| [Validation.Status](xref:DevZest.Data.Presenters.Validation.StatusProperty) | A value of <xref:DevZest.Data.Presenters.ValidationStatus>. |
| [Validation.SucceededTemplate](xref:DevZest.Data.Presenters.Validation.SucceededTemplateProperty) | Template for `Succeeded` status. |
| [Validation.FailedFlushingTemplate](xref:DevZest.Data.Presenters.Validation.FailedFlushingTemplateProperty) | Template for `FailedFlushing` status. |
| [Validation.FailedTemplate](xref:DevZest.Data.Presenters.Validation.FailedTemplateProperty) | Template for `Failed` status. |
| [Validation.ValidatingTemplate](xref:DevZest.Data.Presenters.Validation.ValidatingTemplateProperty) | Template for `Validating` status. |

The following view components are provided specially for validation error display, as demonstrated in `ValidationUI` sample:

* <xref:DevZest.Data.Views.ValidationPlaceholder>: Displays validation errors for specified <xref:DevZest.Data.Presenters.IScalars> or <xref:DevZest.Data.IColumns>.
* <xref:DevZest.Data.Views.ValidationErrorsControl>: Displays all validation errors as list.

---
uid: ValidationErrorsControl
---

# ValidationErrorsControl

<xref:DevZest.Data.Views.ValidationErrorsControl> represents a control to display validation errors list, as demonstrated in `ValidationUI` sample:

![image](/images/ValidationErrorsControl.jpg)

## Features

* Displays the validation errors list.

## Usage

Add <xref:DevZest.Data.Views.ValidationErrorsControl> as scalar binding or row binding via <xref:DevZest.Data.Presenters.BindingFactory.BindToValidationErrorsControl*>, as demonstrated in `ValidationUI` sample (composite scalar binding):

```csharp
protected override void BuildTemplate(TemplateBuilder builder)
{
    builder
        ...
        .AddBinding(_window._validationErrorsControl, this.BindToValidationErrorsControl());
}
```

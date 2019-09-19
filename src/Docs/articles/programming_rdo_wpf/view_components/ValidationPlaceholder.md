---
uid: ValidationPlaceholder
---

# ValidationPlaceholder

<xref:DevZest.Data.Views.ValidationPlaceholder> represents a control to display validation error from specified sources, as demonstrated in `ValidationUI` sample:

![image](/images/ValidationPlaceholder.jpg)

## Features

* Displays the validation error from specified sources.

## Usage

Add <xref:DevZest.Data.Views.ValidationPlaceholder> as scalar binding or row binding via <xref:DevZest.Data.Presenters.BindingFactory.BindToValidationPlaceholder*>, as demonstrated in `ValidationUI` sample:

```csharp
protected override void BuildTemplate(TemplateBuilder builder)
{
    var interests1 = _.Interests.BindToCheckBox(Interests.Books);
    var interests2 = _.Interests.BindToCheckBox(Interests.Comics);
    var interests3 = _.Interests.BindToCheckBox(Interests.Hunting);
    var interests4 = _.Interests.BindToCheckBox(Interests.Movies);
    var interests5 = _.Interests.BindToCheckBox(Interests.Music);
    var interests6 = _.Interests.BindToCheckBox(Interests.Physics);
    var interests7 = _.Interests.BindToCheckBox(Interests.Shopping);
    var interests8 = _.Interests.BindToCheckBox(Interests.Sports);
    var interestsValidation = new RowBinding[] { interests1, interests2, interests3, interests4, interests5, interests6, interests7, interests8 }.BindToValidationPlaceholder();
    builder
        ...
        .AddBinding(1, 4, interests1)
        .AddBinding(2, 4, interests2)
        .AddBinding(1, 5, interests3)
        .AddBinding(2, 5, interests4)
        .AddBinding(1, 6, interests5)
        .AddBinding(2, 6, interests6)
        .AddBinding(1, 7, interests7)
        .AddBinding(2, 7, interests8)
        .AddBinding(0, 8, 2, 8, _.BindToValidationErrorsControl()
        ...
}
```

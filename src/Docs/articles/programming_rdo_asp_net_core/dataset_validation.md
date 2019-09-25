# DataSet Validation

RDO.AspNetCore supports DataSet validation, both server side and client side.

## Server Side

<xref:DevZest.Data.AspNetCore.Primitives.DataSetValidatorProvider> which provides custom DataSet model validator, is registered with ASP.Net Core MVC framework. All RDO.Data validation errors will be merged into ASP.Net Core model state automatically.

In addition, you can use <xref:DevZest.Data.AspNetCore.TagHelpers.ValidationMessageTagHelper> to show error message in the view:

```html
<span dataset-validation-for="Movie" dataset-column="@_.Title" class="text-danger"></span>
```

## Client Side

Client side validation is done by generating `data-` HTML attributes that work with a custom jQuery Validate adapter. The following client side validators are provided by RDO.AspNetCore:

| Validator | Description |
|-----------|-------------|
| <xref:DevZest.Data.AspNetCore.ClientValidation.MaxLengthClientValidator> | Client validator for <xref:DevZest.Data.Annotations.MaxLengthAttribute>. |
| <xref:DevZest.Data.AspNetCore.ClientValidation.NumericClientValidator> | Client validator for numeric column. |
| <xref:DevZest.Data.AspNetCore.ClientValidation.RegularExpressionClientValidator> |  Client validator for <xref:DevZest.Data.Annotations.RegularExpressionAttribute>. |
| <xref:DevZest.Data.AspNetCore.ClientValidation.RequiredClientValidator> |  Client validator for <xref:DevZest.Data.Annotations.RequiredAttribute>. |
| <xref:DevZest.Data.AspNetCore.ClientValidation.StringLengthClientValidator> |  Client validator for <xref:DevZest.Data.Annotations.StringLengthAttribute>. |

These attributes are registered automatically with ASP.Net Core framework.

You can also create your own custom client side validation, by:

1. Create your custom client validator class that implementing <xref:DevZest.Data.AspNetCore.ClientValidation.IDataSetClientValidator> interface, or derived from <xref:DevZest.Data.AspNetCore.ClientValidation.DataSetClientValidatorBase`1>.

2. Add your custom client validator object via [DataSetMvcConfiguration.DataSetClientValidators](xref:DevZest.Data.AspNetCore.DataSetMvcConfiguration.DataSetClientValidators) of the <xref:DevZest.Data.AspNetCore.MvcBuilderExtensions.AddDataSetMvc*> call in your `Startup` class.

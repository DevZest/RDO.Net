# DataSet Tag Helpers

RDO.AspNetCore provides tag helpers to support DataSet with following tag helper attributes:

* `dataset-for`: Specifies the DataSet.
* `dataset-validaiton-for`: Specifies the DataSet for validation message.
* `dataset-column`: Specifies the column of the DataSet.
* `dataset-row`: Specifies the data row of the DataSet. If omitted, default to the first row of the DataSet.
* `dataset-items`: Specifies the option elements for Select Tag.

`dataset-for` is used together with `dataset-column` and `dataset-row` as the replacement of `asp-for` attribute, to identify the model expression. Similarly, `dataset-validation-for` is used together with `dataset-column` and `dataset-row` as the replacement of `asp-validation-for` attribute.

`dataset-items` is the RDO.AspNetCore counterpart of `asp-items`.

The following are tag helpers provided by RDO.AspNetCore:

| Tag Helper | Attributes |
|------------|------------|
| <xref:DevZest.Data.AspNetCore.TagHelpers.InputTagHelper> | `dataset-for`, `dataset-column` and/or `dataset-row`. |
| <xref:DevZest.Data.AspNetCore.TagHelpers.LabelTagHelper> | `dataset-for`, `dataset-column` and/or `dataset-row`. |
| <xref:DevZest.Data.AspNetCore.TagHelpers.SelectTagHelper> | `dataset-for`, `dataset-column` and/or `dataset-row`, plus `dataset-items`. |
| <xref:DevZest.Data.AspNetCore.TagHelpers.TextAreaTagHelper> | `dataset-for`, `dataset-column` and/or `dataset-row`. |
| <xref:DevZest.Data.AspNetCore.TagHelpers.ValidationMessageTagHelper> | `dataset-validation-for`, `dataset-column` and/or `dataset-row`. |

To use these tag helpers, add them in your view (.cshtml) file:

```cshtml
@addTagHelper *, DevZest.Data.AspNetCore
```

Then in your view (.cshtml) file:

```html
...
<div class="form-group">
    <label dataset-for="Movie" dataset-column="@_.Title" class="control-label"></label>
    <input dataset-for="Movie" dataset-column="@_.Title" class="form-control" />
    <span dataset-validation-for="Movie" dataset-column="@_.Title" class="text-danger"></span>
</div>
...
```

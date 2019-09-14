---
uid: model_validation
---

# Model Validation

In addition to model constraints, you can also provide model validation via column validation attributes or custom validator.

## Built-in attributes

Here are some of the built-in column validation attributes:

* [CreditCard](xref:DevZest.Data.Annotations.CreditCardAttribute): Validates that the column has a credit card format.
* [EmailAddress](xref:DevZest.Data.Annotations.EmailAddressAttribute): Validates that the column has an email format.
* [Phone](xref:DevZest.Data.Annotations.PhoneAttribute): Validates that the column has a telephone number format.
* [RegularExpression](xref:DevZest.Data.Annotations.RegularExpressionAttribute): Validates that the column value matches a specified regular expression.
* [Required](xref:DevZest.Data.Annotations.RequiredAttribute): Validates that the field is not null.
* [StringLength](xref:DevZest.Data.Annotations.StringLengthAttribute): Validates that a string column value doesn't exceed a specified length limit.
* [Url](xref:DevZest.Data.Annotations.UrlAttribute): Validates that the column has a URL format.

### Error messages

Column validation attributes let you specify the error message to be displayed for invalid input. For example:

# [C#](#tab/cs)

```cs
[StringLength(8, Message = "Name length can't be more than 8.")]
```

# [VB.Net](#tab/vb)

```vb
<StringLength(8, Message = "Name length can't be more than 8.")>
```

***

Internally, the attributes call String.Format with a placeholder for the column name and sometimes additional placeholders. For example:

# [C#](#tab/cs)

```cs
[StringLength(8, Message = "{0} length must be between {2} and {1}.", MinimumLength = 6)]
```

# [VB.Net](#tab/vb)

```vb
<StringLength(8, Message = "{0} length must be between {2} and {1}.", MinimumLength = 6)>
```

***

When applied to a Name property, the error message created by the preceding code would be "Name length must be between 6 and 8.".

To find out which parameters are passed to String.Format for a particular attribute's error message, see the source code.

## Custom attributes

For scenarios that the built-in column validation attributes don't handle, you can create custom validation attributes:

* Create a class that inherits from <xref:DevZest.Data.Annotations.Primitives.ValidationColumnAttribute>;
* Override the `IsValid` method and `DefaultMessageString` property;
* Make sure your custom attribute is correctly decorated with [ModelDesignerSpec](xref:DevZest.Data.Annotations.Primitives.ModelDesignerSpecAttribute) to be friendly to Model Visualizer.

See the source code of built-in column validation attributes as examples.

## Custom validator

Another option for model validation is to implement custom validator with property returns <xref:DevZest.Data.Annotations.CustomValidatorEntry> and a pair of <xref:DevZest.Data.Annotations.CustomValidatorAttribute> and <xref:DevZest.Data.Annotations._CustomValidatorAttribute> in the model class, as shown in the following example:

# [C#](#tab/cs)

```cs
[CustomValidator(nameof(VAL_ProductNumber))]
public class SalesOrderInfoDetail : ...
{
    ...
    [_CustomValidator]
    private CustomValidatorEntry VAL_ProductNumber
    {
        get
        {
            string Validate(DataRow dataRow)
            {
                if (ProductID[dataRow] == null)
                    return null;
                var productNumber = Product.ProductNumber;

                if (string.IsNullOrEmpty(productNumber[dataRow]))
                    return string.Format(UserMessages.Validation_ValueIsRequired, productNumber.DisplayName);
                else
                    return null;
            }

            IColumns GetSourceColumns()
            {
                return Product.ProductNumber;
            }

            return new CustomValidatorEntry(Validate, GetSourceColumns);
        }
    }
    ...
}
```

# [VB.Net](#tab/vb)

```vb
<CustomValidator("VAL_ProductNumber")>
Public Class SalesOrderInfoDetail
    ...
    <_CustomValidator>
    Private ReadOnly Property VAL_ProductNumber As CustomValidatorEntry
        Get
            Dim validate =
                Function(dataRow As DataRow) As String
                    If ProductID(dataRow) Is Nothing Then Return Nothing
                    Dim productNumber = Product.ProductNumber
                    If String.IsNullOrEmpty(productNumber(dataRow)) Then Return String.Format(My.UserMessages.Validation_ValueIsRequired, productNumber.DisplayName)
                    Return Nothing
                End Function

            Dim getSourceColumns =
                Function() As IColumns
                    Return Product.ProductNumber
                End Function

            Return New CustomValidatorEntry(validate, getSourceColumns)
        End Get
    End Property
    ...
End Class
```

***

You can add custom validator by clicking the left top drop down button of Model Visualizer tool window, then click 'Add Custom Validator...':

![image](/images/model_visualizer_add_val.jpg)

The following dialog will be displayed:

![image](/images/model_visualizer_add_val_dialog.jpg)

Fill the dialog form and click "OK", code of a to-be-implemented custom validator will be generated automatically.

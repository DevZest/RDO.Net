using System.Collections.Generic;
using System.Globalization;
using DevZest.Data.Annotations;
using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace DevZest.Data.AspNetCore.ClientValidation
{
    public class MaxLengthClientValidator : DataSetClientValidatorBase<MaxLengthAttribute>
    {
        protected override void AddValidation(MaxLengthAttribute validatorAttribute, ActionContext actionContext, Column column, IDictionary<string, string> attributes)
        {
            MergeAttribute(attributes, "data-val", "true");
            MergeAttribute(attributes, "data-val-maxlength", validatorAttribute.FormatMessage(column));
            MergeAttribute(attributes, "data-val-maxlength-max", validatorAttribute.Length.ToString(CultureInfo.InvariantCulture));
        }
    }
}

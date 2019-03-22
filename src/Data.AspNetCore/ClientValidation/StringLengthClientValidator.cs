using System.Collections.Generic;
using System.Globalization;
using DevZest.Data.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace DevZest.Data.AspNetCore.ClientValidation
{
    public class StringLengthClientValidator : DataSetClientValidatorBase<StringLengthAttribute>
    {
        protected override void AddValidation(StringLengthAttribute validatorAttribute, ActionContext actionContext, Column column, IDictionary<string, string> attributes)
        {
            MergeAttribute(attributes, "data-val", "true");
            MergeAttribute(attributes, "data-val-length", validatorAttribute.FormatMessage(column));

            if (validatorAttribute.MaximumLength != int.MaxValue)
                MergeAttribute(attributes, "data-val-length-max", validatorAttribute.MaximumLength.ToString(CultureInfo.InvariantCulture));

            if (validatorAttribute.MinimumLength != 0)
                MergeAttribute(attributes, "data-val-length-min", validatorAttribute.MinimumLength.ToString(CultureInfo.InvariantCulture));
        }
    }
}

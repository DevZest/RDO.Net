using System.Collections.Generic;
using DevZest.Data.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace DevZest.Data.AspNetCore.ClientValidation
{
    public class RegularExpressionClientValidator : DataSetClientValidatorBase<RegularExpressionAttribute>
    {
        protected override void AddValidation(RegularExpressionAttribute validatorAttribute, ActionContext actionContext, Column column, IDictionary<string, string> attributes)
        {
            MergeAttribute(attributes, "data-val", "true");
            MergeAttribute(attributes, "data-val-regex", validatorAttribute.FormatMessage(column));
            MergeAttribute(attributes, "data-val-regex-pattern", validatorAttribute.Pattern);
        }
    }
}

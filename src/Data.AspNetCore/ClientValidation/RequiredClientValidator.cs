using DevZest.Data.Annotations;
using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DevZest.Data.AspNetCore.ClientValidation
{
    public class RequiredClientValidator : DataSetClientValidator<RequiredAttribute>
    {
        protected override void AddValidation(RequiredAttribute validatorAttribute, ActionContext actionContext, Column column, IDictionary<string, string> attributes)
        {
            MergeAttribute(attributes, "data-val", "true");
            MergeAttribute(attributes, "data-val-required", validatorAttribute.FormatMessage(column));
        }
    }
}

using DevZest.Data.Annotations;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DevZest.Data.AspNetCore.ClientValidation
{
    /// <summary>
    /// Represents client validator for <see cref="RequiredAttribute"/>.
    /// </summary>
    public class RequiredClientValidator : DataSetClientValidatorBase<RequiredAttribute>
    {
        /// <inheritdoc/>
        protected override void AddValidation(RequiredAttribute validatorAttribute, ActionContext actionContext, Column column, IDictionary<string, string> attributes)
        {
            MergeAttribute(attributes, "data-val", "true");
            MergeAttribute(attributes, "data-val-required", validatorAttribute.FormatMessage(column));
        }
    }
}

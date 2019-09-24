using System.Collections.Generic;
using DevZest.Data.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace DevZest.Data.AspNetCore.ClientValidation
{
    /// <summary>
    /// Represents client validator for <see cref="RegularExpressionAttribute"/>.
    /// </summary>
    public class RegularExpressionClientValidator : DataSetClientValidatorBase<RegularExpressionAttribute>
    {
        /// <inheritdoc/>
        protected override void AddValidation(RegularExpressionAttribute validatorAttribute, ActionContext actionContext, Column column, IDictionary<string, string> attributes)
        {
            MergeAttribute(attributes, "data-val", "true");
            MergeAttribute(attributes, "data-val-regex", validatorAttribute.FormatMessage(column));
            MergeAttribute(attributes, "data-val-regex-pattern", validatorAttribute.Pattern);
        }
    }
}

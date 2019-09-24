using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace DevZest.Data.AspNetCore.ClientValidation
{
    /// <summary>
    /// Represents client validator for numeric column.
    /// </summary>
    public class NumericClientValidator : IDataSetClientValidator
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NumericClientValidator"/> class.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public NumericClientValidator(ModelBindingMessageProvider messageProvider)
        {
            _messageProvider = messageProvider ?? throw new ArgumentNullException();
        }

        private readonly ModelBindingMessageProvider _messageProvider;

        /// <summary>
        /// Adds validation HTML attributes.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        /// <param name="column">The column.</param>
        /// <param name="attributes">The HTML attributes dictionary.</param>
        public void AddValidation(ActionContext actionContext, Column column, IDictionary<string, string> attributes)
        {
            var typeToValidate = column.UnderlyingOrDataType();

            // Check only the numeric types for which we set type='text'.
            if (typeToValidate == typeof(float) ||
                typeToValidate == typeof(double) ||
                typeToValidate == typeof(decimal))
            {
                MergeAttribute(attributes, "data-val", "true");
                MergeAttribute(attributes, "data-val-number", GetErrorMessage(column));
            }
        }

        private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
                return;
            attributes.Add(key, value);
        }

        private string GetErrorMessage(Column column)
        {
            return _messageProvider.ValueMustBeANumberAccessor(column.DisplayName);
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace DevZest.Data.AspNetCore.ClientValidation
{
    public class NumericClientValidator : IDataSetClientValidator
    {
        public NumericClientValidator(ModelBindingMessageProvider messageProvider)
        {
            _messageProvider = messageProvider ?? throw new ArgumentNullException();
        }

        private readonly ModelBindingMessageProvider _messageProvider;

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

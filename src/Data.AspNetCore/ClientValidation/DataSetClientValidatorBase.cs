using System;
using System.Collections.Generic;
using DevZest.Data.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace DevZest.Data.AspNetCore.ClientValidation
{
    public abstract class DataSetClientValidatorBase<T> : IDataSetClientValidator
        where T : Attribute
    {
        public void AddValidation(ActionContext actionContext, Column column, IDictionary<string, string> attributes)
        {
            var validators = column.GetParent().Validators;
            for (int i = 0; i < validators.Count; i++)
                AddValidation(validators[i], actionContext, column, attributes);
        }

        private void AddValidation(IValidator validator, ActionContext actionContext, Column column, IDictionary<string, string> attributes)
        {
            if (validator.Attribute is T validatorAttribute && Check(validatorAttribute) && Check(validator.SourceColumns, column))
                AddValidation(validatorAttribute, actionContext, column, attributes);
        }

        protected virtual bool Check(IColumns validatorSourceColumns, Column column)
        {
            return validatorSourceColumns.Contains(column);
        }

        protected virtual bool Check(T validatorAttribute)
        {
            return true;
        }

        /// <summary>
        /// Adds the given <paramref name="key"/> and <paramref name="value"/> into
        /// <paramref name="attributes"/> if <paramref name="attributes"/> does not contain a value for
        /// <paramref name="key"/>.
        /// </summary>
        /// <param name="attributes">The HTML attributes dictionary.</param>
        /// <param name="key">The attribute key.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns><c>true</c> if an attribute was added, otherwise <c>false</c>.</returns>
        protected static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
                return false;

            attributes.Add(key, value);
            return true;
        }

        protected abstract void AddValidation(T validatorAttribute, ActionContext actionContext, Column column, IDictionary<string, string> attributes);
    }
}

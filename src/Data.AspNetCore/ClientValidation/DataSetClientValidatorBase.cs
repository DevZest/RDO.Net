using System;
using System.Collections.Generic;
using DevZest.Data.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace DevZest.Data.AspNetCore.ClientValidation
{
    /// <summary>
    /// Base class of DataSet client validator for attribute.
    /// </summary>
    /// <typeparam name="T">The type of validator attribute.</typeparam>
    public abstract class DataSetClientValidatorBase<T> : IDataSetClientValidator
        where T : Attribute
    {
        /// <summary>
        /// Adds validation HTML attributes.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        /// <param name="column">The column.</param>
        /// <param name="attributes">The HTML attributes dictionary.</param>
        public void AddValidation(ActionContext actionContext, Column column, IDictionary<string, string> attributes)
        {
            var validators = column.GetParent().Validators;
            for (int i = 0; i < validators.Count; i++)
                AddValidation(validators[i], actionContext, column, attributes);
        }

        private void AddValidation(IValidator validator, ActionContext actionContext, Column column, IDictionary<string, string> attributes)
        {
            if (validator.Attribute is T validatorAttribute && Check(validator.SourceColumns, column))
                AddValidation(validatorAttribute, actionContext, column, attributes);
        }

        private bool Check(IColumns validatorSourceColumns, Column column)
        {
            return validatorSourceColumns.Contains(column);
        }

        /// <summary>
        /// Adds the given <paramref name="key"/> and <paramref name="value"/> into
        /// <paramref name="attributes"/> if <paramref name="attributes"/> does not contain a value for
        /// <paramref name="key"/>.
        /// </summary>
        /// <param name="attributes">The HTML attributes dictionary.</param>
        /// <param name="key">The attribute key.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns><see langword="true"/> if an attribute was added, otherwise <see langword="false"/>.</returns>
        protected static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
                return false;

            attributes.Add(key, value);
            return true;
        }

        /// <summary>
        /// Adds validation HTML attributes.
        /// </summary>
        /// <param name="validatorAttribute">The validator attribute.</param>
        /// <param name="actionContext">The context.</param>
        /// <param name="column">The column.</param>
        /// <param name="attributes">The HTML attributes dictionary.</param>
        protected abstract void AddValidation(T validatorAttribute, ActionContext actionContext, Column column, IDictionary<string, string> attributes);
    }
}

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace DevZest.Data.AspNetCore.Primitives
{
    /// <summary>
    /// Contract for a service providing validation attributes for expressions.
    /// </summary>
    public abstract class DataSetValidationHtmlAttributeProvider
    {
        /// <summary>
        /// Adds validation-related HTML attributes to the <paramref name="attributes" /> if client validation is
        /// enabled.
        /// </summary>
        /// <param name="viewContext">A <see cref="ViewContext"/> instance for the current scope.</param>
        /// <param name="column">The <see cref="Column"/>.</param>
        /// <param name="attributes">
        /// The <see cref="Dictionary{TKey, TValue}"/> to receive the validation attributes. Maps the validation
        /// attribute names to their <see cref="string"/> values. Values must be HTML encoded before they are written
        /// to an HTML document or response.
        /// </param>
        /// <remarks>
        /// Adds nothing to <paramref name="attributes"/> if client-side validation is disabled.
        /// </remarks>
        protected abstract void AddValidationAttributes(ViewContext viewContext, Column column, IDictionary<string, string> attributes);

        /// <summary>
        /// Adds validation-related HTML attributes to the <paramref name="attributes" /> if client validation is
        /// enabled and validation attributes have not yet been added for this <paramref name="expression"/> in the
        /// current &lt;form&gt;.
        /// </summary>
        /// <param name="viewContext">A <see cref="ViewContext"/> instance for the current scope.</param>
        /// <param name="fullHtmlFieldName">The full HTML field name.</param>
        /// <param name="column">The <see cref="Column"/>.</param>
        /// <param name="attributes">
        /// The <see cref="Dictionary{TKey, TValue}"/> to receive the validation attributes. Maps the validation
        /// attribute names to their <see cref="string"/> values. Values must be HTML encoded before they are written
        /// to an HTML document or response.
        /// </param>
        /// <remarks>
        /// Tracks the <paramref name="fullHtmlFieldName"/> in the current <see cref="FormContext"/> to avoid generating
        /// duplicate validation attributes. That is, validation attributes are added only if no previous call has
        /// added them for a field with this name in the &lt;form&gt;.
        /// </remarks>
        public virtual void AddAndTrackValidationAttributes(ViewContext viewContext, string fullHtmlFieldName, Column column, IDictionary<string, string> attributes)
        {
            if (viewContext == null)
                throw new ArgumentNullException(nameof(viewContext));

            if (string.IsNullOrEmpty(fullHtmlFieldName))
                throw new ArgumentNullException(nameof(fullHtmlFieldName));

            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            // Don't track fields when client-side validation is disabled.
            var formContext = viewContext.ClientValidationEnabled ? viewContext.FormContext : null;
            if (formContext == null)
                return;

            if (formContext.RenderedField(fullHtmlFieldName))
                return;

            formContext.RenderedField(fullHtmlFieldName, true);

            AddValidationAttributes(viewContext, column, attributes);
        }
    }
}

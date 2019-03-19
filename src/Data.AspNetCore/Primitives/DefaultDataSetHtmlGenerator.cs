using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DefaultDataSetHtmlGenerator : IDataSetHtmlGenerator
    {
        public DefaultDataSetHtmlGenerator(IOptions<MvcViewOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
                throw new ArgumentNullException(nameof(optionsAccessor));

            // Underscores are fine characters in id's.
            IdAttributeDotReplacement = optionsAccessor.Value.HtmlHelperOptions.IdAttributeDotReplacement;
        }



        // Only need a dictionary if htmlAttributes is non-null. TagBuilder.MergeAttributes() is fine with null.
        private static IDictionary<string, object> GetHtmlAttributeDictionaryOrNull(object htmlAttributes)
        {
            IDictionary<string, object> htmlAttributeDictionary = null;
            if (htmlAttributes != null)
            {
                htmlAttributeDictionary = htmlAttributes as IDictionary<string, object>;
                if (htmlAttributeDictionary == null)
                {
                    htmlAttributeDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                }
            }

            return htmlAttributeDictionary;
        }

        /// <inheritdoc />
        public string IdAttributeDotReplacement { get; }

        /// <inheritdoc />
        public virtual TagBuilder GenerateLabel(ViewContext viewContext, string fullName, Column column, string labelText, object htmlAttributes)
        {
            if (viewContext == null)
                throw new ArgumentNullException(nameof(viewContext));

            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var resolvedLabelText = labelText ?? column.DisplayName;

            var tagBuilder = new TagBuilder("label");
            var idString = NameAndIdProvider.CreateSanitizedId(viewContext, fullName, IdAttributeDotReplacement);
            tagBuilder.Attributes.Add("for", idString);
            tagBuilder.InnerHtml.SetContent(resolvedLabelText);
            tagBuilder.MergeAttributes(GetHtmlAttributeDictionaryOrNull(htmlAttributes), replaceExisting: true);

            return tagBuilder;
        }
    }
}

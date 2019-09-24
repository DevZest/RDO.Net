using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevZest.Data.AspNetCore.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting any HTML element with an <c>dataset-validation-for</c>
    /// attribute.
    /// </summary>
    [HtmlTargetElement("span", Attributes = ValidationForAttributeName + "," + ColumnAttributeName)]
    public class ValidationMessageTagHelper : DataSetTagHelperBase
    {
        private const string DataValidationForAttributeName = "data-valmsg-for";
        private const string ValidationForAttributeName = "dataset-validation-for";

        /// <summary>
        /// Creates a new <see cref="ValidationMessageTagHelper"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
        public ValidationMessageTagHelper(IDataSetHtmlGenerator generator)
            : base(generator)
        {
        }

        /// <inheritdoc/>
        [HtmlAttributeName(ValidationForAttributeName)]
        public override ModelExpression DataSetFor { get => base.DataSetFor; set => base.DataSetFor = value; }

        /// <inheritdoc/>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Ensure Generator does not throw due to empty "fullName" if user provided data-valmsg-for attribute.
            // Assume data-valmsg-for value is non-empty if attribute is present at all. Should align with name of
            // another tag helper e.g. an <input/> and those tag helpers bind Name.
            IDictionary<string, object> htmlAttributes = null;
            if (string.IsNullOrEmpty(FullHtmlFieldName) && output.Attributes.ContainsName(DataValidationForAttributeName))
            {
                htmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { DataValidationForAttributeName, "-non-empty-value-" },
                };
            }

            string message = null;
            if (!output.IsContentModified)
            {
                var tagHelperContent = await output.GetChildContentAsync();

                // We check for whitespace to detect scenarios such as:
                // <span validation-for="Name">
                // </span>
                if (!tagHelperContent.IsEmptyOrWhiteSpace)
                    message = tagHelperContent.GetContent();
            }

            var tagBuilder = Generator.GenerateValidationMessage(
                ViewContext,
                FullHtmlFieldName,
                Column,
                message: message,
                tag: null,
                htmlAttributes: htmlAttributes);

            if (tagBuilder != null)
            {
                output.MergeAttributes(tagBuilder);

                // Do not update the content if another tag helper targeting this element has already done so.
                if (!output.IsContentModified && tagBuilder.HasInnerHtml)
                    output.Content.SetHtmlContent(tagBuilder.InnerHtml);
            }
        }
    }
}

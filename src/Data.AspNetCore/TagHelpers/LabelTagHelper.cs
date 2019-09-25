using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using DevZest.Data.AspNetCore.Primitives;

namespace DevZest.Data.AspNetCore.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;label&gt; elements with an <c>dataset-*</c> attribute.
    /// </summary>
    [HtmlTargetElement("label", Attributes = DataSetAttributes)]
    public class LabelTagHelper : DataSetTagHelperBase
    {
        /// <summary>
        /// Creates a new <see cref="LabelTagHelper"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
        public LabelTagHelper(IDataSetHtmlGenerator generator)
            : base(generator)
        {
        }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var tagBuilder = Generator.GenerateLabel(ViewContext, FullHtmlFieldName, Column, labelText: null, htmlAttributes: null);

            if (tagBuilder != null)
            {
                output.MergeAttributes(tagBuilder);

                // Do not update the content if another tag helper targeting this element has already done so.
                if (!output.IsContentModified)
                {
                    // We check for whitespace to detect scenarios such as:
                    // <label for="Name">
                    // </label>
                    var childContent = await output.GetChildContentAsync();
                    if (childContent.IsEmptyOrWhiteSpace)
                    {
                        // Provide default label text (if any) since there was nothing useful in the Razor source.
                        if (tagBuilder.HasInnerHtml)
                        {
                            output.Content.SetHtmlContent(tagBuilder.InnerHtml);
                        }
                    }
                    else
                    {
                        output.Content.SetHtmlContent(childContent);
                    }
                }
            }
        }
    }
}
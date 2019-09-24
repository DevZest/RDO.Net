﻿using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace DevZest.Data.AspNetCore.TagHelpers
{
    [HtmlTargetElement("textarea", Attributes = DataSetAttributes)]
    public class TextAreaTagHelper : DataSetTagHelperBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="TextAreaTagHelper"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IDataSetHtmlGenerator"/>.</param>
        public TextAreaTagHelper(IDataSetHtmlGenerator generator)
            : base(generator)
        {
        }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var tagBuilder = Generator.GenerateTextArea(
                ViewContext,
                FullHtmlFieldName,
                Column,
                DataValue,
                rows: 0,
                columns: 0,
                htmlAttributes: null);

            if (tagBuilder != null)
            {
                output.MergeAttributes(tagBuilder);
                if (tagBuilder.HasInnerHtml)
                {
                    // Overwrite current Content to ensure expression result round-trips correctly.
                    output.Content.SetHtmlContent(tagBuilder.InnerHtml);
                }
            }
        }
    }
}

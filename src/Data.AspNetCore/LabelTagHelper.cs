//// Copyright (c) .NET Foundation. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//using System;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using Microsoft.AspNetCore.Razor.TagHelpers;

//namespace DevZest.Data.AspNetCore
//{
//    /// <summary>
//    /// <see cref="ITagHelper"/> implementation targeting &lt;label&gt; elements with an <c>rdo-for</c> and <c>rdo-column</c> attribute.
//    /// </summary>
//    [HtmlTargetElement("label", Attributes = ForAttributeName)]
//    public class LabelTagHelper : TagHelper
//    {
//        private const string ForAttributeName = "rdo-for";
//        private const string ColumnAttributeName = "rdo-column";
//        private const string DataRowAttributeName = "rdo-datarow";

//        /// <summary>
//        /// Creates a new <see cref="LabelTagHelper"/>.
//        /// </summary>
//        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
//        public LabelTagHelper(IHtmlGenerator generator)
//        {
//            Generator = generator;
//        }

//        /// <inheritdoc />
//        public override int Order => -2000;

//        [HtmlAttributeNotBound]
//        [ViewContext]
//        public ViewContext ViewContext { get; set; }

//        protected IHtmlGenerator Generator { get; }

//        /// <summary>
//        /// An <see cref="DataSet"/> expression to be evaluated against the current model.
//        /// </summary>
//        [HtmlAttributeName(ForAttributeName)]
//        public ModelExpression For { get; set; }

//        [HtmlAttributeName(ColumnAttributeName)]
//        public Column Column { get; set; }

//        [HtmlAttributeName(DataRowAttributeName)]
//        public DataRow DataRow { get; set; }

//        /// <inheritdoc />
//        /// <remarks>Does nothing if <see cref="For"/> is <c>null</c>.</remarks>
//        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
//        {
//            if (context == null)
//            {
//                throw new ArgumentNullException(nameof(context));
//            }

//            if (output == null)
//            {
//                throw new ArgumentNullException(nameof(output));
//            }

//            var tagBuilder = Generator.GenerateLabel(
//                ViewContext,
//                For.ModelExplorer,
//                For.Name,
//                labelText: null,
//                htmlAttributes: null);

//            if (tagBuilder != null)
//            {
//                output.MergeAttributes(tagBuilder);

//                // Do not update the content if another tag helper targeting this element has already done so.
//                if (!output.IsContentModified)
//                {
//                    // We check for whitespace to detect scenarios such as:
//                    // <label for="Name">
//                    // </label>
//                    var childContent = await output.GetChildContentAsync();
//                    if (childContent.IsEmptyOrWhiteSpace)
//                    {
//                        // Provide default label text (if any) since there was nothing useful in the Razor source.
//                        if (tagBuilder.HasInnerHtml)
//                        {
//                            output.Content.SetHtmlContent(tagBuilder.InnerHtml);
//                        }
//                    }
//                    else
//                    {
//                        output.Content.SetHtmlContent(childContent);
//                    }
//                }
//            }
//        }
//    }
//}
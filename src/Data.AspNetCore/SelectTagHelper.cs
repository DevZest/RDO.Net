using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.TagHelpers.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.AspNetCore
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;select&gt; elements with <c>asp-for</c> and/or
    /// <c>asp-items</c> attribute(s).
    /// </summary>
    [HtmlTargetElement("select", Attributes = ItemsAttributeName)]
    public class SelectTagHelper : DataSetTagHelperBase
    {
        private const string ItemsAttributeName = "dataset-items";
        private bool _allowMultiple;
        private ICollection<string> _currentValues;

        /// <summary>
        /// Creates a new <see cref="SelectTagHelper"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
        public SelectTagHelper(IDataSetHtmlGenerator generator)
            : base(generator)
        {
        }

        /// <summary>
        /// A collection of <see cref="SelectListItem"/> objects used to populate the &lt;select&gt; element with
        /// &lt;optgroup&gt; and &lt;option&gt; elements.
        /// </summary>
        [HtmlAttributeName(ItemsAttributeName)]
        public IEnumerable<SelectListItem> Items { get; set; }

        /// <inheritdoc />
        public override void Init(TagHelperContext context)
        {
            base.Init(context);

            // Base allowMultiple on the instance or declared type of the expression to avoid a
            // "SelectExpressionNotEnumerable" InvalidOperationException during generation.
            // Metadata.IsEnumerableType is similar but does not take runtime type into account.
            var dataType = Column.DataType;
            _allowMultiple = typeof(string) != dataType && typeof(IEnumerable).IsAssignableFrom(dataType);
            _currentValues = Generator.GetCurrentValues(ViewContext, FullHtmlFieldName, Column, DataValue, _allowMultiple);

            // Whether or not (not being highly unlikely) we generate anything, could update contained <option/>
            // elements. Provide selected values for <option/> tag helpers.
            var currentValues = _currentValues == null ? null : new CurrentValues(_currentValues);
            context.Items[typeof(SelectTagHelper)] = currentValues;
        }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="For"/> is <c>null</c>.</remarks>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            // Ensure GenerateSelect() _never_ looks anything up in ViewData.
            var items = Items ?? Enumerable.Empty<SelectListItem>();

            var tagBuilder = Generator.GenerateSelect(
                ViewContext,
                FullHtmlFieldName,
                Column,
                optionLabel: null,
                selectList: items,
                currentValues: _currentValues,
                allowMultiple: _allowMultiple,
                htmlAttributes: null);

            if (tagBuilder != null)
            {
                output.MergeAttributes(tagBuilder);
                if (tagBuilder.HasInnerHtml)
                {
                    output.PostContent.AppendHtml(tagBuilder.InnerHtml);
                }
            }
        }
    }
}

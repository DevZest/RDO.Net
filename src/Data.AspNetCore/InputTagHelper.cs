using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DevZest.Data.AspNetCore
{
    [HtmlTargetElement("input", Attributes = DataSetAttributes, TagStructure = TagStructure.WithoutEndTag)]
    public class InputTagHelper : DataSetTagHelperBase
    {
        private const string FormatAttributeName = "dataset-format";

        // Mapping from datatype names and data annotation hints to values for the <input/> element's "type" attribute.
        private static readonly Dictionary<string, string> _defaultInputTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "HiddenInput", InputType.Hidden.ToString().ToLowerInvariant() },
                { "Password", InputType.Password.ToString().ToLowerInvariant() },
                { "Text", InputType.Text.ToString().ToLowerInvariant() },
                { "PhoneNumber", "tel" },
                { "Url", "url" },
                { "EmailAddress", "email" },
                { "Date", "date" },
                { "DateTime", "datetime-local" },
                { "DateTime-local", "datetime-local" },
                { nameof(DateTimeOffset), "text" },
                { "Time", "time" },
                { "Week", "week" },
                { "Month", "month" },
                { nameof(Byte), "number" },
                { nameof(SByte), "number" },
                { nameof(Int16), "number" },
                { nameof(UInt16), "number" },
                { nameof(Int32), "number" },
                { nameof(UInt32), "number" },
                { nameof(Int64), "number" },
                { nameof(UInt64), "number" },
                { nameof(Single), InputType.Text.ToString().ToLowerInvariant() },
                { nameof(Double), InputType.Text.ToString().ToLowerInvariant() },
                { nameof(Boolean), InputType.CheckBox.ToString().ToLowerInvariant() },
                { nameof(Decimal), InputType.Text.ToString().ToLowerInvariant() },
                { nameof(String), InputType.Text.ToString().ToLowerInvariant() },
                { nameof(IFormFile), "file" },
                { TemplateRenderer.IEnumerableOfIFormFileName, "file" },
            };

        // Mapping from <input/> element's type to RFC 3339 date and time formats.
        private static readonly Dictionary<string, string> _rfc3339Formats =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "date", "{0:yyyy-MM-dd}" },
                { "datetime", @"{0:yyyy-MM-ddTHH\:mm\:ss.fffK}" },
                { "datetime-local", @"{0:yyyy-MM-ddTHH\:mm\:ss.fff}" },
                { "time", @"{0:HH\:mm\:ss.fff}" },
            };

        public InputTagHelper(IDataSetHtmlGenerator generator)
            : base(generator)
        {
        }

        /// <summary>
        /// The type of the &lt;input&gt; element.
        /// </summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases. Also used to determine the <see cref="IHtmlGenerator"/>
        /// helper to call and the default <see cref="Format"/> value. A default <see cref="Format"/> is not calculated
        /// if the provided (see <see cref="InputTypeName"/>) or calculated "type" attribute value is <c>checkbox</c>,
        /// <c>hidden</c>, <c>password</c>, or <c>radio</c>.
        /// </remarks>
        [HtmlAttributeName("type")]
        public string InputTypeName { get; set; }

        /// <summary>
        /// The format string used to format the DataValue result. Sets the generated "value" attribute to that formatted string.
        /// </summary>
        /// <remarks>
        /// Not used if the provided (see <see cref="InputTypeName"/>) or calculated "type" attribute value is
        /// <c>checkbox</c>, <c>password</c>, or <c>radio</c>. That is, <see cref="Format"/> is used when calling
        /// <see cref="IHtmlGenerator.GenerateTextBox"/>.
        /// </remarks>
        [HtmlAttributeName(FormatAttributeName)]
        public string Format { get; set; }

        /// <summary>
        /// The value of the &lt;input&gt; element.
        /// </summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases. Also used to determine the generated "checked" attribute
        /// if <see cref="InputTypeName"/> is "radio". Must not be <c>null</c> in that case.
        /// </remarks>
        public string Value { get; set; }

        /// <summary>
        /// Gets an &lt;input&gt; element's "type" attribute value based on the given <paramref name="modelExplorer"/>
        /// or <see cref="InputType"/>.
        /// </summary>
        /// <param name="modelExplorer">The <see cref="ModelExplorer"/> to use.</param>
        /// <param name="inputTypeHint">When this method returns, contains the string, often the name of a
        /// <see cref="ModelMetadata.ModelType"/> base class, used to determine this method's return value.</param>
        /// <returns>An &lt;input&gt; element's "type" attribute value.</returns>
        protected string GetInputType(Column column, out string inputTypeHint)
        {
            foreach (var hint in GetInputTypeHints(column))
            {
                if (_defaultInputTypes.TryGetValue(hint, out var inputType))
                {
                    inputTypeHint = hint;
                    return inputType;
                }
            }

            inputTypeHint = InputType.Text.ToString().ToLowerInvariant();
            return inputTypeHint;
        }

        // A variant of TemplateRenderer.GetViewNames(). Main change relates to bool? handling.
        private static IEnumerable<string> GetInputTypeHints(Column column)
        {
            if (column.LogicalDataType != LogicalDataType.Custom)
                yield return column.LogicalDataType.ToString();

            // In most cases, we don't want to search for Nullable<T>. We want to search for T, which should handle
            // both T and Nullable<T>. However we special-case bool? to avoid turning an <input/> into a <select/>.
            var dataType = column.DataType;
            if (typeof(bool?) != dataType)
                dataType = dataType.UnderlyingOrDataType();

            foreach (var typeName in column.GetTypeNames(dataType))
            {
                yield return typeName;
            }
        }

        // Get a fall-back format based on the metadata.
        private string GetFormat(string inputTypeHint, string inputType)
        {
            string format;
            if (string.Equals("month", inputType, StringComparison.OrdinalIgnoreCase))
            {
                // "month" is a new HTML5 input type that only will be rendered in Rfc3339 mode
                format = "{0:yyyy-MM}";
            }
            else if (string.Equals("decimal", inputTypeHint, StringComparison.OrdinalIgnoreCase) &&
                string.Equals("text", inputType, StringComparison.Ordinal))
            {
                // Decimal data is edited using an <input type="text"/> element, with a reasonable format.
                // EditFormatString has precedence over this fall-back format.
                format = "{0:0.00}";
            }
            else if (ViewContext.Html5DateRenderingMode == Html5DateRenderingMode.Rfc3339 &&
                (typeof(DateTime) == Column.UnderlyingOrDataType() ||
                 typeof(DateTimeOffset) == Column.UnderlyingOrDataType()))
            {
                // Rfc3339 mode _may_ override EditFormatString in a limited number of cases. Happens only when
                // EditFormatString has a default format i.e. came from a [DataType] attribute.
                if (string.Equals("text", inputType) &&
                    string.Equals(nameof(DateTimeOffset), inputTypeHint, StringComparison.OrdinalIgnoreCase))
                {
                    // Auto-select a format that round-trips Offset and sub-Second values in a DateTimeOffset. Not
                    // done if user chose the "text" type in .cshtml file or with data annotations i.e. when
                    // inputTypeHint==null or "text".
                    format = _rfc3339Formats["datetime"];
                }
                else if (_rfc3339Formats.TryGetValue(inputType, out var rfc3339Format))
                {
                    format = rfc3339Format;
                }
                else
                {
                    // Otherwise use default EditFormatString.
                    format = null;
                }
            }
            else
            {
                // Otherwise use EditFormatString, if any.
                format = null;
            }

            return format;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Pass through attributes that are also well-known HTML attributes. Must be done prior to any copying
            // from a TagBuilder.
            if (InputTypeName != null)
                output.CopyHtmlAttribute("type", context);

            if (Value != null)
                output.CopyHtmlAttribute(nameof(Value), context);

            string inputType;
            string inputTypeHint;
            if (string.IsNullOrEmpty(InputTypeName))
            {
                // Note GetInputType never returns null.
                inputType = GetInputType(Column, out inputTypeHint);
            }
            else
            {
                inputType = InputTypeName.ToLowerInvariant();
                inputTypeHint = null;
            }

            // inputType may be more specific than default the generator chooses below.
            if (!output.Attributes.ContainsName("type"))
                output.Attributes.SetAttribute("type", inputType);

            TagBuilder tagBuilder;
            switch (inputType)
            {
                case "hidden":
                    tagBuilder = GenerateHidden();
                    break;

                case "checkbox":
                    tagBuilder = GenerateCheckBox(output);
                    break;

                case "password":
                    tagBuilder = Generator.GeneratePassword(ViewContext, FullHtmlFieldName, Column, value: null, htmlAttributes: null);
                    break;

                case "radio":
                    tagBuilder = GenerateRadio();
                    break;

                default:
                    tagBuilder = GenerateTextBox(inputTypeHint, inputType);
                    break;
            }

            if (tagBuilder != null)
            {
                // This TagBuilder contains the one <input/> element of interest.
                output.MergeAttributes(tagBuilder);
                if (tagBuilder.HasInnerHtml)
                {
                    // Since this is not the "checkbox" special-case, no guarantee that output is a self-closing
                    // element. A later tag helper targeting this element may change output.TagMode.
                    output.Content.AppendHtml(tagBuilder.InnerHtml);
                }
            }

            return Task.CompletedTask;
        }

        // Imitate Generator.GenerateHidden() using Generator.GenerateTextBox(). This adds support for asp-format that
        // is not available in Generator.GenerateHidden().
        private TagBuilder GenerateHidden()
        {
            var value = DataValue;
            if (value is byte[] byteArrayValue)
                value = Convert.ToBase64String(byteArrayValue);

            var htmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // In DefaultHtmlGenerator(), GenerateTextBox() calls GenerateInput() _almost_ identically to how
            // GenerateHidden() does and the main switch inside GenerateInput() handles InputType.Text and
            // InputType.Hidden identically. No behavior differences at all when a type HTML attribute already exists.
            htmlAttributes["type"] = "hidden";

            return Generator.GenerateTextBox(ViewContext, FullHtmlFieldName, Column, value, Format, htmlAttributes);
        }

        private TagBuilder GenerateCheckBox(TagHelperOutput output)
        {
            if (Column.DataType == typeof(string))
            {
                if (DataValue != null && !bool.TryParse(DataValue.ToString(), out _))
                    throw new InvalidOperationException(DiagnosticMessages.FormatInputTagHelper_InvalidStringResult(DataValue, FullHtmlFieldName, typeof(bool)));
            }
            else if (Column.DataType != typeof(bool) && Column.DataType != typeof(bool?))
            {
                throw new InvalidOperationException(DiagnosticMessages.FormatInputTagHelper_InvalidExpressionResult(
                    FullHtmlFieldName, Column.DataType, "<input>", typeof(bool), typeof(string), "type", "checkbox"));
            }

            // hiddenForCheckboxTag always rendered after the returned element
            var hiddenForCheckboxTag = Generator.GenerateHiddenForCheckbox(ViewContext, FullHtmlFieldName);
            if (hiddenForCheckboxTag != null)
            {
                var renderingMode = output.TagMode == TagMode.SelfClosing ? TagRenderMode.SelfClosing : TagRenderMode.StartTag;
                hiddenForCheckboxTag.TagRenderMode = renderingMode;

                if (ViewContext.FormContext.CanRenderAtEndOfForm)
                    ViewContext.FormContext.EndOfFormContent.Add(hiddenForCheckboxTag);
                else
                    output.PostElement.AppendHtml(hiddenForCheckboxTag);
            }

            bool? isChecked = null;
            if (DataValue != null && bool.TryParse(DataValue.ToString(), out var modelChecked))
                isChecked = modelChecked;
            return Generator.GenerateCheckBox(ViewContext, FullHtmlFieldName, Column, isChecked, htmlAttributes: null);
        }

        private TagBuilder GenerateRadio()
        {
            // Note empty string is allowed.
            if (Value == null)
            {
                throw new InvalidOperationException(DiagnosticMessages.FormatInputTagHelper_ValueRequired(
                    nameof(Value).ToLowerInvariant(),
                    "<input>",
                    "type",
                    "radio"));
            }

            var isChecked = DataValue != null && string.Equals(DataValue.ToString(), Value, StringComparison.OrdinalIgnoreCase);
            return Generator.GenerateRadioButton(ViewContext, FullHtmlFieldName, Column, Value, isChecked, htmlAttributes: null);
        }

        private TagBuilder GenerateTextBox(string inputTypeHint, string inputType)
        {
            var format = Format;
            if (string.IsNullOrEmpty(format))
            {
                if (string.Equals("week", inputType, StringComparison.OrdinalIgnoreCase) &&
                    (DataValue is DateTime || DataValue is DateTimeOffset))
                    format = Column.GetFormattedWeek(DataValue);
                else
                    format = GetFormat(inputTypeHint, inputType);
            }

            var htmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            htmlAttributes["type"] = inputType;
            if (string.Equals(inputType, "file") &&
                string.Equals(
                    inputTypeHint,
                    TemplateRenderer.IEnumerableOfIFormFileName,
                    StringComparison.OrdinalIgnoreCase))
            {
                htmlAttributes["multiple"] = "multiple";
            }

            return Generator.GenerateTextBox(
                ViewContext,
                FullHtmlFieldName,
                Column,
                DataValue,
                format,
                htmlAttributes);
        }
    }
}

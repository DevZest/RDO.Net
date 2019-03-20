using DevZest.Data.Annotations;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DefaultDataSetHtmlGenerator : IDataSetHtmlGenerator
    {
        // See: (http://www.w3.org/TR/html5/forms.html#the-input-element)
        private static readonly string[] _placeholderInputTypes =
            new[] { "text", "search", "url", "tel", "email", "password", "number" };

        // See: (http://www.w3.org/TR/html5/sec-forms.html#apply)
        private static readonly string[] _maxLengthInputTypes =
            new[] { "text", "search", "url", "tel", "email", "password" };

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

        private static string GetInputTypeString(InputType inputType)
        {
            switch (inputType)
            {
                case InputType.CheckBox:
                    return "checkbox";
                case InputType.Hidden:
                    return "hidden";
                case InputType.Password:
                    return "password";
                case InputType.Radio:
                    return "radio";
                case InputType.Text:
                    return "text";
                default:
                    return "text";
            }
        }

        public DefaultDataSetHtmlGenerator(IOptions<MvcViewOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
                throw new ArgumentNullException(nameof(optionsAccessor));

            // Underscores are fine characters in id's.
            IdAttributeDotReplacement = optionsAccessor.Value.HtmlHelperOptions.IdAttributeDotReplacement;

            AllowRenderingMaxLengthAttribute = optionsAccessor.Value.AllowRenderingMaxLengthAttribute;
        }

        /// <inheritdoc />
        public string IdAttributeDotReplacement { get; }

        /// <summary> 
        /// Gets or sets a value that indicates whether the maxlength attribute should be rendered for compatible HTML input elements, 
        /// when they're bound to models marked with either 
        /// <see cref="StringLengthAttribute"/> or <see cref="MaxLengthAttribute"/> attributes. 
        /// </summary>
        /// <remarks>If both attributes are specified, the one with the smaller value will be used for the rendered `maxlength` attribute.</remarks>
        protected bool AllowRenderingMaxLengthAttribute { get; }

        /// <summary>
        /// Adds a placeholder attribute to the <paramref name="tagBuilder" />.
        /// </summary>
        protected virtual void AddPlaceholderAttribute(TagBuilder tagBuilder, Column column)
        {
            var placeholder = column.DisplayPrompt;
            if (!string.IsNullOrEmpty(placeholder))
            {
                tagBuilder.MergeAttribute("placeholder", placeholder);
            }
        }

        /// <summary>
        /// Adds a maxlength attribute to the <paramref name="tagBuilder" />.
        /// </summary>
        /// <param name="tagBuilder">A <see cref="TagBuilder"/> instance.</param>
        protected virtual void AddMaxLengthAttribute(TagBuilder tagBuilder, Column column)
        {
            int? maxLengthValue = null;
            foreach (var validator in column.GetParent().Validators)
            {
                var attribute = validator.Attribute;
                if (attribute is StringLengthAttribute stringLengthAttribute && (!maxLengthValue.HasValue || maxLengthValue.Value > stringLengthAttribute.MaximumLength))
                    maxLengthValue = stringLengthAttribute.MaximumLength;
                //else if (attribute is MaxLengthAttribute maxLengthAttribute && (!maxLengthValue.HasValue || maxLengthValue.Value > maxLengthAttribute.Length))
                //    maxLengthValue = maxLengthAttribute.Length;
            }

            if (maxLengthValue.HasValue)
                tagBuilder.MergeAttribute("maxlength", maxLengthValue.Value.ToString());
        }

        /// <summary>
        /// Adds validation attributes to the <paramref name="tagBuilder" /> if client validation
        /// is enabled.
        /// </summary>
        /// <param name="viewContext">A <see cref="ViewContext"/> instance for the current scope.</param>
        /// <param name="tagBuilder">A <see cref="TagBuilder"/> instance.</param>
        /// <param name="modelExplorer">The <see cref="ModelExplorer"/> for the <paramref name="expression"/>.</param>
        /// <param name="expression">Expression name, relative to the current model.</param>
        protected virtual void AddValidationAttributes(ViewContext viewContext, TagBuilder tagBuilder, Column column)
        {
            //_validationAttributeProvider.AddAndTrackValidationAttributes(
            //    viewContext,
            //    modelExplorer,
            //    expression,
            //    tagBuilder.Attributes);
            throw new NotImplementedException();
        }

        private string FormatValue(object value, string format)
        {
            return ViewDataDictionary.FormatValue(value, format);
        }

        private static object GetModelStateValue(ViewContext viewContext, string key, Type destinationType)
        {
            if (viewContext.ViewData.ModelState.TryGetValue(key, out var entry) && entry.RawValue != null)
                return ModelBindingHelper.ConvertTo(entry.RawValue, destinationType, culture: null);

            return null;
        }

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

        /// <inheritdoc />
        public virtual TagBuilder GenerateTextBox(ViewContext viewContext,
            string fullHtmlFieldName,
            Column column,
            object value,
            string format,
            object htmlAttributes)
        {
            if (viewContext == null)
            {
                throw new ArgumentNullException(nameof(viewContext));
            }

            var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);
            return GenerateInput(
                viewContext,
                InputType.Text,
                fullHtmlFieldName,
                column,
                value,
                isChecked: false,
                setId: true,
                isExplicitValue: true,
                format: format,
                htmlAttributes: htmlAttributeDictionary);
        }

        protected virtual TagBuilder GenerateInput(ViewContext viewContext, InputType inputType,
            string fullHtmlFieldName, Column column,
            object value,
            bool isChecked,
            bool setId,
            bool isExplicitValue,
            string format,
            IDictionary<string, object> htmlAttributes)
        {
            if (viewContext == null)
                throw new ArgumentNullException(nameof(viewContext));

            if (string.IsNullOrEmpty(fullHtmlFieldName))
                throw new ArgumentNullException(nameof(fullHtmlFieldName));

            var inputTypeString = GetInputTypeString(inputType);
            var tagBuilder = new TagBuilder("input")
            {
                TagRenderMode = TagRenderMode.SelfClosing,
            };

            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("type", inputTypeString);
            tagBuilder.MergeAttribute("name", fullHtmlFieldName, replaceExisting: true);

            var suppliedTypeString = tagBuilder.Attributes["type"];
            if (_placeholderInputTypes.Contains(suppliedTypeString))
                AddPlaceholderAttribute(tagBuilder, column);

            if (AllowRenderingMaxLengthAttribute && _maxLengthInputTypes.Contains(suppliedTypeString))
                AddMaxLengthAttribute(tagBuilder, column);

            var valueParameter = FormatValue(value, format);
            var usedModelState = false;
            switch (inputType)
            {
                case InputType.CheckBox:
                    var modelStateWasChecked = GetModelStateValue(viewContext, fullHtmlFieldName, typeof(bool)) as bool?;
                    if (modelStateWasChecked.HasValue)
                    {
                        isChecked = modelStateWasChecked.Value;
                        usedModelState = true;
                    }

                    goto case InputType.Radio;

                case InputType.Radio:
                    if (!usedModelState)
                    {
                        if (GetModelStateValue(viewContext, fullHtmlFieldName, typeof(string)) is string modelStateValue)
                        {
                            isChecked = string.Equals(modelStateValue, valueParameter, StringComparison.Ordinal);
                            usedModelState = true;
                        }
                    }

                    if (isChecked)
                        tagBuilder.MergeAttribute("checked", "checked");

                    tagBuilder.MergeAttribute("value", valueParameter, isExplicitValue);
                    break;

                case InputType.Password:
                    if (value != null)
                        tagBuilder.MergeAttribute("value", valueParameter, isExplicitValue);

                    break;

                case InputType.Text:
                default:
                    var attributeValue = (string)GetModelStateValue(viewContext, fullHtmlFieldName, typeof(string));
                    if (attributeValue == null)
                        attributeValue = valueParameter;

                    var addValue = true;
                    object typeAttributeValue;
                    if (htmlAttributes != null && htmlAttributes.TryGetValue("type", out typeAttributeValue))
                    {
                        var typeAttributeString = typeAttributeValue.ToString();
                        if (string.Equals(typeAttributeString, "file", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(typeAttributeString, "image", StringComparison.OrdinalIgnoreCase))
                            addValue = false;  // 'value' attribute is not needed for 'file' and 'image' input types.
                    }

                    if (addValue)
                        tagBuilder.MergeAttribute("value", attributeValue, replaceExisting: isExplicitValue);

                    break;
            }

            if (setId)
                NameAndIdProvider.GenerateId(viewContext, tagBuilder, fullHtmlFieldName, IdAttributeDotReplacement);

            // If there are any errors for a named field, we add the CSS attribute.
            if (viewContext.ViewData.ModelState.TryGetValue(fullHtmlFieldName, out var entry) && entry.Errors.Count > 0)
                tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);

            //AddValidationAttributes(viewContext, tagBuilder, column);

            return tagBuilder;
        }

        /// <inheritdoc />
        public virtual TagBuilder GenerateHiddenForCheckbox(string fullHtmlFieldName)
        {
            var tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttribute("type", GetInputTypeString(InputType.Hidden));
            tagBuilder.MergeAttribute("value", "false");
            tagBuilder.TagRenderMode = TagRenderMode.SelfClosing;

            if (!string.IsNullOrEmpty(fullHtmlFieldName))
                tagBuilder.MergeAttribute("name", fullHtmlFieldName);

            return tagBuilder;
        }

        /// <inheritdoc />
        public virtual TagBuilder GenerateCheckBox(ViewContext viewContext, string fullFieldName, Column column, bool? isChecked, object htmlAttributes)
        {
            if (viewContext == null)
                throw new ArgumentNullException(nameof(viewContext));

            var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);
            if (isChecked.HasValue && htmlAttributeDictionary != null)
            {
                // Explicit isChecked value must override "checked" in dictionary.
                htmlAttributeDictionary.Remove("checked");
            }

            // Use ViewData only in CheckBox case (metadata null) and when the user didn't pass an isChecked value.
            return GenerateInput(
                viewContext,
                InputType.CheckBox,
                fullFieldName,
                column,
                value: "true",
                isChecked: isChecked ?? false,
                setId: true,
                isExplicitValue: false,
                format: null,
                htmlAttributes: htmlAttributeDictionary);
        }

        /// <inheritdoc />
        public virtual TagBuilder GeneratePassword(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, object htmlAttributes)
        {
            if (viewContext == null)
                throw new ArgumentNullException(nameof(viewContext));

            var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);
            return GenerateInput(
                viewContext,
                InputType.Password,
                fullHtmlFieldName,
                column,
                value,
                isChecked: false,
                setId: true,
                isExplicitValue: true,
                format: null,
                htmlAttributes: htmlAttributeDictionary);
        }

        public virtual TagBuilder GenerateRadioButton(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, bool? isChecked, object htmlAttributes)
        {
            if (viewContext == null)
                throw new ArgumentNullException(nameof(viewContext));

            var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);

            if (isChecked.HasValue && htmlAttributeDictionary != null)
            {
                // Explicit isChecked value must override "checked" in dictionary.
                htmlAttributeDictionary.Remove("checked");
            }

            return GenerateInput(
                viewContext,
                InputType.Radio,
                fullHtmlFieldName,
                column,
                value,
                isChecked: isChecked ?? false,
                setId: true,
                isExplicitValue: true,
                format: null,
                htmlAttributes: htmlAttributeDictionary);
        }
    }
}

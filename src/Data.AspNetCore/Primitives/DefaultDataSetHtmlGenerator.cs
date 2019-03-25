using DevZest.Data.Annotations;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

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

        private readonly DataSetValidationHtmlAttributeProvider _validationAttributeProvider;

        public DefaultDataSetHtmlGenerator(IOptions<MvcViewOptions> optionsAccessor, DataSetValidationHtmlAttributeProvider validationAttributeProvider)
        {
            if (optionsAccessor == null)
                throw new ArgumentNullException(nameof(optionsAccessor));

            _validationAttributeProvider = validationAttributeProvider ?? throw new ArgumentNullException(nameof(validationAttributeProvider));

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
                tagBuilder.MergeAttribute("placeholder", placeholder);
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
                else if (attribute is MaxLengthAttribute maxLengthAttribute && (!maxLengthValue.HasValue || maxLengthValue.Value > maxLengthAttribute.Length))
                    maxLengthValue = maxLengthAttribute.Length;
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
        protected virtual void AddValidationAttributes(ViewContext viewContext, TagBuilder tagBuilder, string fullHtmlFieldName, Column column)
        {
            _validationAttributeProvider.AddAndTrackValidationAttributes(viewContext, fullHtmlFieldName, column, tagBuilder.Attributes);
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

        private static bool IsFullNameValid(string fullName, IDictionary<string, object> htmlAttributeDictionary, string fallbackAttributeName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                // fullName==null is normally an error because name="" is not valid in HTML 5.
                if (htmlAttributeDictionary == null)
                    return false;

                // Check if user has provided an explicit name attribute.
                // Generalized a bit because other attributes e.g. data-valmsg-for refer to element names.
                htmlAttributeDictionary.TryGetValue(fallbackAttributeName, out var attributeObject);
                var attributeString = Convert.ToString(attributeObject, CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(attributeString))
                    return false;
            }

            return true;
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
                    // The following lines causes problem when valueParameter is a formatted datetime
                    // It can be different from the value get from ModelState. To reproduce, remove
                    // the [SqlDate] attribute of Movie.ReleaseDate property, then Movies/Edit page
                    // will not display ReleaseDate correctly.
                    // Not sure the side effect of bypassing GetModelStateValue though.
                    /*
                    var attributeValue = (string)GetModelStateValue(viewContext, fullHtmlFieldName, typeof(string));
                    if (attributeValue == null)
                        attributeValue = valueParameter;
                    */
                    var attributeValue = valueParameter;

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

            AddValidationAttributes(viewContext, tagBuilder, fullHtmlFieldName, column);

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

        /// <inheritdoc />
        public virtual TagBuilder GenerateValidationMessage(ViewContext viewContext, string fullHtmlFieldName, Column column, string message, string tag, object htmlAttributes)
        {
            if (viewContext == null)
                throw new ArgumentNullException(nameof(viewContext));

            var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);
            if (!IsFullNameValid(fullHtmlFieldName, htmlAttributeDictionary, fallbackAttributeName: "data-valmsg-for"))
                throw new ArgumentException(DiagnosticMessages.DataSetHtmlGenerator_FieldNameCannotBeNullOrEmpty, nameof(fullHtmlFieldName));

            var formContext = viewContext.ClientValidationEnabled ? viewContext.FormContext : null;
            if (!viewContext.ViewData.ModelState.ContainsKey(fullHtmlFieldName) && formContext == null)
                return null;

            var tryGetModelStateResult = viewContext.ViewData.ModelState.TryGetValue(fullHtmlFieldName, out var entry);
            var modelErrors = tryGetModelStateResult ? entry.Errors : null;

            ModelError modelError = null;
            if (modelErrors != null && modelErrors.Count != 0)
                modelError = modelErrors.FirstOrDefault(m => !string.IsNullOrEmpty(m.ErrorMessage)) ?? modelErrors[0];

            if (modelError == null && formContext == null)
                return null;

            // Even if there are no model errors, we generate the span and add the validation message
            // if formContext is not null.
            if (string.IsNullOrEmpty(tag))
                tag = viewContext.ValidationMessageElement;

            var tagBuilder = new TagBuilder(tag);
            tagBuilder.MergeAttributes(htmlAttributeDictionary);

            // Only the style of the span is changed according to the errors if message is null or empty.
            // Otherwise the content and style is handled by the client-side validation.
            var className = (modelError != null) ?
                HtmlHelper.ValidationMessageCssClassName :
                HtmlHelper.ValidationMessageValidCssClassName;
            tagBuilder.AddCssClass(className);

            if (!string.IsNullOrEmpty(message))
                tagBuilder.InnerHtml.SetContent(message);
            else if (modelError != null)
                tagBuilder.InnerHtml.SetContent(modelError.ErrorMessage);

            if (formContext != null)
            {
                if (!string.IsNullOrEmpty(fullHtmlFieldName))
                    tagBuilder.MergeAttribute("data-valmsg-for", fullHtmlFieldName);

                var replaceValidationMessageContents = string.IsNullOrEmpty(message);
                tagBuilder.MergeAttribute("data-valmsg-replace",
                    replaceValidationMessageContents.ToString().ToLowerInvariant());
            }

            return tagBuilder;
        }

        /// <inheritdoc />
        public virtual TagBuilder GenerateHidden(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, object htmlAttributes)
        {
            if (viewContext == null)
                throw new ArgumentNullException(nameof(viewContext));

            // Special-case opaque values and arbitrary binary data.
            if (value is byte[] byteArrayValue)
                value = Convert.ToBase64String(byteArrayValue);

            var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);
            return GenerateInput(viewContext, InputType.Hidden, fullHtmlFieldName, column, value,
                isChecked: false, setId: true, isExplicitValue: true, format: null, htmlAttributes: htmlAttributeDictionary);
        }

        /// <inheritdoc />
        public TagBuilder GenerateSelect(
            ViewContext viewContext,
            string fullHtmlFieldName,
            Column column,
            object dataValue,
            string optionLabel,
            IEnumerable<SelectListItem> selectList,
            bool allowMultiple,
            object htmlAttributes)
        {
            if (viewContext == null)
                throw new ArgumentNullException(nameof(viewContext));

            var currentValues = GetCurrentValues(viewContext, fullHtmlFieldName, column, dataValue, allowMultiple);
            return GenerateSelect(
                viewContext,
                fullHtmlFieldName,
                column,
                optionLabel,
                selectList,
                currentValues,
                allowMultiple,
                htmlAttributes);
        }

        /// <inheritdoc />
        public virtual TagBuilder GenerateSelect(
            ViewContext viewContext,
            string fullHtmlFieldName,
            Column column,
            string optionLabel,
            IEnumerable<SelectListItem> selectList,
            ICollection<string> currentValues,
            bool allowMultiple,
            object htmlAttributes)
        {
            if (viewContext == null)
                throw new ArgumentNullException(nameof(viewContext));

            if (string.IsNullOrEmpty(fullHtmlFieldName))
                throw new ArgumentNullException(nameof(fullHtmlFieldName));

            if (selectList == null)
                throw new ArgumentNullException(nameof(selectList));

            var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);

            // Convert each ListItem to an <option> tag and wrap them with <optgroup> if requested.
            var listItemBuilder = GenerateGroupsAndOptions(optionLabel, selectList, currentValues);

            var tagBuilder = new TagBuilder("select");
            tagBuilder.InnerHtml.SetHtmlContent(listItemBuilder);
            tagBuilder.MergeAttributes(htmlAttributeDictionary);
            NameAndIdProvider.GenerateId(viewContext, tagBuilder, fullHtmlFieldName, IdAttributeDotReplacement);
            tagBuilder.MergeAttribute("name", fullHtmlFieldName, replaceExisting: true);

            if (allowMultiple)
                tagBuilder.MergeAttribute("multiple", "multiple");

            // If there are any errors for a named field, we add the css attribute.
            if (viewContext.ViewData.ModelState.TryGetValue(fullHtmlFieldName, out var entry))
            {
                if (entry.Errors.Count > 0)
                    tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
            }

            AddValidationAttributes(viewContext, tagBuilder, fullHtmlFieldName, column);

            return tagBuilder;
        }

        private IHtmlContent GenerateGroupsAndOptions(string optionLabel, IEnumerable<SelectListItem> selectList, ICollection<string> currentValues)
        {
            var itemsList = selectList as IList<SelectListItem>;
            if (itemsList == null)
                itemsList = selectList.ToList();

            var count = itemsList.Count;
            if (optionLabel != null)
                count++;

            // Short-circuit work below if there's nothing to add.
            if (count == 0)
                return HtmlString.Empty;

            var listItemBuilder = new HtmlContentBuilder(count);

            // Make optionLabel the first item that gets rendered.
            if (optionLabel != null)
            {
                listItemBuilder.AppendLine(GenerateOption(
                    new SelectListItem()
                    {
                        Text = optionLabel,
                        Value = string.Empty,
                        Selected = false,
                    },
                    currentValues: null));
            }

            // Group items in the SelectList if requested.
            // The worst case complexity of this algorithm is O(number of groups*n).
            // If there aren't any groups, it is O(n) where n is number of items in the list.
            var optionGenerated = new bool[itemsList.Count];
            for (var i = 0; i < itemsList.Count; i++)
            {
                if (!optionGenerated[i])
                {
                    var item = itemsList[i];
                    var optGroup = item.Group;
                    if (optGroup != null)
                    {
                        var groupBuilder = new TagBuilder("optgroup");
                        if (optGroup.Name != null)
                        {
                            groupBuilder.MergeAttribute("label", optGroup.Name);
                        }

                        if (optGroup.Disabled)
                        {
                            groupBuilder.MergeAttribute("disabled", "disabled");
                        }

                        groupBuilder.InnerHtml.AppendLine();

                        for (var j = i; j < itemsList.Count; j++)
                        {
                            var groupItem = itemsList[j];

                            if (!optionGenerated[j] &&
                                object.ReferenceEquals(optGroup, groupItem.Group))
                            {
                                groupBuilder.InnerHtml.AppendLine(GenerateOption(groupItem, currentValues));
                                optionGenerated[j] = true;
                            }
                        }

                        listItemBuilder.AppendLine(groupBuilder);
                    }
                    else
                    {
                        listItemBuilder.AppendLine(GenerateOption(item, currentValues));
                        optionGenerated[i] = true;
                    }
                }
            }

            return listItemBuilder;
        }

        private IHtmlContent GenerateOption(SelectListItem item, ICollection<string> currentValues)
        {
            var selected = item.Selected;
            if (currentValues != null)
            {
                var value = item.Value ?? item.Text;
                selected = currentValues.Contains(value);
            }

            var tagBuilder = GenerateOption(item, item.Text, selected);
            return tagBuilder;
        }

        private static TagBuilder GenerateOption(SelectListItem item, string text, bool selected)
        {
            var tagBuilder = new TagBuilder("option");
            tagBuilder.InnerHtml.SetContent(text);

            if (item.Value != null)
                tagBuilder.Attributes["value"] = item.Value;

            if (selected)
                tagBuilder.Attributes["selected"] = "selected";

            if (item.Disabled)
                tagBuilder.Attributes["disabled"] = "disabled";

            return tagBuilder;
        }

        /// <inheritdoc />
        public virtual ICollection<string> GetCurrentValues(ViewContext viewContext, string fullHtmlFieldName, Column column, object rawValue, bool allowMultiple)
        {
            if (viewContext == null)
                throw new ArgumentNullException(nameof(viewContext));

            if (rawValue == null)
                return null;

            // Convert raw value to a collection.
            IEnumerable rawValues;
            if (allowMultiple)
            {
                rawValues = rawValue as IEnumerable;
                if (rawValues == null || rawValues is string)
                    throw new ArgumentNullException(DiagnosticMessages.DataSetHtmlGenerator_RawValueNotEnumerable, nameof(rawValue));
            }
            else
                rawValues = new[] { rawValue };

            var dataType = column.DataType;

            if (allowMultiple)
            {
                var enumerableElementType = dataType.GetEnumerableElementType();
                if (enumerableElementType != null)
                    dataType = enumerableElementType;
            }

            var enumNames = dataType.GetEnumNamesAndValues();
            var isTargetEnum = dataType.IsEnum();

            // Logic below assumes isTargetEnum and enumNames are consistent. Confirm that expectation is met.
            Debug.Assert(isTargetEnum ^ enumNames == null);

            var innerType = dataType.UnderlyingOrDataType();

            // Convert raw value collection to strings.
            var currentValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var value in rawValues)
            {
                // Add original or converted string.
                var stringValue = (value as string) ?? Convert.ToString(value, CultureInfo.CurrentCulture);

                // Do not add simple names of enum properties here because whitespace isn't relevant for their binding.
                // Will add matching names just below.
                if (enumNames == null || !enumNames.ContainsKey(stringValue.Trim()))
                    currentValues.Add(stringValue);

                // Remainder handles isEnum cases. Convert.ToString() returns field names for enum values but select
                // list may (well, should) contain integer values.
                var enumValue = value as Enum;
                if (isTargetEnum && enumValue == null && value != null)
                {
                    var valueType = value.GetType();
                    if (typeof(long).IsAssignableFrom(valueType) || typeof(ulong).IsAssignableFrom(valueType))
                    {
                        // E.g. user added an int to a ViewData entry and called a string-based HTML helper.
                        enumValue = ConvertEnumFromInteger(value, innerType);
                    }
                    else if (!string.IsNullOrEmpty(stringValue))
                    {
                        // E.g. got a string from ModelState.
                        var methodInfo = ConvertEnumFromStringMethod.MakeGenericMethod(innerType);
                        enumValue = (Enum)methodInfo.Invoke(obj: null, parameters: new[] { stringValue });
                    }
                }

                if (enumValue != null)
                {
                    // Add integer value.
                    var integerString = enumValue.ToString("d");
                    currentValues.Add(integerString);

                    // isTargetEnum may be false when raw value has a different type than the target e.g. ViewData
                    // contains enum values and property has type int or string.
                    if (isTargetEnum)
                    {
                        // Add all simple names for this value.
                        var matchingNames = enumNames
                            .Where(kvp => string.Equals(integerString, kvp.Value, StringComparison.Ordinal))
                            .Select(kvp => kvp.Key);
                        foreach (var name in matchingNames)
                        {
                            currentValues.Add(name);
                        }
                    }
                }
            }

            return currentValues;
        }

        private static Enum ConvertEnumFromInteger(object value, Type targetType)
        {
            try
            {
                return (Enum)Enum.ToObject(targetType, value);
            }
            catch (Exception exception)
            when (exception is FormatException || exception.InnerException is FormatException)
            {
                // The integer was too large for this enum type.
                return null;
            }
        }

        private static readonly MethodInfo ConvertEnumFromStringMethod =
            typeof(DefaultDataSetHtmlGenerator).GetTypeInfo().GetDeclaredMethod(nameof(ConvertEnumFromString));

        private static object ConvertEnumFromString<TEnum>(string value) where TEnum : struct
        {
            if (Enum.TryParse(value, out TEnum enumValue))
                return enumValue;

            // Do not return default(TEnum) when parse was unsuccessful.
            return null;
        }
    }
}

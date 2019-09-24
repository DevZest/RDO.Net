using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DevZest.Data.AspNetCore.Primitives
{
    /// <summary>
    /// Contract for a service supporting DataSet HTML generation.
    /// </summary>
    public interface IDataSetHtmlGenerator
    {
        /// <summary>
        /// Gets the <see cref="string"/> that replaces periods in the ID attribute of an element.
        /// </summary>
        string IdAttributeDotReplacement { get; }

        /// <summary>
        /// Generate a &lt;input type="text".../&gt; element.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="fullHtmlFieldName">The full HTML field name.</param>
        /// <param name="column">The column.</param>
        /// <param name="value">The data value.</param>
        /// <param name="format">The string format.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>The tag builder.</returns>
        TagBuilder GenerateTextBox(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, string format, object htmlAttributes);

        /// <summary>
        /// Generate an additional &lt;input type="hidden".../&gt; for checkboxes. This addresses scenarios where unchecked checkboxes
        /// are not sent in the request. Sending a hidden input makes it possible to know that the checkbox was present on the page
        /// when the request was submitted.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="fullHtmlFieldName">The full HTML field name.</param>
        /// <returns>The tag builder.</returns>
        TagBuilder GenerateHiddenForCheckbox(ViewContext viewContext, string fullHtmlFieldName);

        /// <summary>
        /// Generate a &lt;input type="checkbox".../&gt; element.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="fullHtmlFieldName">The full HTML field name.</param>
        /// <param name="column">The column.</param>
        /// <param name="isChecked">Indicates whether checkbox is checked.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>The tag helper.</returns>
        TagBuilder GenerateCheckBox(ViewContext viewContext, string fullHtmlFieldName, Column column, bool? isChecked, object htmlAttributes);

        /// <summary>
        /// Generate a &lt;input type="hidden".../&gt; element.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="fullHtmlFieldName">The full HTML field name.</param>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>The tag builder.</returns>
        TagBuilder GenerateHidden(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, object htmlAttributes);

        /// <summary>
        /// Generate a &lt;label.../&gt; element.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="fullHtmlFieldName">The full HTML field name.</param>
        /// <param name="column">The column.</param>
        /// <param name="labelText">The label text.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>The tag builder.</returns>
        TagBuilder GenerateLabel(ViewContext viewContext, string fullHtmlFieldName, Column column, string labelText, object htmlAttributes);

        /// <summary>
        /// Generate a &lt;input type="password".../&gt; element.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="fullHtmlFieldName">The full HTML field name.</param>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>The tag builder.</returns>
        TagBuilder GeneratePassword(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, object htmlAttributes);

        /// <summary>
        /// Generate a &lt;input type="radio".../&gt; element.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="fullHtmlFieldName">The full HTML field name.</param>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <param name="isChecked">Indicates whether radio button is checked.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>The tag builder.</returns>
        TagBuilder GenerateRadioButton(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, bool? isChecked, object htmlAttributes);

        /// <summary>
        /// Generate a &lt;select&gt; element.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="fullHtmlFieldName">The full HTML field name.</param>
        /// <param name="column">The column.</param>
        /// <param name="dataValue">The data value.</param>
        /// <param name="optionLabel">The option label.</param>
        /// <param name="selectList">The select list.</param>
        /// <param name="allowMultiple">Indicates whether multiple select allowed.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>The tag builder.</returns>
        TagBuilder GenerateSelect(
            ViewContext viewContext,
            string fullHtmlFieldName,
            Column column,
            object dataValue,
            string optionLabel,
            IEnumerable<SelectListItem> selectList,
            bool allowMultiple,
            object htmlAttributes);

        /// <summary>
        /// Generate a &lt;select&gt; element.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="fullHtmlFieldName">The full HTML field name.</param>
        /// <param name="column">The column.</param>
        /// <param name="optionLabel">The option label.</param>
        /// <param name="selectList">The select list.</param>
        /// <param name="currentValues">The current values.</param>
        /// <param name="allowMultiple">Indicates whether multiple select allowed.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>The tag builder.</returns>
        TagBuilder GenerateSelect(
            ViewContext viewContext,
            string fullHtmlFieldName,
            Column column,
            string optionLabel,
            IEnumerable<SelectListItem> selectList,
            ICollection<string> currentValues,
            bool allowMultiple,
            object htmlAttributes);

        /// <summary>
        /// Generate a &lt;textarea&gt; element.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="fullHtmlFieldName">The full HTML field name.</param>
        /// <param name="column">The column.</param>
        /// <param name="dataValue">The data value.</param>
        /// <param name="rows">Specifies the visible number of lines in a text area.</param>
        /// <param name="cols">Specifies the visible width of a text area.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>The tag builder.</returns>
        TagBuilder GenerateTextArea(
            ViewContext viewContext,
            string fullHtmlFieldName,
            Column column,
            object dataValue,
            int rows,
            int cols,
            object htmlAttributes);

        /// <summary>
        /// Generate a tag element if the viewContext's ModelState contains an error for the column.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="fullHtmlFieldName">The full HTML field name.</param>
        /// <param name="column">The column.</param>
        /// <param name="message">The error message.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>The tag builder.</returns>
        TagBuilder GenerateValidationMessage(
            ViewContext viewContext,
            string fullHtmlFieldName,
            Column column,
            string message,
            string tag,
            object htmlAttributes);

        /// <summary>
        /// Gets the collection of current values for the given column.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="column">The column.</param>
        /// <param name="dataValue">The data value.</param>
        /// <param name="allowMultiple">Indicates whether multiple values allowed.</param>
        /// <returns><see langword="null"/> if no expression result is found. Otherwise a collection of strings containing current values.</returns>
        ICollection<string> GetCurrentValues(ViewContext viewContext, Column column, object dataValue, bool allowMultiple);
    }
}

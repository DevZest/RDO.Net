using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DevZest.Data.AspNetCore.Primitives
{
    public interface IDataSetHtmlGenerator
    {
        string IdAttributeDotReplacement { get; }

        TagBuilder GenerateTextBox(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, string format, object htmlAttributes);

        TagBuilder GenerateHiddenForCheckbox(string fullHtmlFieldName);

        TagBuilder GenerateCheckBox(ViewContext viewContext, string fullHtmlFieldName, Column column, bool? isChecked, object htmlAttributes);

        TagBuilder GenerateHidden(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, object htmlAttributes);

        TagBuilder GenerateLabel(ViewContext viewContext, string fullHtmlFieldName, Column column, string labelText, object htmlAttributes);

        TagBuilder GeneratePassword(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, object htmlAttributes);

        TagBuilder GenerateRadioButton(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, bool? isChecked, object htmlAttributes);

        TagBuilder GenerateSelect(ViewContext viewContext,
            string fullHtmlFieldName,
            Column column,
            object dataValue,
            string optionLabel,
            IEnumerable<SelectListItem> selectList,
            bool allowMultiple,
            object htmlAttributes);

        TagBuilder GenerateSelect(
            ViewContext viewContext,
            string fullHtmlFieldName,
            Column column,
            string optionLabel,
            IEnumerable<SelectListItem> selectList,
            ICollection<string> currentValues,
            bool allowMultiple,
            object htmlAttributes);

        TagBuilder GenerateValidationMessage(ViewContext viewContext, string fullHtmlFieldName, Column column, string message, string tag, object htmlAttributes);

        ICollection<string> GetCurrentValues(ViewContext viewContext, string fullHtmlFieldName, Column column, object dataValue, bool allowMultiple);
    }
}

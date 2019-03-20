using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DevZest.Data.AspNetCore.Primitives
{
    public interface IDataSetHtmlGenerator
    {
        string IdAttributeDotReplacement { get; }

        TagBuilder GenerateLabel(ViewContext viewContext, string fullHtmlFieldName, Column column, string labelText, object htmlAttributes);

        TagBuilder GenerateTextBox(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, string format, object htmlAttributes);

        TagBuilder GenerateHiddenForCheckbox(string fullHtmlFieldName);

        TagBuilder GenerateCheckBox(ViewContext viewContext, string fullFieldName, Column column, bool? isChecked, object htmlAttributes);

        TagBuilder GeneratePassword(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, object htmlAttributes);

        TagBuilder GenerateRadioButton(ViewContext viewContext, string fullHtmlFieldName, Column column, object value, bool? isChecked, object htmlAttributes);
    }
}

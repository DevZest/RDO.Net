using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DevZest.Data.AspNetCore.Primitives
{
    public interface IDataSetHtmlGenerator
    {
        string IdAttributeDotReplacement { get; }

        TagBuilder GenerateLabel(ViewContext viewContext, string fullName, Column column, string labelText, object htmlAttributes);

    }
}

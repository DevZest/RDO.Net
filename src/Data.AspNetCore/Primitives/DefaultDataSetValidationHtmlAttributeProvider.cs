using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DefaultDataSetValidationHtmlAttributeProvider : DataSetValidationHtmlAttributeProvider
    {
        protected override void AddValidationAttributes(ViewContext viewContext, Column column, IDictionary<string, string> attributes)
        {
            //TODO: to be implemented.
        }
    }
}

using DevZest.Data.Primitives;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DevZest.Data.AspNetCore.ClientValidation
{
    public interface IDataSetClientValidator
    {
        void AddValidation(ActionContext actionContext, Column column, IDictionary<string, string> attributes);
    }
}

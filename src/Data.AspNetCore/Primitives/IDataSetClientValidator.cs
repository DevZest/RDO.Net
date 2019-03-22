using DevZest.Data.Primitives;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DevZest.Data.AspNetCore.Primitives
{
    public interface IDataSetClientValidator
    {
        void AddValidation(IValidator validator, ActionContext actionContext, Column column, IDictionary<string, string> attributes);
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DevZest.Data.AspNetCore.ClientValidation
{
    /// <summary>
    /// Represents client validator for column data values in DataSet.
    /// </summary>
    public interface IDataSetClientValidator
    {
        /// <summary>
        /// Adds validation HTML attributes.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        /// <param name="column">The column.</param>
        /// <param name="attributes">The HTML attributes dictionary.</param>
        void AddValidation(ActionContext actionContext, Column column, IDictionary<string, string> attributes);
    }
}

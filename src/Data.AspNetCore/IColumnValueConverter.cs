using DevZest.Data.Addons;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace DevZest.Data.AspNetCore
{
    /// <summary>
    /// Represents the column value converter.
    /// </summary>
    public interface IColumnValueConverter : IAddon
    {
        /// <summary>
        /// Converts to column value.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="valueProviderResult">The value provider result.</param>
        /// <param name="defaultConverter">The default converter.</param>
        /// <returns>The converted column value.</returns>
        object ConvertToColumnValue(Column column, ValueProviderResult valueProviderResult, Func<Column, ValueProviderResult, object> defaultConverter);
    }
}

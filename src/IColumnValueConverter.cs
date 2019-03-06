using DevZest.Data.Addons;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace DevZest.Data.AspNetCore
{
    public interface IColumnValueConverter : IAddon
    {
        object ConvertToColumnValue(Column column, ValueProviderResult valueProviderResult, Func<Column, ValueProviderResult, object> defaultConverter);
    }
}

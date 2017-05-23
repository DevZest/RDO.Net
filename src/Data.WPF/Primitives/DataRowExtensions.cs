using DevZest.Data;
using System.Runtime.CompilerServices;

namespace DevZest.Windows.Primitives
{
    internal static class DataRowExtensions
    {
        private static readonly ConditionalWeakTable<DataRow, RowPresenter> s_dataRowToRowPresenterMappings = new ConditionalWeakTable<DataRow, RowPresenter>();

        internal static RowPresenter GetRowPresenter(this DataRow dataRow)
        {
            RowPresenter result;
            return s_dataRowToRowPresenterMappings.TryGetValue(dataRow, out result) ? result : null;
        }

        internal static void SetRowPresenter(this DataRow dataRow, RowPresenter value)
        {
            s_dataRowToRowPresenterMappings.Remove(dataRow);
            if (value != null)
                s_dataRowToRowPresenterMappings.Add(dataRow, value);
        }
    }
}

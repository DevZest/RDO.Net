using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DevZest.Data.Windows
{
    internal static class ModelExtensions
    {
        private static ConditionalWeakTable<Model, RowPresenter> s_editingRows = new ConditionalWeakTable<Model, RowPresenter>();

        internal static RowPresenter GetEditingRow(this Model model)
        {
            Debug.Assert(model != null);

            RowPresenter result;
            if (s_editingRows.TryGetValue(model, out result))
                return result;
            return null;
        }

        internal static void SetEditingRow(this Model model, RowPresenter value)
        {
            Debug.Assert(model != null);

            RowPresenter oldValue;
            if (!s_editingRows.TryGetValue(model, out oldValue))
                oldValue = null;

            if (value == oldValue)
                return;

            if (oldValue != null)
                s_editingRows.Remove(model);

            if (value != null)
                s_editingRows.Add(model, value);
        }
    }
}

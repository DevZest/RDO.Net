using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DevZest.Data.Windows
{
    internal static class ModelExtensions
    {
        private static ConditionalWeakTable<Model, RowView> s_editingRows = new ConditionalWeakTable<Model, RowView>();

        internal static RowView GetEditingRow(this Model model)
        {
            Debug.Assert(model != null);

            RowView result;
            if (s_editingRows.TryGetValue(model, out result))
                return result;
            return null;
        }

        internal static void SetEditingRow(this Model model, RowView value)
        {
            Debug.Assert(model != null);

            RowView oldValue;
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

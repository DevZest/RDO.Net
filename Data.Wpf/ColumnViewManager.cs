using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class ColumnViewManager<T> : ViewManager<T>
        where T : UIElement, new()
    {
        internal ColumnViewManager(Column column, Action<T> initializer)
            : base(initializer)
        {
            Debug.Assert(column != null);
            Column = column;
        }

        public Column Column { get; private set; }

        internal sealed override bool IsValidFor(Model model)
        {
            return Column.GetParentModel() == model;
        }
    }
}

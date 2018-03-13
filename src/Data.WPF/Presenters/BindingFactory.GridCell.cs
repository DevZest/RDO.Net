using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowCompositeBinding<GridCell> MergeIntoGridCell<T>(this RowBinding<T> editingRowBinding, string format = null, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            return editingRowBinding.MergeIntoInPlaceEditor(format, formatProvider).AddToGridCell();

        }

        public static RowCompositeBinding<GridCell> MergeIntoGridCell<TEditing, TInert>(this RowBinding<TEditing> editingRowBinding, RowBinding<TInert> inertRowBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            return editingRowBinding.MergeIntoInPlaceEditor(inertRowBinding).AddToGridCell();
        }

        public static RowCompositeBinding<GridCell> AddToGridCell<T>(this RowBindingBase<T> rowBinding)
            where T : UIElement, new()
        {
            if (rowBinding == null)
                throw new ArgumentNullException(nameof(rowBinding));

            var result = new RowCompositeBinding<GridCell>();
            result.AddChild(rowBinding, v => v.GetChild<T>());
            return result;
        }
    }
}

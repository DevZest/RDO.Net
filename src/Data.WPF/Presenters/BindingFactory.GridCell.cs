using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowCompositeBinding<GridCell> AddToGridCell<T>(this RowInput<T> rowInput, string format = null, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            return rowInput.AddToInPlaceEditor(format, formatProvider).AddToGridCell();

        }

        public static RowCompositeBinding<GridCell> AddToGridCell<TEditing, TInert>(this RowInput<TEditing> rowInput, RowBinding<TInert> inertRowBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            return rowInput.AddToInPlaceEditor(inertRowBinding).AddToGridCell();
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

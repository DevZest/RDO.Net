using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
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

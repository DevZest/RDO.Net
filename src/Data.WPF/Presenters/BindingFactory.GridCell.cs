using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        /// <summary>
        /// Merges editor row binding into <see cref="GridCell"/>, with inert element displays as string.
        /// </summary>
        /// <typeparam name="T">The element type of the editor row binding.</typeparam>
        /// <param name="editorRowBinding">The editor row binding.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>The row binding object of <see cref="GridCell"/>.</returns>

        public static RowCompositeBinding<GridCell> MergeIntoGridCell<T>(this RowBinding<T> editorRowBinding, string format = null, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            return editorRowBinding.MergeIntoInPlaceEditor(format, formatProvider).AddToGridCell();

        }

        /// <summary>
        /// Merges editor row binding into <see cref="GridCell"/>, with inert row binding.
        /// </summary>
        /// <typeparam name="TEditor">The element type of the editor row binding.</typeparam>
        /// <typeparam name="TInert">The element type of the inert row binding.</typeparam>
        /// <param name="editorRowBinding">The editor row binding.</param>
        /// <param name="inertRowBinding">The inert row binding.</param>
        /// <returns>The row binding object of <see cref="GridCell"/>.</returns>
        public static RowCompositeBinding<GridCell> MergeIntoGridCell<TEditor, TInert>(this RowBinding<TEditor> editorRowBinding, RowBinding<TInert> inertRowBinding)
            where TEditor : UIElement, new()
            where TInert : UIElement, new()
        {
            return editorRowBinding.MergeIntoInPlaceEditor(inertRowBinding).AddToGridCell();
        }

        /// <summary>
        /// Adds row binding into <see cref="GridCell"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the row binding.</typeparam>
        /// <param name="rowBinding">The row binding.</param>
        /// <returns>The row binding object of <see cref="GridCell"/>.</returns>
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

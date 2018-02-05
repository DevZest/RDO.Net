using DevZest.Data;
using DevZest.Data.Primitives;
using DevZest.Data.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowCompositeBinding<InPlaceEditor> BindToInPlaceEditor<T>(this RowBinding<T> editingRowBinding, string format = null, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            if (editingRowBinding.Input == null)
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_EditingRowBindingNullInput, nameof(editingRowBinding));

            var column = editingRowBinding.Input.Target as Column;
            if (column == null)
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_EditingRowBindingNotColumn, nameof(editingRowBinding));
            var inertRowBinding = column.BindToTextBlock(format, formatProvider);
            return ComposeInPlaceEditor(editingRowBinding, inertRowBinding);
        }

        public static RowCompositeBinding<InPlaceEditor> BindToInPlaceEditor<TEditing, TInert>(this RowBinding<TEditing> editingRowBinding, RowBinding<TInert> inertRowBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            if (editingRowBinding.Input == null)
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_EditingRowBindingNullInput, nameof(editingRowBinding));
            if (inertRowBinding == null)
                throw new ArgumentNullException(nameof(inertRowBinding));
            return ComposeInPlaceEditor(editingRowBinding, inertRowBinding);
        }

        private static RowCompositeBinding<InPlaceEditor> ComposeInPlaceEditor<TEditing, TInert>(RowBinding<TEditing> editingRowBinding, RowBinding<TInert> inertRowBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            //return new CompositeRowBinding<InPlaceEditor>().AddChild(inertRowBinding, v => v.InertElement).AddChild(editingRowBinding, v => v.EditingElement);
            throw new NotImplementedException();
        }
    }
}

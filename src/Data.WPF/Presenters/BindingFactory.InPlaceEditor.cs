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
        public static RowBinding<InPlaceEditor> AddToInPlaceEditor<T>(this RowInput<T> rowInput, string format = null, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            if (rowInput == null)
                throw new ArgumentNullException(nameof(rowInput));

            var column = rowInput.Target as Column;
            if (column == null)
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_EditingRowBindingNotColumn, nameof(rowInput));
            var inertRowBinding = column.BindToTextBlock(format, formatProvider);
            return AddToInPlaceEditor(rowInput, inertRowBinding);

        }

        public static RowBinding<InPlaceEditor> AddToInPlaceEditor<TEditing, TInert>(this RowInput<TEditing> rowInput, RowBinding<TInert> inertRowBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            if (rowInput == null)
                throw new ArgumentNullException(nameof(rowInput));
            if (inertRowBinding == null)
                throw new ArgumentNullException(nameof(inertRowBinding));

            return InPlaceEditor.AddToInPlaceEditor(rowInput, inertRowBinding);
        }

        public static ScalarBinding<InPlaceEditor> AddToInPlaceEditor<T>(this ScalarInput<T> scalarInput, string format = null, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            if (scalarInput == null)
                throw new ArgumentNullException(nameof(scalarInput));

            var scalar = scalarInput.Target as Scalar;
            if (scalar == null)
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_EditingScalarBindingNotScalar, nameof(scalarInput));
            var inertScalarBinding = scalar.BindToTextBlock(format, formatProvider);
            return AddToInPlaceEditor(scalarInput, inertScalarBinding);

        }

        public static ScalarBinding<InPlaceEditor> AddToInPlaceEditor<TEditing, TInert>(this ScalarInput<TEditing> scalarInput, ScalarBinding<TInert> inertScalarBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            if (scalarInput == null)
                throw new ArgumentNullException(nameof(scalarInput));
            if (inertScalarBinding == null)
                throw new ArgumentNullException(nameof(inertScalarBinding));

            return InPlaceEditor.AddToInPlaceEditor(scalarInput, inertScalarBinding);
        }
    }
}

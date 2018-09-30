using DevZest.Data.Views;
using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static RowBinding<InPlaceEditor> MergeIntoInPlaceEditor<T>(this RowBinding<T> editingRowBinding, string format = null, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            var rowInput = VerifyEditingBinding(editingRowBinding, nameof(editingRowBinding));
            var column = rowInput.Target as Column;
            if (column == null)
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_EditingRowBindingNotColumn, nameof(rowInput));
            var inertRowBinding = column.BindToTextBlock(format, formatProvider);
            return MergeIntoInPlaceEditor(rowInput, inertRowBinding);
        }

        public static RowBinding<InPlaceEditor> MergeIntoInPlaceEditor<T>(this RowBinding<T> editingRowBinding, Func<RowPresenter, string> format, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            var rowInput = VerifyEditingBinding(editingRowBinding, nameof(editingRowBinding));
            var column = rowInput.Target as Column;
            if (column == null)
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_EditingRowBindingNotColumn, nameof(rowInput));
            var inertRowBinding = column.BindToTextBlock(format, formatProvider);
            return MergeIntoInPlaceEditor(rowInput, inertRowBinding);
        }

        public static RowBinding<InPlaceEditor> MergeIntoInPlaceEditor<TEditing, TInert>(this RowBinding<TEditing> editingRowBinding, RowBinding<TInert> inertRowBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            var rowInput = VerifyEditingBinding(editingRowBinding, nameof(editingRowBinding));
            inertRowBinding.VerifyNotNull(nameof(inertRowBinding));
            return MergeIntoInPlaceEditor(rowInput, inertRowBinding);
        }

        private static RowInput<T> VerifyEditingBinding<T>(RowBinding<T> editingRowBinding, string paramName)
            where T : UIElement, new()
        {
            if (editingRowBinding == null)
                throw new ArgumentNullException(paramName);
            var rowInput = editingRowBinding.Input;
            if (rowInput == null)
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_VerifyEditingBinding, paramName);
            return rowInput;
        }

        private static RowBinding<InPlaceEditor> MergeIntoInPlaceEditor<TEditing, TInert>(this RowInput<TEditing> rowInput, RowBinding<TInert> inertRowBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            Debug.Assert(rowInput != null);
            Debug.Assert(inertRowBinding != null);
            return InPlaceEditor.AddToInPlaceEditor(rowInput, inertRowBinding);
        }

        public static ScalarBinding<InPlaceEditor> MergeIntoInPlaceEditor<T>(this ScalarBinding<T> editingScalarBinding, string format = null, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            var scalarInput = VerifyEditingBinding(editingScalarBinding, nameof(editingScalarBinding));
            var scalar = scalarInput.Target as Scalar;
            if (scalar == null)
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_EditingScalarBindingNotScalar, nameof(editingScalarBinding));
            var inertScalarBinding = scalar.BindToTextBlock(format, formatProvider);
            return MergeIntoInPlaceEditor(scalarInput, inertScalarBinding);
        }

        public static ScalarBinding<InPlaceEditor> MergeIntoInPlaceEditor<TEditing, TInert>(this ScalarBinding<TEditing> editingScalarBinding, ScalarBinding<TInert> inertScalarBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            var scalarInput = VerifyEditingBinding(editingScalarBinding, nameof(editingScalarBinding));
            inertScalarBinding.VerifyNotNull(nameof(inertScalarBinding));
            return MergeIntoInPlaceEditor(scalarInput, inertScalarBinding);
        }

        private static ScalarInput<T> VerifyEditingBinding<T>(ScalarBinding<T> editingScalarBinding, string paramName)
            where T : UIElement, new()
        {
            if (editingScalarBinding == null)
                throw new ArgumentNullException(paramName);
            var scalarInput = editingScalarBinding.Input;
            if (scalarInput == null)
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_VerifyEditingBinding, paramName);
            return scalarInput;
        }

        private static ScalarBinding<InPlaceEditor> MergeIntoInPlaceEditor<TEditing, TInert>(this ScalarInput<TEditing> scalarInput, ScalarBinding<TInert> inertScalarBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            Debug.Assert(scalarInput != null);
            Debug.Assert(inertScalarBinding != null);
            return InPlaceEditor.AddToInPlaceEditor(scalarInput, inertScalarBinding);
        }
    }
}

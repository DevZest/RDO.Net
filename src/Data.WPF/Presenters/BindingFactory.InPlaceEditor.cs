using DevZest.Data.Views;
using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        /// <summary>
        /// Merges editor row binding into <see cref="InPlaceEditor"/>, with inert element displays as string.
        /// </summary>
        /// <typeparam name="T">The element type of the editor row binding.</typeparam>
        /// <param name="editorRowBinding">The editor row binding.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>The row binding object of <see cref="InPlaceEditor"/>.</returns>
        public static RowBinding<InPlaceEditor> MergeIntoInPlaceEditor<T>(this RowBinding<T> editorRowBinding, string format = null, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            var rowInput = VerifyEditorBinding(editorRowBinding, nameof(editorRowBinding));
            if (!(rowInput.Target is Column column))
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_EditorRowBindingNotColumn, nameof(rowInput));
            var inertRowBinding = column.BindToTextBlock(format, formatProvider);
            return MergeIntoInPlaceEditor(rowInput, inertRowBinding);
        }

        /// <summary>
        /// Merges editor row binding into <see cref="InPlaceEditor"/>, with inert element displays as string.
        /// </summary>
        /// <typeparam name="T">The element type of the editor row binding.</typeparam>
        /// <param name="editorRowBinding">The editor row binding.</param>
        /// <param name="format">A delegate to return composite format string from <see cref="RowPresenter"/>.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>The row binding object of <see cref="InPlaceEditor"/>.</returns>
        public static RowBinding<InPlaceEditor> MergeIntoInPlaceEditor<T>(this RowBinding<T> editorRowBinding, Func<RowPresenter, string> format, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));

            var rowInput = VerifyEditorBinding(editorRowBinding, nameof(editorRowBinding));
            if (!(rowInput.Target is Column column))
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_EditorRowBindingNotColumn, nameof(rowInput));
            var inertRowBinding = column.BindToTextBlock(format, formatProvider);
            return MergeIntoInPlaceEditor(rowInput, inertRowBinding);
        }

        /// <summary>
        /// Merges editor row binding into <see cref="InPlaceEditor"/>, with inert row binding.
        /// </summary>
        /// <typeparam name="TEditor">The element type of the editor row binding.</typeparam>
        /// <typeparam name="TInert">The element type of the inert row binding.</typeparam>
        /// <param name="editorRowBinding">The editor row binding.</param>
        /// <param name="inertRowBinding">The inert row binding.</param>
        /// <returns>The row binding object of <see cref="InPlaceEditor"/>.</returns>
        public static RowBinding<InPlaceEditor> MergeIntoInPlaceEditor<TEditor, TInert>(this RowBinding<TEditor> editorRowBinding, RowBinding<TInert> inertRowBinding)
            where TEditor : UIElement, new()
            where TInert : UIElement, new()
        {
            var rowInput = VerifyEditorBinding(editorRowBinding, nameof(editorRowBinding));
            inertRowBinding.VerifyNotNull(nameof(inertRowBinding));
            return MergeIntoInPlaceEditor(rowInput, inertRowBinding);
        }

        private static RowInput<T> VerifyEditorBinding<T>(RowBinding<T> editorRowBinding, string paramName)
            where T : UIElement, new()
        {
            if (editorRowBinding == null)
                throw new ArgumentNullException(paramName);
            var rowInput = editorRowBinding.Input;
            if (rowInput == null)
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_VerifyEditorBinding, paramName);
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

        /// <summary>
        /// Merges editor scalar binding into <see cref="InPlaceEditor"/>, with inert element displays as string.
        /// </summary>
        /// <typeparam name="T">The element type of editor scalar binding.</typeparam>
        /// <param name="editorScalarBinding">The editor scalar binding.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>The scalar binding object of <see cref="InPlaceEditor"/>.</returns>
        public static ScalarBinding<InPlaceEditor> MergeIntoInPlaceEditor<T>(this ScalarBinding<T> editorScalarBinding, string format = null, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            var scalarInput = VerifyEditorBinding(editorScalarBinding, nameof(editorScalarBinding));
            if (!(scalarInput.Target is Scalar scalar))
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_EditorScalarBindingNotScalar, nameof(editorScalarBinding));
            var inertScalarBinding = scalar.BindToTextBlock(format, formatProvider);
            return MergeIntoInPlaceEditor(scalarInput, inertScalarBinding);
        }

        /// <summary>
        /// Merges editor scalar binding into <see cref="InPlaceEditor"/>, with inert scalar binding.
        /// </summary>
        /// <typeparam name="TEditor">The element type of editor scalar binding.</typeparam>
        /// <typeparam name="TInert">The element type of inert scalar binding.</typeparam>
        /// <param name="editorScalarBinding">The editor scalar binding.</param>
        /// <param name="inertScalarBinding">The inert scacalr binding.</param>
        /// <returns>The scalar binding object of <see cref="InPlaceEditor"/>.</returns>
        public static ScalarBinding<InPlaceEditor> MergeIntoInPlaceEditor<TEditor, TInert>(this ScalarBinding<TEditor> editorScalarBinding, ScalarBinding<TInert> inertScalarBinding)
            where TEditor : UIElement, new()
            where TInert : UIElement, new()
        {
            var scalarInput = VerifyEditorBinding(editorScalarBinding, nameof(editorScalarBinding));
            inertScalarBinding.VerifyNotNull(nameof(inertScalarBinding));
            return MergeIntoInPlaceEditor(scalarInput, inertScalarBinding);
        }

        private static ScalarInput<T> VerifyEditorBinding<T>(ScalarBinding<T> editorScalarBinding, string paramName)
            where T : UIElement, new()
        {
            if (editorScalarBinding == null)
                throw new ArgumentNullException(paramName);
            var scalarInput = editorScalarBinding.Input;
            if (scalarInput == null)
                throw new ArgumentException(DiagnosticMessages.InPlaceEditor_VerifyEditorBinding, paramName);
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

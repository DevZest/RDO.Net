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
        public static CompositeRowBinding<InPlaceEditor> AsInPlaceEditor<T>(this RowBinding<T> editingRowBinding, string format = null, IFormatProvider formatProvider = null)
            where T : UIElement, new()
        {
            if (editingRowBinding.Input == null)
                throw new ArgumentException(Strings.InPlaceEditor_EditingRowBindingNullInput, nameof(editingRowBinding));

            var column = editingRowBinding.Input.Target as Column;
            if (column == null)
                throw new ArgumentException(Strings.InPlaceEditor_EditingRowBindingNotColumn, nameof(editingRowBinding));
            var inertRowBinding = column.AsTextBlock(format, formatProvider);
            return ComposeInPlaceEditor(editingRowBinding, inertRowBinding);
        }

        public static CompositeRowBinding<InPlaceEditor> AsInPlaceEditor<TEditing, TInert>(this RowBinding<TEditing> editingRowBinding, RowBinding<TInert> inertRowBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            if (editingRowBinding.Input == null)
                throw new ArgumentException(Strings.InPlaceEditor_EditingRowBindingNullInput, nameof(editingRowBinding));
            if (inertRowBinding == null)
                throw new ArgumentNullException(nameof(inertRowBinding));
            return ComposeInPlaceEditor(editingRowBinding, inertRowBinding);
        }

        private static CompositeRowBinding<InPlaceEditor> ComposeInPlaceEditor<TEditing, TInert>(RowBinding<TEditing> editingRowBinding, RowBinding<TInert> inertRowBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            return new CompositeRowBinding<InPlaceEditor>().AddChild(inertRowBinding, "INERT").AddChild(editingRowBinding, "EDITING");
        }

        public static RowBinding<ComboBox> AsComboBox<T>(this Column<T> source, IEnumerable selectionData, string selectedValuePath, string displayMemberPath)
        {
            return new RowBinding<ComboBox>(
                onSetup: (e, r) =>
                {
                    e.ItemsSource = selectionData;
                    e.SelectedValuePath = selectedValuePath;
                    e.DisplayMemberPath = displayMemberPath;
                },
                onRefresh: (e, r) =>
                {
                    e.SelectedValue = r.GetValue(source);
                    e.IsDropDownOpen = false;
                },
                onCleanup: (e, r) =>
                {
                    e.ItemsSource = null;
                    e.SelectedValuePath = null;
                    e.DisplayMemberPath = null;
                }
                ).WithInput(new PropertyChangedTrigger<ComboBox>(ComboBox.SelectedValueProperty), source, e => (T)e.SelectedValue);
        }

        public static ScalarBinding<ColumnHeader> AsColumnHeader(this Column column, object title = null)
        {
            return new ScalarBinding<ColumnHeader>(
                onRefresh: null,
                onCleanup: null,
                onSetup: e =>
                {
                    e.Column = column;
                    e.Content = title ?? column.DisplayName;
                });
        }

        public static RowBinding<RowHeader> AsRowHeader(this Model source)
        {
            return new RowBinding<RowHeader>(onRefresh: null);
        }

        public static ScalarBinding<TextBlock> AsScalarTextBlock(this string text)
        {
            return new ScalarBinding<TextBlock>(
                onSetup: e =>
                {
                    e.Text = text;
                },
                onRefresh: null,
                onCleanup: null);
        }


        public static ScalarBinding<TextBlock> AsTextBlock<T>(this Scalar<T> source, string format = null, IFormatProvider formatProvider = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<TextBlock>(
                onRefresh: e =>
                {
                    e.Text = source.Value.ToString(format, formatProvider);
                });
        }

        public static RowBinding<ValidationView> AsValidationView<T>(this RowInput<T> source)
            where T : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<ValidationView>(
                onSetup: (e, r) =>
                {
                    e.AsyncValidators = source.AsyncValidators;
                },
                onRefresh: (e, r) =>
                {
                    e.Errors = source.GetErrors(r);
                    e.Warnings = source.GetWarnings(r);
                    e.RefreshStatus();
                },
                onCleanup: (e, r) =>
                {
                    e.AsyncValidators = RowAsyncValidators.Empty;
                    e.Errors = e.Warnings = Array<ValidationMessage>.Empty;
                });
        }

        public static RowBinding<ValidationView> AsValidationView(this Model source)
        {
            return new RowBinding<ValidationView>(
                onSetup: (e, r) =>
                {
                    e.AsyncValidators = r.AsyncValidators;
                },
                onRefresh: (e, r) =>
                {
                    e.RefreshStatus();
                },
                onCleanup: (e, r) =>
                {
                    e.AsyncValidators = RowAsyncValidators.Empty;
                });
        }

        public static ScalarBinding<ValidationView> AsValidationView(this DataPresenter source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<ValidationView>(
                onRefresh: e =>
                {
                    e.AsyncValidators = source.AllRowsAsyncValidators;
                },
                onSetup: e =>
                {
                    e.RefreshStatus();
                },
                onCleanup: e =>
                {
                    e.AsyncValidators = RowAsyncValidators.Empty;
                });
        }

        public static RowBinding<Image> AsImage(this Column<ImageSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new RowBinding<Image>(onRefresh: (e, row) =>
            {
                e.Source = row.GetValue(source);
            });
        }
    }
}

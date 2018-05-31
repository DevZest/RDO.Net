using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    static partial class BindingFactory
    {
        public static RowBinding<ComboBox> BindToComboBox<T>(this EnumColumn<T> enumColumn)
            where T : struct, IConvertible
        {
            return enumColumn.BindToComboBox(enumColumn.EnumItems.Select(x => new
            {
                Value = x.Value,
                Description = x.Description
            }), nameof(EnumItem<T?>.Value), nameof(EnumItem<T?>.Description));
        }

        public static RowBinding<ComboBox> BindToComboBox<T>(this Column<T> source, IEnumerable selectionData, string selectedValuePath, string displayMemberPath)
        {
            source.VerifyNotNull(nameof(source));
            selectionData.VerifyNotNull(nameof(selectionData));
            if (string.IsNullOrEmpty(selectedValuePath))
                throw new ArgumentNullException(nameof(selectedValuePath));
            if (string.IsNullOrEmpty(displayMemberPath))
                throw new ArgumentNullException(nameof(displayMemberPath));

            return new RowBinding<ComboBox>(
                onSetup: (v, p) =>
                {
                    v.ItemsSource = selectionData;
                    v.SelectedValuePath = selectedValuePath;
                    v.DisplayMemberPath = displayMemberPath;
                },
                onRefresh: (v, p) =>
                {
                    v.SelectedValue = p.GetValue(source);
                },
                onCleanup: (v, p) =>
                {
                    v.ItemsSource = null;
                    v.SelectedValuePath = null;
                    v.DisplayMemberPath = null;
                }).WithInput(ComboBox.SelectedValueProperty, source, e => (T)e.SelectedValue);
        }

        public static ScalarBinding<ComboBox> BindToComboBox<T>(this Scalar<T> source, IEnumerable selectionData, string selectedValuePath, string displayMemberPath)
        {
            source.VerifyNotNull(nameof(source));
            selectionData.VerifyNotNull(nameof(selectionData));
            if (string.IsNullOrEmpty(selectedValuePath))
                throw new ArgumentNullException(nameof(selectedValuePath));
            if (string.IsNullOrEmpty(displayMemberPath))
                throw new ArgumentNullException(nameof(displayMemberPath));

            return new ScalarBinding<ComboBox>(
                onSetup: (v, p) =>
                {
                    v.ItemsSource = selectionData;
                    v.SelectedValuePath = selectedValuePath;
                    v.DisplayMemberPath = displayMemberPath;
                },
                onRefresh: (v, p) =>
                {
                    v.SelectedValue = source.GetValue();
                },
                onCleanup: (v, p) =>
                {
                    v.ItemsSource = null;
                    v.SelectedValuePath = null;
                    v.DisplayMemberPath = null;
                }).WithInput(ComboBox.SelectedValueProperty, source, e => (T)e.SelectedValue);
        }
    }
}

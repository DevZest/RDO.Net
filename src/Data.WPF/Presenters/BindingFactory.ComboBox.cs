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
            Check.NotNull(source, nameof(source));
            Check.NotNull(selectionData, nameof(selectionData));
            if (string.IsNullOrEmpty(selectedValuePath))
                throw new ArgumentNullException(nameof(selectedValuePath));
            if (string.IsNullOrEmpty(displayMemberPath))
                throw new ArgumentNullException(nameof(displayMemberPath));

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
                },
                onCleanup: (e, r) =>
                {
                    e.ItemsSource = null;
                    e.SelectedValuePath = null;
                    e.DisplayMemberPath = null;
                }).WithInput(ComboBox.SelectedValueProperty, source, e => (T)e.SelectedValue);
        }
    }
}

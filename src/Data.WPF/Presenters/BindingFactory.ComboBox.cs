using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    static partial class BindingFactory
    {
        /// <summary>
        /// Binds an enum column to <see cref="ComboBox"/>.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="source">The source column.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<ComboBox> BindToComboBox<T>(this EnumColumn<T> source)
            where T : struct, IConvertible
        {
            return source.BindToComboBox(source.EnumItems.Select(x => new
            {
                Value = x.Value,
                Display = x.Description
            }));
        }

        /// <summary>
        /// Binds a column to <see cref="ComboBox"/>.
        /// </summary>
        /// <typeparam name="T">The data type of the column.</typeparam>
        /// <param name="source">The source column.</param>
        /// <param name="dropDownList">Data of drop-down list.</param>
        /// <param name="selectedValuePath">The path that is used to get the selected value from drop-down list.</param>
        /// <param name="displayMemberPath">The path to serve as the visual representation of the drop-down list item.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<ComboBox> BindToComboBox<T>(this Column<T> source, IEnumerable dropDownList, string selectedValuePath = "Value", string displayMemberPath = "Display")
        {
            source.VerifyNotNull(nameof(source));
            dropDownList.VerifyNotNull(nameof(dropDownList));
            if (string.IsNullOrEmpty(selectedValuePath))
                throw new ArgumentNullException(nameof(selectedValuePath));
            if (string.IsNullOrEmpty(displayMemberPath))
                throw new ArgumentNullException(nameof(displayMemberPath));

            return new RowBinding<ComboBox>(
                onSetup: (v, p) =>
                {
                    v.ItemsSource = dropDownList;
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

        /// <summary>
        /// Binds a column to <see cref="ComboBox"/>.
        /// </summary>
        /// <typeparam name="T">The data type of the column.</typeparam>
        /// <param name="source">The source column.</param>
        /// <param name="dropDownList">Data of drop-down list.</param>
        /// <param name="selectedValuePath">The path that is used to get the selected value from drop-down list.</param>
        /// <param name="displayMemberPath">The path to serve as the visual representation of the drop-down list item.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<ComboBox> BindToComboBox<T>(this Column<T> source, Column<IEnumerable> dropDownList, string selectedValuePath = "Value", string displayMemberPath = "Display")
        {
            source.VerifyNotNull(nameof(source));
            dropDownList.VerifyNotNull(nameof(dropDownList));
            if (string.IsNullOrEmpty(selectedValuePath))
                throw new ArgumentNullException(nameof(selectedValuePath));
            if (string.IsNullOrEmpty(displayMemberPath))
                throw new ArgumentNullException(nameof(displayMemberPath));

            return new RowBinding<ComboBox>(
                onSetup: (v, p) =>
                {
                    v.SelectedValuePath = selectedValuePath;
                    v.DisplayMemberPath = displayMemberPath;
                },
                onRefresh: (v, p) =>
                {
                    v.ItemsSource = p.GetValue(dropDownList);
                    v.SelectedValue = p.GetValue(source);
                },
                onCleanup: (v, p) =>
                {
                    v.ItemsSource = null;
                    v.SelectedValuePath = null;
                    v.DisplayMemberPath = null;
                }).WithInput(ComboBox.SelectedValueProperty, source, e => (T)e.SelectedValue);
        }

        /// <summary>
        /// Binds a scalar data to <see cref="ComboBox"/>.
        /// </summary>
        /// <typeparam name="T">The data type of the column.</typeparam>
        /// <param name="source">The source scalar data.</param>
        /// <param name="dropDownList">Data of drop-down list.</param>
        /// <param name="selectedValuePath">The path that is used to get the selected value from drop-down list.</param>
        /// <param name="displayMemberPath">The path to serve as the visual representation of the drop-down list item.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<ComboBox> BindToComboBox<T>(this Scalar<T> source, IEnumerable dropDownList, string selectedValuePath = "Value", string displayMemberPath = "Display")
        {
            source.VerifyNotNull(nameof(source));
            dropDownList.VerifyNotNull(nameof(dropDownList));
            if (string.IsNullOrEmpty(selectedValuePath))
                throw new ArgumentNullException(nameof(selectedValuePath));
            if (string.IsNullOrEmpty(displayMemberPath))
                throw new ArgumentNullException(nameof(displayMemberPath));

            return new ScalarBinding<ComboBox>(
                onSetup: (v, p) =>
                {
                    v.ItemsSource = dropDownList;
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

        /// <summary>
        /// Binds a scalar data to <see cref="ComboBox"/>.
        /// </summary>
        /// <typeparam name="T">The data type of the column.</typeparam>
        /// <param name="source">The source scalar data.</param>
        /// <param name="dropDownList">Data of drop-down list.</param>
        /// <param name="selectedValuePath">The path that is used to get the selected value from drop-down list.</param>
        /// <param name="displayMemberPath">The path to serve as the visual representation of the drop-down list item.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<ComboBox> BindToComboBox<T>(this Scalar<T> source, Scalar<IEnumerable> dropDownList, string selectedValuePath = "Value", string displayMemberPath = "Display")
        {
            source.VerifyNotNull(nameof(source));
            dropDownList.VerifyNotNull(nameof(dropDownList));
            if (string.IsNullOrEmpty(selectedValuePath))
                throw new ArgumentNullException(nameof(selectedValuePath));
            if (string.IsNullOrEmpty(displayMemberPath))
                throw new ArgumentNullException(nameof(displayMemberPath));

            return new ScalarBinding<ComboBox>(
                onSetup: (v, p) =>
                {
                    v.SelectedValuePath = selectedValuePath;
                    v.DisplayMemberPath = displayMemberPath;
                },
                onRefresh: (v, p) =>
                {
                    v.ItemsSource = dropDownList.Value;
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

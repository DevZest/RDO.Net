using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        /// <summary>
        /// Binds <see cref="DataPresenter"/> to <see cref="CheckBox"/> to select/deselect all rows.
        /// </summary>
        /// <param name="dataPresenter">The <see cref="DataPresenter"/></param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<CheckBox> BindToCheckBox(this DataPresenter dataPresenter)
        {
            return ToSelectAll<CheckBox>(dataPresenter);
        }

        /// <summary>
        /// Binds <see cref="Model"/> to <see cref="CheckBox"/> to select current row.
        /// </summary>
        /// <param name="model">The model of the DataSet.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<CheckBox> BindToCheckBox(this Model model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var trigger = new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty).WithAction(v =>
            {
                var binding = (TwoWayBinding)v.GetBinding();
                if (binding.IsRefreshing)
                    return;
                var isChecked = v.IsChecked;
                if (!isChecked.HasValue)
                    return;

                var isSelected = isChecked.GetValueOrDefault();
                var row = v.GetRowPresenter();
                row.IsSelected = isSelected;
            });

            return new RowBinding<CheckBox>(
                onRefresh: (v, p) => v.IsChecked = p.IsSelected,
                onSetup: (v, p) => trigger.Attach(v),
                onCleanup: (v, p) => trigger.Detach(v));
        }

        /// <summary>
        /// Binds a nullable boolean column to <see cref="CheckBox"/>.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <param name="title">The title of the CheckBox. If null, the value of <see cref="Column.DisplayName"/> will be used.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<CheckBox> BindToCheckBox(this Column<bool?> source, object title = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new RowBinding<CheckBox>(
                onSetup: (v, p) =>
                {
                    if (v.Content == null)
                        v.Content = title ?? source.DisplayName;
                },
                onRefresh: (v, p) => v.IsChecked = p.GetValue(source), onCleanup: null)
                .WithInput(CheckBox.IsCheckedProperty, source, v => v.IsChecked);
        }

        /// <summary>
        /// Binds a boolean column to <see cref="CheckBox"/>.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <param name="title">The title of the CheckBox. If null, the value of <see cref="Column.DisplayName"/> will be used.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<CheckBox> BindToCheckBox(this Column<bool> source, object title = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new RowBinding<CheckBox>(
                onSetup: (v, p) =>
                {
                    if (v.Content == null)
                        v.Content = title ?? source.DisplayName;
                },
                onRefresh: (v, p) => v.IsChecked = p.GetValue(source), onCleanup: null)
                .WithInput(CheckBox.IsCheckedProperty, source, v => v.IsChecked == true);
        }

        /// <summary>
        /// Binds an enum value column to <see cref="CheckBox"/>.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="source">The source column.</param>
        /// <param name="enumMemberValue">The value of enum member.</param>
        /// <param name="title">The title of the CheckBox. If null, the name of enum member will be used.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<CheckBox> BindToCheckBox<T>(this Column<T> source, T enumMemberValue, object title = null)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(DiagnosticMessages.BindingFactory_EnumTypeRequired(nameof(T)), nameof(T));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<CheckBox>(
                onSetup: (v, p) =>
                {
                    if (v.Content == null)
                        v.Content = title ?? Enum.GetName(typeof(T), enumMemberValue);
                },
                onRefresh: (v, p) => v.IsChecked = p.GetValue(source).HasFlag(enumMemberValue),
                onCleanup: null)
                .BeginInput(new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty), new ExplicitTrigger<CheckBox>())
                .WithFlush(source, (r, v) =>
                {
                    var value = r.GetValue(source);
                    var newValue = value.GetNewValue(enumMemberValue, v);
                    if (source.AreEqual(value, newValue))
                        return false;
                    r.EditValue(source, newValue);
                    return true;
                })
                .EndInput();
        }

        private static bool HasFlag<T>(this T value, T flag)
            where T : struct, IConvertible
        {
            return (value as Enum).HasFlag(flag as Enum);
        }

        private static T GetNewValue<T>(this T value, T flag, CheckBox v)
            where T : struct, IConvertible
        {
            return v.IsChecked == true ? value.AddFlag(flag) : value.RemoveFlag(flag);
        }

        private static T AddFlag<T>(this T value, T flag)
            where T : struct, IConvertible
        {
            return (T)Enum.ToObject(typeof(T), value.ToUInt64(null) | flag.ToUInt64(null));
        }

        private static T RemoveFlag<T>(this T value, T flag)
            where T : struct, IConvertible
        {
            return (T)Enum.ToObject(typeof(T), value.ToUInt64(null) & ~flag.ToUInt64(null));
        }

        /// <summary>
        /// Binds a nullable enum value column to <see cref="CheckBox"/>.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="source">The source column.</param>
        /// <param name="enumMemberValue">The value of enum member.</param>
        /// <param name="title">The title of the CheckBox. If null, the name of enum member will be used.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<CheckBox> BindToCheckBox<T>(this Column<T?> source, T enumMemberValue, object title = null)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(DiagnosticMessages.BindingFactory_EnumTypeRequired(nameof(T)), nameof(T));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new RowBinding<CheckBox>(onRefresh: (v, p) => v.IsChecked = p.GetValue(source).HasFlag(enumMemberValue),
                onSetup: (v, p) =>
                {
                    v.Content = title ?? Enum.GetName(typeof(T), enumMemberValue);
                }, onCleanup: null)
                .BeginInput(new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty), new ExplicitTrigger<CheckBox>())
                .WithFlush(source, (r, v) =>
                {
                    var value = r.GetValue(source);
                    var newValue = value.GetNewValue(enumMemberValue, v, source.IsNullable);
                    if (source.AreEqual(value, newValue))
                        return false;
                    r.EditValue(source, newValue);
                    return true;
                })
                .EndInput();
        }

        private static bool? HasFlag<T>(this T? value, T flag)
            where T : struct, IConvertible
        {
            if (value == null)
                return false;
            else
                return value.GetValueOrDefault().HasFlag(flag);
        }

        private static T? GetNewValue<T>(this T? nullable, T flag, CheckBox v, bool isNullable)
            where T : struct, IConvertible
        {
            var value = nullable.HasValue ? nullable.GetValueOrDefault() : default(T);
            var result = value.GetNewValue(flag, v);
            if (isNullable && result.Equals(default(T)))
                return null;
            else
                return result;
        }

        /// <summary>
        /// Binds a nullable scalar data to <see cref="CheckBox"/>.
        /// </summary>
        /// <param name="source">The source scalar data.</param>
        /// <param name="title">The title of the CheckBox.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<CheckBox> BindToCheckBox(this Scalar<bool?> source, object title = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<CheckBox>(onRefresh: v => v.IsChecked = source.GetValue(),
                onSetup: v =>
                {
                    if (title != null)
                        v.Content = title;
                }, onCleanup: null)
                .BeginInput(new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty))
                .WithFlush(source, v => v.IsChecked)
                .EndInput();
        }

        /// <summary>
        /// Binds a scalar data to <see cref="CheckBox"/>.
        /// </summary>
        /// <param name="source">The source scalar data.</param>
        /// <param name="title">The title of the CheckBox.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<CheckBox> BindToCheckBox(this Scalar<bool> source, object title = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<CheckBox>(onRefresh: v => v.IsChecked = source.GetValue(),
                onSetup: v =>
                {
                    if (title != null)
                        v.Content = title;
                }, onCleanup: null)
                .BeginInput(new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty))
                .WithFlush(source, v => v.IsChecked.Value)
                .EndInput();
        }

        /// <summary>
        /// Binds an enum scalar data to <see cref="CheckBox"/>.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="source">The source scalar data.</param>
        /// <param name="enumMemberValue">The value of enum member.</param>
        /// <param name="title">The title of the CheckBox. If null, the name of enum member will be used.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<CheckBox> BindToCheckBox<T>(this Scalar<T> source, T enumMemberValue, object title = null)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(DiagnosticMessages.BindingFactory_EnumTypeRequired(nameof(T)), nameof(T));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<CheckBox>(onRefresh: v => v.IsChecked = source.GetValue().HasFlag(enumMemberValue),
                onSetup: v =>
                {
                    v.Content = title ?? Enum.GetName(typeof(T), enumMemberValue);
                }, onCleanup: null)
                .BeginInput(new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty), new ExplicitTrigger<CheckBox>())
                .WithFlush(source, v =>
                {
                    var value = source.GetValue();
                    var newValue = value.GetNewValue(enumMemberValue, v);
                    if (Comparer<T>.Default.Compare(value, newValue) == 0)
                        return false;
                    source.EditValue(newValue);
                    return true;
                })
                .EndInput();
        }

        /// <summary>
        /// Binds a nullable enum scalar data to <see cref="CheckBox"/>.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="source">The source scalar data.</param>
        /// <param name="enumMemberValue">The value of enum member.</param>
        /// <param name="title">The title of the CheckBox. If null, the name of enum member will be used.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<CheckBox> BindToCheckBox<T>(this Scalar<T?> source, T enumMemberValue, object title = null)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(DiagnosticMessages.BindingFactory_EnumTypeRequired(nameof(T)), nameof(T));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<CheckBox>(onRefresh: v => v.IsChecked = source.GetValue().HasFlag(enumMemberValue),
                onSetup: v =>
                {
                    v.Content = title ?? Enum.GetName(typeof(T), enumMemberValue);
                }, onCleanup: null)
                .BeginInput(new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty), new ExplicitTrigger<CheckBox>())
                .WithFlush(source, v =>
                {
                    var value = source.GetValue();
                    var newValue = value.GetNewValue(enumMemberValue, v, true);
                    if (Comparer<T?>.Default.Compare(value, newValue) == 0)
                        return false;
                    source.EditValue(newValue);
                    return true;
                })
                .EndInput();
        }
    }
}

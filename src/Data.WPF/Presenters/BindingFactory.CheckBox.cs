using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        public static ScalarBinding<CheckBox> BindToCheckBox(this DataPresenter dataPresenter)
        {
            return ToSelectAll<CheckBox>(dataPresenter);
        }

        public static RowBinding<CheckBox> BindToCheckBox(this Model model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var trigger = new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty).WithExecuteAction(v =>
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

        public static RowBinding<CheckBox> BindToCheckBox(this Column<bool?> source, string display = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(display))
                display = source.DisplayName;
            return new RowBinding<CheckBox>(
                onSetup: (v, p) =>
                {
                    if (v.Content == null)
                        v.Content = display;
                },
                onRefresh: (v, p) => v.IsChecked = p.GetValue(source), onCleanup: null)
                .WithInput(CheckBox.IsCheckedProperty, source, v => v.IsChecked);
        }

        public static RowBinding<CheckBox> BindToCheckBox(this Column<bool> source, string display = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(display))
                display = source.DisplayName;
            return new RowBinding<CheckBox>(
                onSetup: (v, p) =>
                {
                    if (v.Content == null)
                        v.Content = display;
                },
                onRefresh: (v, p) => v.IsChecked = p.GetValue(source), onCleanup: null)
                .WithInput(CheckBox.IsCheckedProperty, source, v => v.IsChecked == true);
        }

        public static RowBinding<CheckBox> BindToCheckBox<T>(this Column<T> source, T flag, string display = null)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(DiagnosticMessages.BindingFactory_EnumTypeRequired(nameof(T)), nameof(T));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(display))
                display = Enum.GetName(typeof(T), flag);

            return new RowBinding<CheckBox>(
                onSetup: (v, p) =>
                {
                    if (v.Content == null)
                        v.Content = display;
                },
                onRefresh: (v, p) => v.IsChecked = p.GetValue(source).HasFlag(flag),
                onCleanup: null)
                .BeginInput(new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty), new ExplicitTrigger<CheckBox>())
                .WithFlush(source, (r, v) =>
                {
                    var value = r.GetValue(source);
                    var newValue = value.GetNewValue(flag, v);
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

        public static RowBinding<CheckBox> BindToCheckBox<T>(this Column<T?> source, T flag, string display = null)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(DiagnosticMessages.BindingFactory_EnumTypeRequired(nameof(T)), nameof(T));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(display))
                display = Enum.GetName(typeof(T), flag);

            return new RowBinding<CheckBox>(onRefresh: (v, p) => v.IsChecked = p.GetValue(source).HasFlag(flag),
                onSetup: (v, p) =>
                {
                    v.Content = display;
                }, onCleanup: null)
                .BeginInput(new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty), new ExplicitTrigger<CheckBox>())
                .WithFlush(source, (r, v) =>
                {
                    var value = r.GetValue(source);
                    var newValue = value.GetNewValue(flag, v, source.IsNullable);
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

        public static ScalarBinding<CheckBox> BindToCheckBox(this Scalar<bool?> source, string display = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<CheckBox>(onRefresh: v => v.IsChecked = source.GetValue(),
                onSetup: v =>
                {
                    if (display != null)
                        v.Content = display;
                }, onCleanup: null)
                .BeginInput(new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty))
                .WithFlush(source, v => v.IsChecked)
                .EndInput();
        }

        public static ScalarBinding<CheckBox> BindToCheckBox(this Scalar<bool> source, string display = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<CheckBox>(onRefresh: v => v.IsChecked = source.GetValue(),
                onSetup: v =>
                {
                    if (display != null)
                        v.Content = display;
                }, onCleanup: null)
                .BeginInput(new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty))
                .WithFlush(source, v => v.IsChecked.Value)
                .EndInput();
        }

        public static ScalarBinding<CheckBox> BindToCheckBox<T>(this Scalar<T> source, T flag, string display = null)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(DiagnosticMessages.BindingFactory_EnumTypeRequired(nameof(T)), nameof(T));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(display))
                display = Enum.GetName(typeof(T), flag);

            return new ScalarBinding<CheckBox>(onRefresh: v => v.IsChecked = source.GetValue().HasFlag(flag),
                onSetup: v =>
                {
                    v.Content = display;
                }, onCleanup: null)
                .BeginInput(new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty), new ExplicitTrigger<CheckBox>())
                .WithFlush(source, v =>
                {
                    var value = source.GetValue();
                    var newValue = value.GetNewValue(flag, v);
                    if (Comparer<T>.Default.Compare(value, newValue) == 0)
                        return false;
                    source.EditValue(newValue);
                    return true;
                })
                .EndInput();
        }

        public static ScalarBinding<CheckBox> BindToCheckBox<T>(this Scalar<T?> source, T flag, string display = null)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(DiagnosticMessages.BindingFactory_EnumTypeRequired(nameof(T)), nameof(T));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(display))
                display = Enum.GetName(typeof(T), flag);

            return new ScalarBinding<CheckBox>(onRefresh: v => v.IsChecked = source.GetValue().HasFlag(flag),
                onSetup: v =>
                {
                    v.Content = display;
                }, onCleanup: null)
                .BeginInput(new PropertyChangedTrigger<CheckBox>(CheckBox.IsCheckedProperty), new ExplicitTrigger<CheckBox>())
                .WithFlush(source, v =>
                {
                    var value = source.GetValue();
                    var newValue = value.GetNewValue(flag, v, true);
                    if (Comparer<T?>.Default.Compare(value, newValue) == 0)
                        return false;
                    source.EditValue(newValue);
                    return true;
                })
                .EndInput();
        }
    }
}

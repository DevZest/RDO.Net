using DevZest.Data.Presenters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevZest.Data.Views
{
    public class ValidationErrorsControl : ItemsControl
    {
        public static class Templates
        {
            public static readonly TemplateId ValidationErrorId = new TemplateId(typeof(ValidationErrorsControl));

            public static ControlTemplate ValidationError
            {
                get { return ValidationErrorId.GetOrLoad(); }
            }
        }

        public static IValueConverter VallidationErrorsConverter
        {
            get { return _ValidationErrorsConverter.Singleton; }
        }

        private class _ValidationErrorsConverter : IValueConverter
        {
            public static readonly _ValidationErrorsConverter Singleton = new _ValidationErrorsConverter();
            private _ValidationErrorsConverter()
            {
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var errors = value as IReadOnlyList<System.Windows.Controls.ValidationError>;
                var result = ValidationErrors.Empty;
                if (errors != null)
                {
                    for (int i = 0; i < errors.Count; i++)
                    {
                        var error = errors[i].ErrorContent as ValidationError;
                        if (error != null)
                            result = result.Add(error);
                    }
                }
                return result;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        public static IValueConverter ItemsCountToBulletVisibilityConverter
        {
            get { return _ValidationErrorsConverter.Singleton; }
        }

        private class _ItemsCountToBulletVisibilityConverter : IValueConverter
        {
            public static readonly _ItemsCountToBulletVisibilityConverter Singleton = new _ItemsCountToBulletVisibilityConverter();
            private _ItemsCountToBulletVisibilityConverter()
            {
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var itemsCount = (int)value;
                return itemsCount == 1 ? Visibility.Collapsed : Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private static readonly DependencyPropertyKey ItemsCountPropertyKey;
        public static readonly DependencyProperty ItemsCountProperty;

        static ValidationErrorsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationErrorsControl), new FrameworkPropertyMetadata(typeof(ValidationErrorsControl)));
            ItemsCountPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ItemsCount), typeof(int), typeof(ValidationErrorsControl), new FrameworkPropertyMetadata(0));
            ItemsCountProperty = ItemsCountPropertyKey.DependencyProperty;
        }

        public int ItemsCount
        {
            get { return (int)GetValue(ItemsCountProperty); }
            private set
            {
                if (value == 0)
                    ClearValue(ItemsCountPropertyKey);
                else
                    SetValue(ItemsCountPropertyKey, value);
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            ItemsCount = Items == null ? 0 : Items.Count;
        }
    }
}

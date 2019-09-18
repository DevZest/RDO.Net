using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevZest.Data.Views
{
    /// <summary>
    /// Represents a control to display validation errors.
    /// </summary>
    public class ValidationErrorsControl : ItemsControl
    {
        /// <summary>
        /// Gets a converter to convert items count to bullet visibility.
        /// </summary>
        public static IValueConverter ItemsCountToBulletVisibilityConverter
        {
            get { return _ItemsCountToBulletVisibilityConverter.Singleton; }
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

        /// <summary>
        /// Identifies the <see cref="ItemsCount"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsCountProperty;

        static ValidationErrorsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationErrorsControl), new FrameworkPropertyMetadata(typeof(ValidationErrorsControl)));
            ItemsCountPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ItemsCount), typeof(int), typeof(ValidationErrorsControl), new FrameworkPropertyMetadata(0));
            ItemsCountProperty = ItemsCountPropertyKey.DependencyProperty;
        }

        /// <summary>
        /// Gets the items count.
        /// </summary>
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

        /// <inheritdoc/>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            ItemsCount = Items == null ? 0 : Items.Count;
        }
    }
}

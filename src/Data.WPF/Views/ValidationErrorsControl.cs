﻿using DevZest.Data.Presenters;
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
        internal static class TemplateIds
        {
            public static readonly TemplateId Failed = new TemplateId(typeof(ValidationErrorsControl));
        }

        public static class Templates
        {
            public static ControlTemplate Failed
            {
                get { return TemplateIds.Failed.GetOrLoad(); }
            }
        }

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
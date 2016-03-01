using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    internal static class UIElementExtensions
    {
        private static readonly DependencyProperty TemplateItemProperty = DependencyProperty.RegisterAttached(nameof(TemplateItem),
            typeof(TemplateItem), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static TemplateItem GetTemplateItem(this UIElement element)
        {
            return (TemplateItem)element.GetValue(TemplateItemProperty);
        }

        internal static void SetTemplateItem(this UIElement element, TemplateItem value)
        {
            Debug.Assert(value != null && element.GetTemplateItem() == null);
            element.SetValue(TemplateItemProperty, value);
        }

        private static readonly DependencyProperty DataPresenterProperty = DependencyProperty.RegisterAttached(nameof(DataPresenter),
            typeof(DataPresenter), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static DataPresenter GetDataPresenter(this UIElement element)
        {
            return (DataPresenter)element.GetValue(DataPresenterProperty);
        }

        internal static void SetDataPresenter(this UIElement element, DataPresenter value)
        {
            if (value == null)
            {
                Debug.Assert(element.GetDataPresenter() != null);
                element.ClearValue(DataPresenterProperty);
            }
            else
            {
                Debug.Assert(element.GetDataPresenter() == null);
                element.SetValue(DataPresenterProperty, value);
            }
        }

        private static readonly DependencyProperty RowPresenterProperty = DependencyProperty.RegisterAttached(nameof(RowPresenter),
            typeof(RowPresenter), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static RowPresenter GetRowPresenter(this UIElement element)
        {
            return (RowPresenter)element.GetValue(RowPresenterProperty);
        }

        internal static void SetRowPresenter(this UIElement element, RowPresenter value)
        {
            if (value == null)
            {
                Debug.Assert(element.GetRowPresenter() != null);
                element.ClearValue(RowPresenterProperty);
            }
            else
            {
                Debug.Assert(element.GetRowPresenter() == null);
                element.SetValue(RowPresenterProperty, value);
            }
        }

        private static readonly DependencyProperty RepeatOrdinalProperty = DependencyProperty.RegisterAttached("RepeatOrdinal",
            typeof(int), typeof(UIElementExtensions), new PropertyMetadata(-1));

        public static int GetRepeatOrdinal(this UIElement element)
        {
            return (int)element.GetValue(RepeatOrdinalProperty);
        }

        internal static void SetRepeatOrdinal(this UIElement element, int value)
        {
            if (value < 0)
                element.ClearValue(RepeatOrdinalProperty);
            else
                element.SetValue(RepeatOrdinalProperty, value);
        }
    }
}

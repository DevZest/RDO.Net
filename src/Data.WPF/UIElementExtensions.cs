using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public static class UIElementExtensions
    {
        private static readonly DependencyProperty TemplateUnitProperty = DependencyProperty.RegisterAttached(nameof(TemplateUnit),
            typeof(TemplateUnit), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static TemplateUnit GetTemplateUnit(this UIElement element)
        {
            return (TemplateUnit)element.GetValue(TemplateUnitProperty);
        }

        internal static void SetTemplateUnit(this UIElement element, TemplateUnit value)
        {
            Debug.Assert(value != null && element.GetTemplateUnit() == null);
            element.SetValue(TemplateUnitProperty, value);
        }

        private static readonly DependencyProperty DataRowPresenterProperty = DependencyProperty.RegisterAttached(nameof(DataRowPresenter),
            typeof(DataRowPresenter), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static DataRowPresenter GetDataRowPresenter(this UIElement element)
        {
            return (DataRowPresenter)element.GetValue(DataRowPresenterProperty);
        }

        internal static void SetDataRowPresenter(this UIElement element, DataRowPresenter value)
        {
            if (value == null)
            {
                Debug.Assert(element.GetDataRowPresenter() != null);
                element.ClearValue(DataRowPresenterProperty);
            }
            else
            {
                Debug.Assert(element.GetDataRowPresenter() == null);
                element.SetValue(DataRowPresenterProperty, value);
            }
        }

        public static T GetSourceValue<T>(this UIElement element, Column<T> column)
        {
            var dataRowPresenter = element.ExpectDataRowPresenter();
            return dataRowPresenter.GetValue(column);
        }

        public static object GetSourceValue(this UIElement element, Column column)
        {
            var dataRowPresenter = element.ExpectDataRowPresenter();
            return dataRowPresenter.GetValue(column);
        }

        public static void SetSourceValue<T>(this UIElement element, Column<T> column, T value, bool suppressUpdateTarget = true)
        {
            var dataRowPresenter = element.ExpectDataRowPresenter();
            dataRowPresenter.SetValue(column, value, suppressUpdateTarget);
        }

        public static void SetSourceValue<T>(this UIElement element, Column column, object value, bool suppressUpdateTarget = true)
        {
            var dataRowPresenter = element.ExpectDataRowPresenter();
            dataRowPresenter.SetValue(column, value, suppressUpdateTarget);
        }

        public static string GetSourceText(this UIElement element, Column column)
        {
            var dataRowPresenter = element.ExpectDataRowPresenter();
            return dataRowPresenter.GetValue(column).ToString();
        }

        private static DataRowPresenter ExpectDataRowPresenter(this UIElement element)
        {
            var result = element.GetDataRowPresenter();
            if (result == null)
                throw new InvalidOperationException(Strings.UIElement_ExpectDataRowPresenter);
            return result;
        }
    }
}

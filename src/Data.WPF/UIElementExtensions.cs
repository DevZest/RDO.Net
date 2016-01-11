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

        private static readonly DependencyProperty RowProperty = DependencyProperty.RegisterAttached("Row",
            typeof(RowView), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static RowView GetRow(this UIElement element)
        {
            return (RowView)element.GetValue(RowProperty);
        }

        internal static void SetRow(this UIElement element, RowView value)
        {
            if (value == null)
            {
                Debug.Assert(element.GetRow() != null);
                element.ClearValue(RowProperty);
            }
            else
            {
                Debug.Assert(element.GetRow() == null);
                element.SetValue(RowProperty, value);
            }
        }

        public static T GetSourceValue<T>(this UIElement element, Column<T> column)
        {
            var row = element.ExpectRow();
            return row.GetValue(column);
        }

        public static object GetSourceValue(this UIElement element, Column column)
        {
            var row = element.ExpectRow();
            return row.GetValue(column);
        }

        public static void SetSourceValue<T>(this UIElement element, Column<T> column, T value, bool suppressUpdateTarget = true)
        {
            var row = element.ExpectRow();
            row.SetValue(column, value, suppressUpdateTarget);
        }

        public static void SetSourceValue<T>(this UIElement element, Column column, object value, bool suppressUpdateTarget = true)
        {
            var row = element.ExpectRow();
            row.SetValue(column, value, suppressUpdateTarget);
        }

        public static string GetSourceText(this UIElement element, Column column)
        {
            var row = element.ExpectRow();
            return row.GetValue(column).ToString();
        }

        private static RowView ExpectRow(this UIElement element)
        {
            var result = element.GetRow();
            if (result == null)
                throw new InvalidOperationException(Strings.UIElement_ExpectRow);
            return result;
        }
    }
}

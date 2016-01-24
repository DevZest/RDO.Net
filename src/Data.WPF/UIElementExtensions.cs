using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public static class UIElementExtensions
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

        private static readonly DependencyProperty DataViewProperty = DependencyProperty.RegisterAttached(nameof(DataView),
            typeof(DataView), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static DataView GetDataView(this UIElement element)
        {
            return (DataView)element.GetValue(DataViewProperty);
        }

        internal static void SetDataView(this UIElement element, DataView value)
        {
            if (value == null)
            {
                Debug.Assert(element.GetDataView() != null);
                element.ClearValue(DataViewProperty);
            }
            else
            {
                Debug.Assert(element.GetDataView() == null);
                element.SetValue(DataViewProperty, value);
            }
        }

        private static readonly DependencyProperty RowViewProperty = DependencyProperty.RegisterAttached(nameof(RowView),
            typeof(RowView), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static RowView GetRowView(this UIElement element)
        {
            return (RowView)element.GetValue(RowViewProperty);
        }

        internal static void SetRowView(this UIElement element, RowView value)
        {
            if (value == null)
            {
                Debug.Assert(element.GetRowView() != null);
                element.ClearValue(RowViewProperty);
            }
            else
            {
                Debug.Assert(element.GetRowView() == null);
                element.SetValue(RowViewProperty, value);
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
            var result = element.GetRowView();
            if (result == null)
                throw new InvalidOperationException(Strings.UIElement_ExpectRow);
            return result;
        }
    }
}

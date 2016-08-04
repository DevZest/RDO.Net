using DevZest.Data.Windows.Controls;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public static class UIElementExtensions
    {
        private static readonly DependencyProperty TemplateItemProperty = DependencyProperty.RegisterAttached(nameof(TemplateItem),
            typeof(TemplateItem), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static TemplateItem GetTemplateItem(this UIElement element)
        {
            return (TemplateItem)element.GetValue(TemplateItemProperty);
        }

        public static DataPresenter GetDataPresenter(this UIElement element)
        {
            var templateItem = element.GetTemplateItem();
            return templateItem == null || templateItem.Template == null ? null : templateItem.Template.DataPresenter;
        }

        internal static void SetTemplateItem(this UIElement element, TemplateItem value)
        {
            Debug.Assert(value != null && element.GetTemplateItem() == null);
            element.SetValue(TemplateItemProperty, value);
        }

        private static readonly DependencyProperty BlockViewProperty = DependencyProperty.RegisterAttached(nameof(BlockView),
            typeof(BlockView), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static BlockView GetBlockView(this UIElement element)
        {
            return (BlockView)element.GetValue(BlockViewProperty);
        }

        internal static void SetBlockView(this UIElement element, BlockView value)
        {
            if (value == null)
            {
                Debug.Assert(element.GetBlockView() != null);
                element.ClearValue(BlockViewProperty);
            }
            else
            {
                Debug.Assert(element.GetBlockView() == null);
                element.SetValue(BlockViewProperty, value);
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
    }
}

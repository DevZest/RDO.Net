using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal static class UIElementExtensions
    {
        private static readonly DependencyProperty TemplateItemProperty = DependencyProperty.RegisterAttached(nameof(TemplateItem),
            typeof(TemplateItem), typeof(UIElementExtensions), new PropertyMetadata(null));

        internal static TemplateItem GetTemplateItem(this UIElement element)
        {
            return (TemplateItem)element.GetValue(TemplateItemProperty);
        }

        internal static void SetTemplateItem(this UIElement element, TemplateItem value)
        {
            Debug.Assert(value != null && element.GetTemplateItem() == null);
            element.SetValue(TemplateItemProperty, value);
        }

        private static readonly DependencyProperty BlockPresenterProperty = DependencyProperty.RegisterAttached(nameof(IBlockPresenter),
            typeof(IBlockPresenter), typeof(UIElementExtensions), new PropertyMetadata(null));

        internal static IBlockPresenter GetBlockPresenter(this UIElement element)
        {
            return (IBlockPresenter)element.GetValue(BlockPresenterProperty);
        }

        internal static void SetBlockPresenter(this UIElement element, IBlockPresenter value)
        {
            if (value == null)
            {
                Debug.Assert(element.GetBlockPresenter() != null);
                element.ClearValue(BlockPresenterProperty);
            }
            else
            {
                Debug.Assert(element.GetBlockPresenter() == null);
                element.SetValue(BlockPresenterProperty, value);
            }
        }

        private static readonly DependencyProperty RowPresenterProperty = DependencyProperty.RegisterAttached(nameof(RowPresenter),
            typeof(RowPresenter), typeof(UIElementExtensions), new PropertyMetadata(null));

        internal static RowPresenter GetRowPresenter(this UIElement element)
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

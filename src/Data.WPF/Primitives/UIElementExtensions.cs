using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public static class UIElementExtensions
    {
        private static readonly DependencyProperty BindingProperty = DependencyProperty.RegisterAttached(nameof(Binding),
            typeof(Binding), typeof(UIElementExtensions), new PropertyMetadata(null));

        public static Binding GetBinding(this UIElement element)
        {
            return (Binding)element.GetValue(BindingProperty);
        }

        public static DataPresenter GetDataPresenter(this UIElement element)
        {
            var binding = element.GetBinding();
            return binding == null || binding.Template == null ? null : binding.Template.DataPresenter;
        }

        internal static void SetBinding(this UIElement element, Binding value)
        {
            Debug.Assert(value != null && element.GetBinding() == null);
            element.SetValue(BindingProperty, value);
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
                element.ClearValue(RowPresenterProperty);
            else
                element.SetValue(RowPresenterProperty, value);
        }
    }
}

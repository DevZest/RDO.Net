using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace DevZest.Data.Tools
{
    internal class WatermarkAdorner : Adorner
    {
        private readonly ContentPresenter contentPresenter;

        public WatermarkAdorner(UIElement adornedElement, object watermark) :
           base(adornedElement)
        {
            this.IsHitTestVisible = false;

            this.contentPresenter = new ContentPresenter();
            this.contentPresenter.Content = watermark;
            this.contentPresenter.Opacity = 0.5;
            this.contentPresenter.Margin = new Thickness(Control.Margin.Left + Control.Padding.Left, Control.Margin.Top + Control.Padding.Top, 0, 0);

            // Hide the control adorner when the adorned element is hidden
            Binding binding = new Binding(nameof(UIElement.IsVisible));
            binding.Source = adornedElement;
            binding.Converter = new BooleanToVisibilityConverter();
            this.SetBinding(VisibilityProperty, binding);
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        private Control Control
        {
            get { return (Control)this.AdornedElement; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.contentPresenter;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // Here's the secret to getting the adorner to cover the whole control
            this.contentPresenter.Measure(Control.RenderSize);
            return Control.RenderSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }
    }
}

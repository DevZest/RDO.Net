using System.Windows;

namespace DevZest.Windows.Controls
{
    public sealed class Placeholder : UIElement
    {
        public double DesiredWidth { get; set; }

        public double DesiredHeight { get; set; }

        protected override Size MeasureCore(Size availableSize)
        {
            var width = availableSize.Width;
            if (double.IsPositiveInfinity(width))
                width = DesiredWidth;

            var height = availableSize.Height;
            if (double.IsPositiveInfinity(height))
                height = DesiredHeight;
            return new Size(width, height);
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    internal interface IScrollHandler
    {
        ScrollViewer ScrollOwner { get; set; }
        double ViewportWidth { get; }
        double ViewportHeight { get; }
        double ExtentWidth { get; }
        double ExtentHeight { get; }
        double HorizontalOffset { get; set; }
        double VerticalOffset { get; set; }
        Rect MakeVisible(Visual visual, Rect rectangle);
    }
}

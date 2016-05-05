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
        double ExtentHeight { get; }
        double ExtentWidth { get; }
        double ScrollOffsetX { get; set; }
        double ScrollOffsetY { get; set; }
        Rect MakeVisible(Visual visual, Rect rectangle);
    }
}

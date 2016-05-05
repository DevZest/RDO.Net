using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    internal interface IScrollHandler
    {
        ScrollViewer ScrollOwner { get; set; }
        double ViewportX { get; }
        double ViewportY { get; }
        double ExtentX { get; }
        double ExtentY { get; }
        double OffsetX { get; set; }
        double OffsetY { get; set; }
        Rect MakeVisible(Visual visual, Rect rectangle);
    }
}

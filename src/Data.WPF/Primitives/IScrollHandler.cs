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
        double HorizontalOffset { get; }
        double VerticalOffset { get; }
        double DeltaHorizontalOffset { get; set; }
        double DeltaVerticalOffset { get; set; }
        void SetHorizontalOffset(double offset);
        void SetVerticalOffset(double offset);
        Rect MakeVisible(Visual visual, Rect rectangle);
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Presenters.Primitives
{
    internal interface IScrollHandler : IScrollable
    {
        ScrollViewer ScrollOwner { get; set; }
        Rect MakeVisible(Visual visual, Rect rectangle);
    }
}

using System.Windows;
using System.Windows.Controls;

namespace DevZest
{
    internal static partial class UIElementExtensions
    {
        /// <summary>
        /// Render a UIElement such that the visual tree is generated, 
        /// without actually displaying the UIElement anywhere
        /// </summary>
        internal static void CreateVisualTree(this UIElement element)
        {
            var box = new Viewbox { Child = element };

            box.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            box.Arrange(new Rect(box.DesiredSize));
        }
    }
}

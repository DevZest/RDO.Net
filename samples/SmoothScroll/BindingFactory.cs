using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using DevZest.Windows;

namespace SmoothScroll
{
    internal static class BindingFactory
    {
        private static void Setup(TextBlock element, RowPresenter rowPresenter)
        {
            element.Margin = new Thickness(10, 0, 0, 0);
        }

        private static void Refresh(TextBlock element, Foo _, RowPresenter rowPresenter)
        {
            element.Text = rowPresenter.GetValue(_.Text);
            if (rowPresenter.GetValue(_.IsSectionHeader))
            {
                element.Foreground = Brushes.White;
                element.Background = Brushes.Black;
                element.Padding = new Thickness(0);
                element.TextWrapping = TextWrapping.NoWrap;
            }
            else
            {
                var r = rowPresenter.GetValue(_.BackgroundR);
                var g = rowPresenter.GetValue(_.BackgroundG);
                var b = rowPresenter.GetValue(_.BackgroundB);
                element.Foreground = Brushes.Black;
                element.Background = new SolidColorBrush(Color.FromArgb(255, r, g, b));
                element.Padding = new Thickness(10);
                element.TextWrapping = TextWrapping.Wrap;
            }
        }

        public static RowBinding<TextBlock> TextBlock(this Foo _)
        {
            return new RowBinding<TextBlock>(Setup, (e, r) => Refresh(e, _, r), null);
        }
    }
}

using DevZest.Data.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Media;
using System.Windows;
using DevZest.Data;

namespace SmoothScroll
{
    internal static class BindingFactory
    {
        private static void Refresh(TextBlock element, Foo _, RowPresenter rowPresenter)
        {
            element.Text = rowPresenter.GetValue(_.Text);
            if (rowPresenter.GetValue(_.IsSectionHeader).Value)
            {
                element.Foreground = Brushes.White;
                element.Background = Brushes.Black;
                element.Padding = new Thickness(0);
                element.TextWrapping = TextWrapping.NoWrap;
            }
            else
            {
                var r = rowPresenter.GetValue(_.BackgroundR).Value;
                var g = rowPresenter.GetValue(_.BackgroundG).Value;
                var b = rowPresenter.GetValue(_.BackgroundB).Value;
                element.Foreground = Brushes.Black;
                element.Background = new SolidColorBrush(Color.FromArgb(255, r, g, b));
                element.Padding = new Thickness(10);
                element.TextWrapping = TextWrapping.Wrap;
            }
        }

        public static RowBinding<TextBlock> BindTextBlock(this Foo _)
        {
            return new RowBinding<TextBlock>((e, r) => Refresh(e, _, r));
        }

        private static void Setup(Border element, RowPresenter rowPresenter)
        {
            element.BorderBrush = Brushes.White;
        }

        private static void Refresh(Border element, RowPresenter rowPresenter)
        {
            var thickness = rowPresenter.View.IsKeyboardFocusWithin ? 2 : (rowPresenter.IsCurrent ? 1 : 0);
            element.BorderThickness = new Thickness(thickness);
        }

        public static RowBinding<Border> BindBorder(this Model _)
        {
            return new RowBinding<Border>(Refresh, Setup, null);
        }
    }
}

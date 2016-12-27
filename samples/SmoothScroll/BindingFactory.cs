using DevZest.Data.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Media;
using System.Windows;

namespace SmoothScroll
{
    internal static class BindingFactory
    {
        private sealed class FooToTextBlockRowBinding : RowBinding<TextBlock>
        {
            private readonly Foo _;

            public FooToTextBlockRowBinding(Foo _)
            {
                this._ = _;
            }

            protected override void Setup(TextBlock element, RowPresenter rowPresenter)
            {
            }

            protected override void Refresh(TextBlock element, RowPresenter rowPresenter)
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

            protected override void Cleanup(TextBlock element, RowPresenter rowPresenter)
            {
            }
        }

        public static RowBinding<TextBlock> BindTextBlock(this Foo _)
        {
            return new FooToTextBlockRowBinding(_);
        }
    }
}

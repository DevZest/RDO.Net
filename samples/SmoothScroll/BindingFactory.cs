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
        private sealed class FooToTextBlockRowBinding : RowBinding<TextBlock>
        {
            private readonly Foo _;

            public FooToTextBlockRowBinding(Foo _)
            {
                this._ = _;
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

        private sealed class BorderRowBinding : RowBinding<Border>
        {
            public static readonly BorderRowBinding Singleton = new BorderRowBinding();

            private BorderRowBinding()
            {
            }

            protected override void Setup(Border element, RowPresenter rowPresenter)
            {
                element.BorderBrush = Brushes.White;
            }

            protected override void Refresh(Border element, RowPresenter rowPresenter)
            {
                var thickness = rowPresenter.View.IsKeyboardFocusWithin ? 2 : (rowPresenter.IsCurrent ? 1 : 0);
                element.BorderThickness = new Thickness(thickness);
            }
        }

        public static RowBinding<Border> BindBorder(this Model _)
        {
            return BorderRowBinding.Singleton;
        }
    }
}

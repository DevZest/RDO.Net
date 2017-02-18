using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    internal static class BindingFactory
    {
        public static BlockBinding<BlockHeader> BlockHeader(this Model _)
        {
            return new BlockBinding<BlockHeader>(onRefresh: (element, blockOrdinal, rows) =>
            {
                element.Text = blockOrdinal.ToString();
            });
        }

        public static BlockBinding<BlockLabel> BlockLabel<T>(this Column source, BlockBinding<T> target)
            where T : UIElement, new()
        {
            return new BlockBinding<BlockLabel>(
                onSetup: (e, i, r) =>
                {
                    e.Content = source.DisplayName;
                    if (target != null)
                        e.Target = target.SettingUpElement;
                },
                onRefresh: (e, i, r) =>
                {
                },
                onCleanup: (e, i, r) =>
                {
                });
        }

        public static BlockBinding<Placeholder> BlockPlaceholder(this Model _, double desiredWidth = 0, double desiredHeight = 0)
        {
            return new BlockBinding<Placeholder>(null, (e, o, r) => Setup(e, desiredWidth, desiredHeight), null);
        }

        private static void Setup(Placeholder element, double desiredWidth, double desiredHeight)
        {
            element.DesiredWidth = desiredWidth;
            element.DesiredHeight = desiredHeight;
        }

        public static ScalarBinding<Label> ScalarLabel<TTarget>(this Column source, ScalarBinding<TTarget> target = null, string format = null, IFormatProvider formatProvider = null)
            where TTarget : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<Label>(
                onSetup: e =>
                {
                    e.Content = source.DisplayName.ToString(format, formatProvider);
                    if (target != null)
                        e.Target = target.SettingUpElement;
                },
                onRefresh: e =>
                {
                },
                onCleanup: e =>
                {
                });
        }

        public static ScalarBinding<Placeholder> ScalarPlaceholder(this Model _, double desiredWidth = 0, double desiredHeight = 0)
        {
            return new ScalarBinding<Placeholder>(null, e => Setup(e, desiredWidth, desiredHeight), null);
        }

        public static RowBinding<Placeholder> RowPlaceholder(this Model _, Action<Placeholder, RowPresenter> onRefresh = null)
        {
            return new RowBinding<Placeholder>(onRefresh);
        }

        private static void Refresh(Placeholder element, RowPresenter rowPresenter, double desiredWidth, double desiredHeight, Action<Placeholder, RowPresenter> onRefresh)
        {
            element.DesiredWidth = desiredWidth;
            element.DesiredHeight = desiredHeight;
            if (onRefresh != null)
                onRefresh(element, rowPresenter);
        }

        public static RowBinding<Placeholder> RowPlaceholder(this Model _, double desiredWidth, double desiredHeight, Action<Placeholder, RowPresenter> onRefresh = null)
        {
            return new RowBinding<Placeholder>((e, r) => Refresh(e, r, desiredWidth, desiredHeight, onRefresh));
        }
    }
}

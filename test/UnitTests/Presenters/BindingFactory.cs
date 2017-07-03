using DevZest.Data;
using DevZest.Data.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    internal static class BindingFactory
    {
        public static BlockBinding<BlockHeader> BlockHeader(this Model _)
        {
            return new BlockBinding<BlockHeader>(onRefresh: (e, bp) =>
            {
                e.Text = bp.Ordinal.ToString();
            });
        }

        public static BlockBinding<BlockLabel> BlockLabel<T>(this Column source, BlockBinding<T> target)
            where T : UIElement, new()
        {
            return new BlockBinding<BlockLabel>(
                onSetup: (e, bp) =>
                {
                    e.Content = source.DisplayName;
                    if (target != null)
                        e.Target = target.SettingUpElement;
                },
                onRefresh: (e, bp) =>
                {
                },
                onCleanup: (e, bp) =>
                {
                });
        }

        public static BlockBinding<Placeholder> BlockPlaceholder(this Model _, double desiredWidth = 0, double desiredHeight = 0)
        {
            return new BlockBinding<Placeholder>(null, (e, bp) => Setup(e, desiredWidth, desiredHeight), null);
        }

        private static void Setup(Placeholder element, double desiredWidth, double desiredHeight)
        {
            element.DesiredWidth = desiredWidth;
            element.DesiredHeight = desiredHeight;
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

        public static ScalarBinding<Label> AsFlowRepeatableScalarLabel<TTarget>(this Column source, ScalarBinding<TTarget> target = null, string format = null, IFormatProvider formatProvider = null)
            where TTarget : UIElement, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ScalarBinding<Label>(
                onSetup: (e, sp) =>
                {
                    e.Content = string.Format("{0}. {1}", sp.FlowIndex, source.DisplayName.ToString(format, formatProvider));
                    if (target != null)
                        e.Target = target.SettingUpElement;
                },
                onRefresh: null,
                onCleanup: null).WithFlowRepeatable(true);
        }

    }
}

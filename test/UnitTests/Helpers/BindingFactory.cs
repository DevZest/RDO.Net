using DevZest.Data.Windows.Helpers;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Helpers
{
    internal static class BindingFactory
    {
        private static void RefreshDisplayName(TextBlock element, Column column)
        {
            element.Text = column.DisplayName;
        }

        public static ScalarBinding<TextBlock> BindDisplayNameToTextBlock(this Column column)
        {
            return new ScalarBinding<TextBlock>(e => RefreshDisplayName(e, column));
        }

        public static BlockBinding<BlockHeader> BindBlockOrdinalToTextBlock(this Model _)
        {
            return new BlockBinding<BlockHeader>(onRefresh: (element, blockOrdinal, rows) =>
            {
                element.Text = blockOrdinal.ToString();
            });
        }

        public static BlockBinding<Placeholder> BindToBlockPlaceholder(this Model _, double desiredWidth = 0, double desiredHeight = 0)
        {
            return new BlockBinding<Placeholder>(null, (e, o, r) => Setup(e, desiredWidth, desiredHeight), null);
        }

        private static void Setup(Placeholder element, double desiredWidth, double desiredHeight)
        {
            element.DesiredWidth = desiredWidth;
            element.DesiredHeight = desiredHeight;
        }

        public static ScalarBinding<Placeholder> BindToScalarPlaceholder(this Model _, double desiredWidth = 0, double desiredHeight = 0)
        {
            return new ScalarBinding<Placeholder>(null, e => Setup(e, desiredWidth, desiredHeight), null);
        }

        public static RowBinding<Placeholder> BindToRowPlaceholder(this Model _, Action<Placeholder, RowPresenter> onRefresh = null)
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

        public static RowBinding<Placeholder> BindToRowPlaceholder(this Model _, double desiredWidth, double desiredHeight, Action<Placeholder, RowPresenter> onRefresh = null)
        {
            return new RowBinding<Placeholder>((e, r) => Refresh(e, r, desiredWidth, desiredHeight, onRefresh));
        }
    }
}

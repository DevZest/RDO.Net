using DevZest.Data.Windows.Helpers;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    internal static class BindingFactory
    {
        private static void RefreshIsEditing(TextBlock element, RowPresenter rowPresenter)
        {
            element.Text = rowPresenter.IsEditing.ToString();
        }

        public static RowBinding<TextBlock> BindIsEditingToTextBlock(this Model _)
        {
            return new RowBinding<TextBlock>(RefreshIsEditing);
        }

        private static void RefreshIsCurrent(TextBlock element, RowPresenter rowPresenter)
        {
            element.Text = rowPresenter.IsCurrent.ToString();
        }

        public static RowBinding<TextBlock> BindIsCurrentToTextBlock(this Model _)
        {
            return new RowBinding<TextBlock>(RefreshIsCurrent);
        }

        private static void RefreshDisplayName(TextBlock element, Column column)
        {
            element.Text = column.DisplayName;
        }

        private sealed class DisplayNameToTextBlockBinding : ScalarBinding<TextBlock>
        {
            private Column _column;

            public DisplayNameToTextBlockBinding(Column column)
            {
                _column = column;
            }

            protected internal override void Refresh(TextBlock element)
            {
                element.Text = _column.DisplayName;
            }
        }

        public static ScalarBinding<TextBlock> BindDisplayNameToTextBlock(this Column column)
        {
            return new DisplayNameToTextBlockBinding(column);
        }

        private static void Refresh(TextBlock element, _String column, RowPresenter rowPresenter)
        {
            element.Text = rowPresenter.GetValue(column);
        }

        public static RowBinding<TextBlock> BindToTextBlock(this _String column)
        {
            return new RowBinding<TextBlock>((e, r) => Refresh(e, column, r));
        }

        private sealed class BlockOrdinalToTextBlockBinding : BlockBinding<TextBlock>
        {
            protected override void Refresh(TextBlock element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
            {
                element.Text = blockOrdinal.ToString();
            }
        }

        public static BlockBinding<TextBlock> BindBlockOrdinalToTextBlock(this Model _)
        {
            return new BlockOrdinalToTextBlockBinding();
        }

        private class PlaceholderBlockBinding : BlockBinding<Placeholder>
        {
            public PlaceholderBlockBinding(double desiredWidth, double desiredHeight)
            {
                _desiredWidth = desiredWidth;
                _desiredHeight = desiredHeight;
            }

            private double _desiredWidth;
            private double _desiredHeight;

            protected override void Setup(Placeholder element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
            {
                element.DesiredWidth = _desiredWidth;
                element.DesiredHeight = _desiredHeight;
            }

            protected sealed override void Refresh(Placeholder element, int blockOrdinal, IReadOnlyList<RowPresenter> rows)
            {
            }
        }

        public static BlockBinding<Placeholder> BindToBlockPlaceholder(this Model _, double desiredWidth = 0, double desiredHeight = 0)
        {
            return new PlaceholderBlockBinding(desiredWidth, desiredHeight);
        }

        private class PlaceholderScalarBinding : ScalarBinding<Placeholder>
        {
            public PlaceholderScalarBinding(double desiredWidth, double desiredHeight)
            {
                _desiredWidth = desiredWidth;
                _desiredHeight = desiredHeight;
            }

            private double _desiredWidth;
            private double _desiredHeight;

            protected override void Setup(Placeholder element)
            {
                element.DesiredWidth = _desiredWidth;
                element.DesiredHeight = _desiredHeight;
            }

            protected internal override void Refresh(Placeholder element)
            {
            }
        }

        public static ScalarBinding<Placeholder> BindToScalarPlaceholder(this Model _, double desiredWidth = 0, double desiredHeight = 0)
        {
            return new PlaceholderScalarBinding(desiredWidth, desiredHeight);
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

using DevZest.Data.Windows.Helpers;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    internal static class BindingFactory
    {
        private sealed class IsEditingToTextBlockBinding : RowBinding<TextBlock>
        {
            protected override void Refresh(TextBlock element, RowPresenter rowPresenter)
            {
                element.Text = rowPresenter.IsEditing.ToString();
            }
        }

        public static RowBinding<TextBlock> BindIsEditingToTextBlock(this Model _)
        {
            return new IsEditingToTextBlockBinding();
        }

        private sealed class IsCurrentTextBlockBinding : RowBinding<TextBlock>
        {
            protected override void Refresh(TextBlock element, RowPresenter rowPresenter)
            {
                element.Text = rowPresenter.IsCurrent.ToString();
            }
        }

        public static RowBinding<TextBlock> BindIsCurrentToTextBlock(this Model _)
        {
            return new IsCurrentTextBlockBinding();
        }

        private sealed class DisplayNameToTextBlockBinding : ScalarBinding<TextBlock>
        {
            private Column _column;

            public DisplayNameToTextBlockBinding(Column column)
            {
                _column = column;
            }

            protected override void Refresh(TextBlock element)
            {
                element.Text = _column.DisplayName;
            }
        }

        public static ScalarBinding<TextBlock> BindDisplayNameToTextBlock(this Column column)
        {
            return new DisplayNameToTextBlockBinding(column);
        }

        private sealed class StringToTextBlockBinding : RowBinding<TextBlock>
        {
            private _String _stringColumn;

            public StringToTextBlockBinding(_String stringColumn)
            {
                _stringColumn = stringColumn;
            }

            protected override void Refresh(TextBlock element, RowPresenter rowPresenter)
            {
                element.Text = rowPresenter.GetValue(_stringColumn);
            }
        }

        public static RowBinding<TextBlock> BindToTextBlock(this _String stringColumn)
        {
            return new StringToTextBlockBinding(stringColumn);
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

            protected override void Refresh(Placeholder element)
            {
            }
        }

        public static ScalarBinding<Placeholder> BindToScalarPlaceholder(this Model _, double desiredWidth = 0, double desiredHeight = 0)
        {
            return new PlaceholderScalarBinding(desiredWidth, desiredHeight);
        }

        private sealed class PlaceholderRowBinding : RowBinding<Placeholder>
        {
            public PlaceholderRowBinding()
            {
            }

            public PlaceholderRowBinding(double desiredWidth, double desiredHeight)
            {
                _desiredWidth = desiredWidth;
                _desiredHeight = desiredHeight;
            }

            private double _desiredWidth;
            private double _desiredHeight;

            protected override void Setup(Placeholder element, RowPresenter rowPresenter)
            {
                element.DesiredWidth = _desiredWidth;
                element.DesiredHeight = _desiredHeight;
            }

            private Action<Placeholder, RowPresenter> _onRefresh;
            public Action<Placeholder, RowPresenter> OnRefresh
            {
                get { return _onRefresh; }
                set
                {
                    VerifyNotSealed();
                    _onRefresh = value;
                }
            }

            public PlaceholderRowBinding WithOnRefresh(Action<Placeholder, RowPresenter> onRefresh)
            {
                OnRefresh = onRefresh;
                return this;
            }

            protected override void Refresh(Placeholder element, RowPresenter rowPresenter)
            {
                if (OnRefresh != null)
                    OnRefresh(element, rowPresenter);
            }
        }

        public static RowBinding<Placeholder> BindToRowPlaceholder(this Model _, Action<Placeholder, RowPresenter> onRefresh = null)
        {
            return new PlaceholderRowBinding().WithOnRefresh(onRefresh);
        }

        public static RowBinding<Placeholder> BindToRowPlaceholder(this Model _, double desiredWidth, double desiredHeight, Action<Placeholder, RowPresenter> onRefresh = null)
        {
            return new PlaceholderRowBinding(desiredWidth, desiredHeight).WithOnRefresh(onRefresh);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class BlockBinding : Binding, IConcatList<BlockBinding>
    {
        #region IConcatList<BlockBinding>

        int IReadOnlyCollection<BlockBinding>.Count
        {
            get { return 1; }
        }

        BlockBinding IReadOnlyList<BlockBinding>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        void IConcatList<BlockBinding>.Sort(Comparison<BlockBinding> comparision)
        {
        }

        IEnumerator<BlockBinding> IEnumerable<BlockBinding>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        #endregion

        public sealed class Builder<T> : Binding.Builder<T, BlockBinding, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(TemplateBuilder templateBuilder)
                : base(templateBuilder, BlockBinding.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override void AddItem(Template template, GridRange gridRange, BlockBinding item)
            {
                template.AddBinding(gridRange, item);
            }

            public Builder<T> OnSetup(Action<T, int, IReadOnlyList<RowPresenter>> onSetup)
            {
                if (onSetup == null)
                    throw new ArgumentNullException(nameof(onSetup));
                TemplateItem.InitOnSetup(onSetup);
                return This;
            }

            public Builder<T> OnCleanup(Action<T, int, IReadOnlyList<RowPresenter>> onCleanup)
            {
                if (onCleanup == null)
                    throw new ArgumentNullException(nameof(onCleanup));
                TemplateItem.InitOnCleanup(onCleanup);
                return This;
            }

            public Builder<T> OnRefresh(Action<T, int, IReadOnlyList<RowPresenter>> onRefresh)
            {
                if (onRefresh == null)
                    throw new ArgumentNullException(nameof(onRefresh));
                TemplateItem.InitOnRefresh(onRefresh);
                return This;
            }
        }

        internal static BlockBinding Create<T>()
            where T : UIElement, new()
        {
            return new BlockBinding(() => new T());
        }

        private BlockBinding(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        internal UIElement Setup(BlockView blockView)
        {
            Debug.Assert(blockView != null);
            return Setup(x => OnSetup(x, blockView));
        }

        private Action<UIElement, int, IReadOnlyList<RowPresenter>> _onSetup;
        private void InitOnSetup<T>(Action<T, int, IReadOnlyList<RowPresenter>> onSetup)
            where T : UIElement
        {
            Debug.Assert(onSetup != null);
            _onSetup = (element, ordinal, rows) => onSetup((T)element, ordinal, rows);
        }

        private void OnSetup(UIElement element, BlockView blockView)
        {
            element.SetBlockView(blockView);
            if (_onSetup != null)
                _onSetup(element, blockView.Ordinal, blockView);
        }

        private Action<UIElement, int, IReadOnlyList<RowPresenter>> _onCleanup;
        private void InitOnCleanup<T>(Action<T, int, IReadOnlyList<RowPresenter>> onCleanup)
            where T : UIElement
        {
            Debug.Assert(onCleanup != null);
            _onCleanup = (element, ordinal, rows) => onCleanup((T)element, ordinal, rows);
        }

        protected override void OnCleanup(UIElement element)
        {
            if (_onCleanup != null)
            {
                var blockView = element.GetBlockView();
                _onCleanup(element, blockView.Ordinal, blockView);
            }
            element.SetBlockView(null);
        }

        private Action<UIElement, int, IReadOnlyList<RowPresenter>> _onRefresh;
        private void InitOnRefresh<T>(Action<T, int, IReadOnlyList<RowPresenter>> onRefresh)
            where T : UIElement
        {
            Debug.Assert(onRefresh != null);
            _onRefresh = (element, ordinal, rows) => onRefresh((T)element, ordinal, rows);
        }

        internal sealed override void Refresh(UIElement element)
        {
            if (_onRefresh != null)
            {
                var blockView = element.GetBlockView();
                _onRefresh(element, blockView.Ordinal, blockView);
            }
        }

        internal override void VerifyRowRange(GridRange rowRange)
        {
            if (GridRange.IntersectsWith(rowRange))
                throw new InvalidOperationException(Strings.BlockBinding_IntersectsWithRowRange(Ordinal));

            if (!Template.Orientation.HasValue)
                throw new InvalidOperationException(Strings.BlockBinding_NullOrientation);

            var orientation = Template.Orientation.GetValueOrDefault();
            if (orientation == Orientation.Horizontal)
            {
                if (!rowRange.Contains(GridRange.Left) || !rowRange.Contains(GridRange.Right))
                    throw new InvalidOperationException(Strings.BlockBinding_OutOfHorizontalRowRange(Ordinal));
            }
            else
            {
                Debug.Assert(orientation == Orientation.Vertical);
                if (!rowRange.Contains(GridRange.Top) || !rowRange.Contains(GridRange.Bottom))
                    throw new InvalidOperationException(Strings.BlockBinding_OutOfVerticalRowRange(Ordinal));
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class BlockItem : TemplateItem, IConcatList<BlockItem>
    {
        #region IConcatList<BlockItem>

        int IReadOnlyCollection<BlockItem>.Count
        {
            get { return 1; }
        }

        BlockItem IReadOnlyList<BlockItem>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        void IConcatList<BlockItem>.Sort(Comparison<BlockItem> comparision)
        {
        }

        IEnumerator<BlockItem> IEnumerable<BlockItem>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        #endregion

        public sealed class Builder<T> : TemplateItem.Builder<T, BlockItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(TemplateBuilder templateBuilder)
                : base(templateBuilder, BlockItem.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override void AddItem(Template template, GridRange gridRange, BlockItem item)
            {
                template.AddBlockItem(gridRange, item);
            }

            public Builder<T> OnMount(Action<T, IBlockPresenter> onMount)
            {
                if (onMount == null)
                    throw new ArgumentNullException(nameof(onMount));
                TemplateItem.OnMount(onMount);
                return This;
            }

            public Builder<T> OnUnmount(Action<T, IBlockPresenter> onUnmount)
            {
                if (onUnmount == null)
                    throw new ArgumentNullException(nameof(onUnmount));
                TemplateItem.OnUnmount(onUnmount);
                return This;
            }

            public Builder<T> OnRefresh(Action<T, IBlockPresenter> onRefresh)
            {
                if (onRefresh == null)
                    throw new ArgumentNullException(nameof(onRefresh));
                TemplateItem.OnRefresh(onRefresh);
                return This;
            }
        }

        internal static BlockItem Create<T>()
            where T : UIElement, new()
        {
            return new BlockItem(() => new T());
        }

        private BlockItem(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        internal UIElement Mount(IBlockPresenter blockPresenter, Action<UIElement> initializer)
        {
            Debug.Assert(blockPresenter != null);
            return base.Mount(x => x.SetBlockPresenter(blockPresenter), initializer);
        }

        protected sealed override void OnMount(UIElement element)
        {
            if (_onMount != null)
                _onMount(element, element.GetBlockPresenter());
        }

        protected override void Cleanup(UIElement element)
        {
            element.SetBlockPresenter(null);
        }

        private Action<UIElement, IBlockPresenter> _onMount;
        private void OnMount<T>(Action<T, IBlockPresenter> onMount)
            where T : UIElement
        {
            Debug.Assert(onMount != null);
            _onMount = (element, blockPresenter) => onMount((T)element, blockPresenter);
        }

        protected sealed override void OnUnmount(UIElement element)
        {
            if (_onMount != null)
                _onMount(element, element.GetBlockPresenter());
        }

        private Action<UIElement, IBlockPresenter> _onUnmount;
        private void OnUnmount<T>(Action<T, IBlockPresenter> onUnmount)
            where T : UIElement
        {
            Debug.Assert(onUnmount != null);
            _onUnmount = (element, blockPresenter) => onUnmount((T)element, blockPresenter);
        }

        internal sealed override void Refresh(UIElement element)
        {
            if (_onRefresh != null)
                _onRefresh(element, element.GetBlockPresenter());
        }

        private Action<UIElement, IBlockPresenter> _onRefresh;
        private void OnRefresh<T>(Action<T, IBlockPresenter> onRefresh)
            where T : UIElement
        {
            Debug.Assert(onRefresh != null);
            _onRefresh = (element, blockPresenter) => onRefresh((T)element, blockPresenter);
        }

        internal override void VerifyRowRange(GridRange rowRange)
        {
            if (GridRange.IntersectsWith(rowRange))
                throw new InvalidOperationException(Strings.BlockItem_IntersectsWithRowRange(Ordinal));

            if (!Template.Orientation.HasValue)
                throw new InvalidOperationException(Strings.BlockItem_NullOrientation);

            var orientation = Template.Orientation.GetValueOrDefault();
            if (orientation == Orientation.Horizontal)
            {
                if (!rowRange.Contains(GridRange.Left) || !rowRange.Contains(GridRange.Right))
                    throw new InvalidOperationException(Strings.BlockItem_OutOfHorizontalRowRange(Ordinal));
            }
            else
            {
                Debug.Assert(orientation == Orientation.Vertical);
                if (!rowRange.Contains(GridRange.Top) || !rowRange.Contains(GridRange.Bottom))
                    throw new InvalidOperationException(Strings.ScalarItem_OutOfVerticalRowRange(Ordinal));
            }
        }
    }
}

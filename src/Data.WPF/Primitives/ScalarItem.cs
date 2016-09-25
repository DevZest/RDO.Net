using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class ScalarItem : TemplateItem, IConcatList<ScalarItem>
    {
        #region IConcatList<ScalarItem>

        int IReadOnlyCollection<ScalarItem>.Count
        {
            get { return 1; }
        }

        ScalarItem IReadOnlyList<ScalarItem>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        void IConcatList<ScalarItem>.Sort(Comparison<ScalarItem> comparision)
        {
        }

        IEnumerator<ScalarItem> IEnumerable<ScalarItem>.GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        #endregion

        public sealed class Builder<T> : TemplateItem.Builder<T, ScalarItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(TemplateBuilder templateBuilder, bool isMultidimensional = false)
                : base(templateBuilder, ScalarItem.Create<T>())
            {
                TemplateItem.IsMultidimensional = isMultidimensional;
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override void AddItem(Template template, GridRange gridRange, ScalarItem item)
            {
                template.AddScalarItem(gridRange, item);
            }

            public Builder<T> OnSetup(Action<T> onSetup)
            {
                if (onSetup == null)
                    throw new ArgumentNullException(nameof(onSetup));
                TemplateItem.InitOnSetup(onSetup);
                return This;
            }

            public Builder<T> OnCleanup(Action<T> onCleanup)
            {
                if (onCleanup == null)
                    throw new ArgumentNullException(nameof(onCleanup));
                TemplateItem.InitOnCleanup(onCleanup);
                return This;
            }

            public Builder<T> OnRefresh(Action<T> onRefresh)
            {
                if (onRefresh == null)
                    throw new ArgumentNullException(nameof(onRefresh));
                TemplateItem.InitOnRefresh(onRefresh);
                return This;
            }
        }

        internal static ScalarItem Create<T>()
            where T : UIElement, new()
        {
            return new ScalarItem(() => new T());
        }

        private ScalarItem(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        private Action<UIElement> _onSetup;
        private void InitOnSetup<T>(Action<T> onSetup)
            where T : UIElement
        {
            Debug.Assert(onSetup != null);
            _onSetup = element => onSetup((T)element);
        }

        private void OnSetup(UIElement element)
        {
            if (_onSetup != null)
                _onSetup(element);
        }

        internal UIElement Setup()
        {
            return Setup(x => OnSetup(x));
        }

        private Action<UIElement> _onCleanup;
        private void InitOnCleanup<T>(Action<T> onCleanup)
            where T : UIElement
        {
            Debug.Assert(onCleanup != null);
            _onCleanup = element => onCleanup((T)element);
        }

        protected override void OnCleanup(UIElement element)
        {
            if (_onCleanup != null)
                _onCleanup(element);
        }

        private Action<UIElement> _onRefresh;
        private void InitOnRefresh<T>(Action<T> onRefresh)
            where T : UIElement
        {
            Debug.Assert(onRefresh != null);
            _onRefresh = element => onRefresh((T)element);
        }

        internal sealed override void Refresh(UIElement element)
        {
            if (_onRefresh != null)
                _onRefresh(element);
        }

        public bool IsMultidimensional { get; private set; }

        internal int CumulativeBlockDimensionsDelta { get; set; }

        internal override void VerifyRowRange(GridRange rowRange)
        {
            if (GridRange.IntersectsWith(rowRange))
                throw new InvalidOperationException(Strings.ScalarItem_IntersectsWithRowRange(Ordinal));

            if (!IsMultidimensional)
                return;

            if (Template.IsMultidimensional(Orientation.Horizontal))
            {
                if (!rowRange.Contains(GridRange.Left) || !rowRange.Contains(GridRange.Right))
                    throw new InvalidOperationException(Strings.ScalarItem_OutOfHorizontalRowRange(Ordinal));
            }
            else if (Template.IsMultidimensional(Orientation.Vertical))
            {
                if (!rowRange.Contains(GridRange.Top) || !rowRange.Contains(GridRange.Bottom))
                    throw new InvalidOperationException(Strings.ScalarItem_OutOfVerticalRowRange(Ordinal));
            }
            else
                throw new InvalidOperationException(Strings.ScalarItem_OneDimensionalTemplate(Ordinal));
        }

        private ElementManager ElementManager
        {
            get { return Template.ElementManager; }
        }

        public int BlockDimensions
        {
            get { return IsMultidimensional ? ElementManager.BlockDimensions : 1; }
        }

        public UIElement this[int blockDimension]
        {
            get
            {
                if (blockDimension < 0 || blockDimension >= BlockDimensions)
                    throw new ArgumentOutOfRangeException(nameof(blockDimension));

                var ordinal = Ordinal;
                int prevCumulativeBlockDimensionsDelta = ordinal == 0 ? 0 : Template.ScalarItems[ordinal - 1].CumulativeBlockDimensionsDelta;
                var elementIndex = ordinal * BlockDimensions - prevCumulativeBlockDimensionsDelta + blockDimension;
                if (ordinal >= Template.ScalarItemsSplit)
                    elementIndex += ElementManager.BlockViewList.Count;
                return ElementManager.Elements[elementIndex];
            }
        }

        internal override void VerifyFrozenMargins(string templateItemsName)
        {
            base.VerifyFrozenMargins(templateItemsName);
            if (LayoutOrientation == Orientation.Horizontal)
                VerifyHorizontalStretches();
            else
                VerifyVerticalStretches();
        }

        private Orientation LayoutOrientation
        {
            get { return Template.Orientation.Value; }
        }

        private void VerifyHorizontalStretches()
        {
            if (GridRange.HorizontallyIntersectsWith(Template.GridColumns.Count - Template.Stretches))
                throw new InvalidOperationException(Strings.ScalarItem_InvalidStretches(Ordinal));
        }

        private void VerifyVerticalStretches()
        {
            if (GridRange.VerticallyIntersectsWith(Template.GridRows.Count - Template.Stretches))
                throw new InvalidOperationException(Strings.ScalarItem_InvalidStretches(Ordinal));
        }

        internal override AutoSizeWaiver CoercedAutoSizeWaiver
        {
            get
            {
                var result = base.CoercedAutoSizeWaiver;
                if (!Template.Orientation.HasValue)
                    return result;

                if (LayoutOrientation == Orientation.Horizontal)
                {
                    if (Template.RowRange.ColumnSpan.IntersectsWith(GridRange.ColumnSpan))
                        result |= AutoSizeWaiver.Width;
                }
                else
                {
                    if (Template.RowRange.RowSpan.IntersectsWith(GridRange.RowSpan))
                        result |= AutoSizeWaiver.Height;
                }

                return result;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class ScalarItem : TemplateItem, IConcatList<ScalarItem>
    {
        #region IConcatList<ScalarItem>

        bool IConcatList<ScalarItem>.IsReadOnly
        {
            get { return true; }
        }

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

        private sealed class Binding : BindingBase
        {
            internal static Binding Bind<T>(TemplateItem templateItem, Action<DataPresenter, T> updateTarget)
                where T : UIElement
            {
                return new Binding(templateItem, (source, element) => updateTarget(source, (T)element), null, null);
            }

            internal static Binding BindToSource<T>(TemplateItem templateItem, Action<T, DataPresenter> updateSource, BindingTrigger[] triggers)
                where T : UIElement
            {
                return new Binding(templateItem, null, (element, source) => updateSource((T)element, source), triggers);
            }

            internal static Binding BindTwoWay<T>(TemplateItem templateItem, Action<DataPresenter, T> updateTarget, Action<T, DataPresenter> updateSource, BindingTrigger[] triggers)
                where T : UIElement
            {
                return new Binding(templateItem, (source, element) => updateTarget(source, (T)element), (element, source) => updateSource((T)element, source), triggers);
            }

            private Binding(TemplateItem templateItem, Action<DataPresenter, UIElement> updateTarget, Action<UIElement, DataPresenter> updateSource, BindingTrigger[] triggers)
                : base(templateItem, triggers)
            {
                _updateTargetAction = updateTarget;
                _updateSourceAction = updateSource;
            }

            private Action<DataPresenter, UIElement> _updateTargetAction;

            private Action<UIElement, DataPresenter> _updateSourceAction;

            public override void UpdateTarget(BindingContext bindingContext, UIElement element)
            {
                if (_updateTargetAction != null)
                    _updateTargetAction(bindingContext.DataPresenter, element);
            }

            public override void UpdateSource(BindingContext bindingContext, UIElement element)
            {
                if (_updateSourceAction != null)
                    _updateSourceAction(element, bindingContext.DataPresenter);
            }
        }

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

            public Builder<T> Bind(Action<DataPresenter, T> updateTarget)
            {
                TemplateItem.AddBinding(Binding.Bind(TemplateItem, updateTarget));
                return This;
            }

            public Builder<T> BindToSource(Action<T, DataPresenter> updateSource, params BindingTrigger[] triggers)
            {
                TemplateItem.AddBinding(Binding.BindToSource(TemplateItem, updateSource, triggers));
                return This;
            }

            public Builder<T> BindTwoWay(Action<DataPresenter, T> updateTarget, Action<T, DataPresenter> updateSource, params BindingTrigger[] triggers)
            {
                TemplateItem.AddBinding(Binding.BindTwoWay(TemplateItem, updateTarget, updateSource, triggers));
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
                    elementIndex += ElementManager.Blocks.Count;
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
            if (GridRange.ContainsHorizontal(Template.GridColumns.Count - Template.Stretches))
                throw new InvalidOperationException(Strings.ScalarItem_InvalidStretches(Ordinal));
        }

        private void VerifyVerticalStretches()
        {
            if (GridRange.ContainsVertical(Template.GridRows.Count - Template.Stretches))
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

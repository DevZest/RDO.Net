using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class DataItem : TemplateItem
    {
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

        public sealed class Builder<T> : TemplateItem.Builder<T, DataItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(TemplateItemBuilderFactory builderFactory, bool isMultidimensional = false)
                : base(builderFactory, DataItem.Create<T>())
            {
                Item.IsMultidimensional = isMultidimensional;
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override void AddItem(Template template, GridRange gridRange, DataItem item)
            {
                template.AddDataItem(gridRange, item);
            }

            public Builder<T> Bind(Action<DataPresenter, T> updateTarget)
            {
                Item.AddBinding(Binding.Bind(Item, updateTarget));
                return This;
            }

            public Builder<T> BindToSource(Action<T, DataPresenter> updateSource, params BindingTrigger[] triggers)
            {
                Item.AddBinding(Binding.BindToSource(Item, updateSource, triggers));
                return This;
            }

            public Builder<T> BindTwoWay(Action<DataPresenter, T> updateTarget, Action<T, DataPresenter> updateSource, params BindingTrigger[] triggers)
            {
                Item.AddBinding(Binding.BindTwoWay(Item, updateTarget, updateSource, triggers));
                return This;
            }
        }

        internal static DataItem Create<T>()
            where T : UIElement, new()
        {
            return new DataItem(() => new T());
        }

        private DataItem(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        public bool IsMultidimensional { get; private set; }

        internal int AccumulatedBlockDimensionsDelta { get; set; }

        internal override void VerifyGridRange(GridRange rowRange)
        {
            if (GridRange.IntersectsWith(rowRange))
                throw new InvalidOperationException(Strings.DataItem_IntersectsWithRowRange(Ordinal));

            if (!IsMultidimensional)
                return;

            if (Template.IsMultidimensional(Orientation.Horizontal))
            {
                if (!rowRange.Contains(GridRange.Left) || !rowRange.Contains(GridRange.Right))
                    throw new InvalidOperationException(Strings.DataItem_OutOfHorizontalRowRange(Ordinal));
            }
            else if (Template.IsMultidimensional(Orientation.Vertical))
            {
                if (!rowRange.Contains(GridRange.Top) || !rowRange.Contains(GridRange.Bottom))
                    throw new InvalidOperationException(Strings.DataItem_OutOfVerticalRowRange(Ordinal));
            }
            else
                throw new InvalidOperationException(Strings.DataItem_OneDimensionalTemplate(Ordinal));
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
                int prevAccumulatedBlockDimensionsDelta = ordinal == 0 ? 0 : Template.DataItems[ordinal - 1].AccumulatedBlockDimensionsDelta;
                var elementIndex = ordinal * BlockDimensions - prevAccumulatedBlockDimensionsDelta + blockDimension;
                if (ordinal >= Template.DataItemsSplit)
                    elementIndex += ElementManager.Blocks.Count;
                return ElementManager.Elements[elementIndex];
            }
        }
    }
}

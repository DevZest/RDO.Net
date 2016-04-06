﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class BlockItem : TemplateItem
    {
        private sealed class Binding : BindingBase
        {
            internal static Binding Bind<T>(TemplateItem templateItem, Action<IBlockPresenter, T> updateTarget)
                where T : UIElement
            {
                return new Binding(templateItem, (source, element) => updateTarget(source, (T)element), null, null);
            }

            internal static Binding BindToSource<T>(TemplateItem templateItem, Action<T, IBlockPresenter> updateSource, BindingTrigger[] triggers)
                where T : UIElement
            {
                return new Binding(templateItem, null, (element, source) => updateSource((T)element, source), triggers);
            }

            internal static Binding BindTwoWay<T>(TemplateItem templateItem, Action<IBlockPresenter, T> updateTarget, Action<T, IBlockPresenter> updateSource, BindingTrigger[] triggers)
                where T : UIElement
            {
                return new Binding(templateItem, (source, element) => updateTarget(source, (T)element), (element, source) => updateSource((T)element, source), triggers);
            }

            private Binding(TemplateItem templateItem, Action<IBlockPresenter, UIElement> updateTarget, Action<UIElement, IBlockPresenter> updateSource, BindingTrigger[] triggers)
                : base(templateItem, triggers)
            {
                _updateTargetAction = updateTarget;
                _updateSourceAction = updateSource;
            }

            private Action<IBlockPresenter, UIElement> _updateTargetAction;

            private Action<UIElement, IBlockPresenter> _updateSourceAction;

            public override void UpdateTarget(BindingContext bindingContext, UIElement element)
            {
                if (_updateTargetAction != null)
                    _updateTargetAction(bindingContext.BlockPresenter, element);
            }

            public override void UpdateSource(BindingContext bindingContext, UIElement element)
            {
                if (_updateSourceAction != null)
                    _updateSourceAction(element, bindingContext.BlockPresenter);
            }
        }

        public sealed class Builder<T> : TemplateItem.Builder<T, BlockItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(TemplateItemBuilderFactory rangeConfig)
                : base(rangeConfig, BlockItem.Create<T>())
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

            public Builder<T> Bind(Action<IBlockPresenter, T> updateTarget)
            {
                Item.AddBinding(Binding.Bind(Item, updateTarget));
                return This;
            }

            public Builder<T> BindToSource(Action<T, IBlockPresenter> updateSource, params BindingTrigger[] triggers)
            {
                Item.AddBinding(Binding.BindToSource(Item, updateSource, triggers));
                return This;
            }

            public Builder<T> BindTwoWay(Action<IBlockPresenter, T> updateTarget, Action<T, IBlockPresenter> updateSource, params BindingTrigger[] triggers)
            {
                Item.AddBinding(Binding.BindTwoWay(Item, updateTarget, updateSource, triggers));
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

        internal override void VerifyGridRange(GridRange rowRange)
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
                    throw new InvalidOperationException(Strings.DataItem_OutOfVerticalRowRange(Ordinal));
            }
        }
    }
}

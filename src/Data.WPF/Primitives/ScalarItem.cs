using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class ScalarItem : TemplateItem
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

            public override void UpdateTarget(BindingSource bindingSource, UIElement element)
            {
                if (_updateTargetAction != null)
                    _updateTargetAction(bindingSource.DataPresenter, element);
            }

            public override void UpdateSource(UIElement element, BindingSource bindingSource)
            {
                if (_updateSourceAction != null)
                    _updateSourceAction(element, bindingSource.DataPresenter);
            }
        }

        public sealed class Builder<T> : TemplateItem.Builder<T, ScalarItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(GridRangeBuilder rangeConfig)
                : base(rangeConfig, ScalarItem.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override TemplateBuilder End(GridRangeBuilder rangeConfig, ScalarItem item)
            {
                return rangeConfig.End(item);
            }

            public Builder<T> IsRepeatable(bool value)
            {
                Item.IsRepeatable = value;
                return this;
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

        internal static ScalarItem Create<T>()
            where T : UIElement, new()
        {
            return new ScalarItem(() => new T());
        }

        private ScalarItem(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        public bool IsRepeatable { get; private set; }

        internal bool ShouldFlow
        {
            get
            {
                if (!IsRepeatable)
                    return false;
                else if (Template.RepeatOrientation == RepeatOrientation.Y && Template.FlowDimension != 1)
                    return Template.RepeatRange.Contains(GridRange.Left) && Template.RepeatRange.Contains(GridRange.Right);
                else if (Template.RepeatOrientation == RepeatOrientation.X && Template.FlowDimension != 1)
                    return Template.RepeatRange.Contains(GridRange.Top) && Template.RepeatRange.Contains(GridRange.Bottom);
                else
                    return false;
            }
        }
    }
}

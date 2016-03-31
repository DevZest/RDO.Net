using System;
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
            internal Builder(GridRangeBuilder rangeConfig)
                : base(rangeConfig, DataItem.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override TemplateBuilder End(GridRangeBuilder rangeConfig, DataItem item)
            {
                return rangeConfig.End(item);
            }

            public Builder<T> Repeat(bool value)
            {
                Item.Repeatable = value;
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

        internal static DataItem Create<T>()
            where T : UIElement, new()
        {
            return new DataItem(() => new T());
        }

        private DataItem(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        public bool Repeatable { get; private set; }

        internal bool CrossRepeatable
        {
            get
            {
                if (!Repeatable)
                    return false;
                else if (Template.CrossRepeatable(Orientation.Horizontal))
                    return Template.RowRange.Contains(GridRange.Left) && Template.RowRange.Contains(GridRange.Right);
                else if (Template.CrossRepeatable(Orientation.Vertical))
                    return Template.RowRange.Contains(GridRange.Top) && Template.RowRange.Contains(GridRange.Bottom);
                else
                    return false;
            }
        }

        internal int AccumulatedCrossRepeatsDelta { get; set; }
    }
}

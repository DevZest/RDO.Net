using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public class RowItem : TemplateItem, IConcatList<RowItem>
    {
        #region IConcatList<RowItem>

        int IReadOnlyCollection<RowItem>.Count
        {
            get { return 1; }
        }

        RowItem IReadOnlyList<RowItem>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        void IConcatList<RowItem>.Sort(Comparison<RowItem> comparision)
        {
        }

        IEnumerator<RowItem> IEnumerable<RowItem>.GetEnumerator()
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
            internal static Binding Bind<T>(TemplateItem templateItem, Action<RowPresenter, T> updateTarget)
                where T : UIElement
            {
                return new Binding(templateItem, (source, element) => updateTarget(source, (T)element), null, null);
            }

            internal static Binding BindToSource<T>(TemplateItem templateItem, Action<T, RowPresenter> updateSource, BindingTrigger[] triggers)
                where T : UIElement
            {
                return new Binding(templateItem, null, (element, source) => updateSource((T)element, source), triggers);
            }

            internal static Binding BindTwoWay<T>(TemplateItem templateItem, Action<RowPresenter, T> updateTarget, Action<T, RowPresenter> updateSource, BindingTrigger[] triggers)
                where T : UIElement
            {
                return new Binding(templateItem, (source, element) => updateTarget(source, (T)element), (element, source) => updateSource((T)element, source), triggers);
            }

            private Binding(TemplateItem templateItem, Action<RowPresenter, UIElement> updateTarget, Action<UIElement, RowPresenter> updateSource, BindingTrigger[] triggers)
                : base(templateItem, triggers)
            {
                _updateTargetAction = updateTarget;
                _updateSourceAction = updateSource;
            }

            private Action<RowPresenter, UIElement> _updateTargetAction;

            private Action<UIElement, RowPresenter> _updateSourceAction;

            public override void UpdateTarget(BindingContext bindingContext, UIElement element)
            {
                if (_updateTargetAction != null)
                    _updateTargetAction(bindingContext.RowPresenter, element);
            }

            public override void UpdateSource(BindingContext bindingContext, UIElement element)
            {
                if (_updateSourceAction != null)
                    _updateSourceAction(element, bindingContext.RowPresenter);
            }
        }

        public sealed class Builder<T> : TemplateItem.Builder<T, RowItem, Builder<T>>
            where T : UIElement, new()
        {
            internal Builder(TemplateBuilder templateBuilder)
                : base(templateBuilder, RowItem.Create<T>())
            {
            }

            internal override Builder<T> This
            {
                get { return this; }
            }

            internal override void AddItem(Template template, GridRange gridRange, RowItem item)
            {
                template.AddRowItem(gridRange, item);
            }

            public Builder<T> Bind(Action<RowPresenter, T> updateTargetAction)
            {
                TemplateItem.AddBinding(Binding.Bind(TemplateItem, updateTargetAction));
                return This;
            }

            public Builder<T> BindToSource(Action<T, RowPresenter> updateSourceAction, params BindingTrigger[] triggers)
            {
                TemplateItem.AddBinding(Binding.BindToSource(TemplateItem, updateSourceAction, triggers));
                return This;
            }

            public Builder<T> BindTwoWay(Action<RowPresenter, T> updateTargetAction, Action<T, RowPresenter> updateSourceAction, params BindingTrigger[] triggers)
            {
                TemplateItem.AddBinding(Binding.BindTwoWay(TemplateItem, updateTargetAction, updateSourceAction, triggers));
                return This;
            }
        }

        internal static RowItem Create<T>()
            where T : UIElement, new()
        {
            return new RowItem(() => new T());
        }

        internal RowItem(Func<UIElement> constructor)
            : base(constructor)
        {
        }

        internal sealed override void VerifyRowRange(GridRange rowRange)
        {
            if (!rowRange.Contains(GridRange))
                throw new InvalidOperationException(Strings.RowItem_OutOfRowRange(Ordinal));
        }
    }
}

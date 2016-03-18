using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class Binding
    {
        internal static Binding Bind<T>(TemplateItem templateItem, Action<BindingSource, T> updateTarget)
            where T : UIElement
        {
            return new Binding(templateItem, (source, element) => updateTarget(source, (T)element), null, null);
        }

        internal static Binding BindToSource<T>(TemplateItem templateItem, Action<T, BindingSource> updateSource, BindingTrigger[] triggers)
            where T : UIElement
        {
            return new Binding(templateItem, null, (element, source) => updateSource((T)element, source), triggers);
        }

        internal static Binding BindTwoWay<T>(TemplateItem templateItem, Action<BindingSource, T> updateTarget, Action<T, BindingSource> updateSource, BindingTrigger[] triggers)
            where T : UIElement
        {
            return new Binding(templateItem, (source, element) => updateTarget(source, (T)element), (element, source) => updateSource((T)element, source), triggers);
        }

        private Binding(TemplateItem templateItem, Action<BindingSource, UIElement> updateTarget, Action<UIElement, BindingSource> updateSource, BindingTrigger[] triggers)
        {
            TemplateItem = templateItem;
            UpdateTargetAction = updateTarget;
            UpdateSourceAction = updateSource;
            if (triggers == null || triggers.Length == 0)
                _triggers = EmptyArray<BindingTrigger>.Singleton;
            else
                _triggers = triggers;
        }

        public TemplateItem TemplateItem { get; private set; }

        public Action<BindingSource, UIElement> UpdateTargetAction { get; private set; }

        public Action<UIElement, BindingSource> UpdateSourceAction { get; private set; }

        IReadOnlyList<BindingTrigger> _triggers;
        public IReadOnlyList<BindingTrigger> Triggers
        {
            get { return _triggers; }
        }
    }
}

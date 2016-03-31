using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class BindingBase
    {
        protected BindingBase(TemplateItem templateItem, BindingTrigger[] triggers)
        {
            TemplateItem = templateItem;
            if (triggers == null || triggers.Length == 0)
                _triggers = EmptyArray<BindingTrigger>.Singleton;
            else
                _triggers = triggers;
        }

        public TemplateItem TemplateItem { get; private set; }

        public abstract void UpdateTarget(BindingContext bindingContext, UIElement element);

        public abstract void UpdateSource(BindingContext bindingContext, UIElement element);

        IReadOnlyList<BindingTrigger> _triggers;
        public IReadOnlyList<BindingTrigger> Triggers
        {
            get { return _triggers; }
        }
    }
}

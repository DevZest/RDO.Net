using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    internal sealed class Binding
    {
        internal static Binding Bind<T>(TemplateItem owner, Action<BindingSource, T> updateTarget)
            where T : UIElement
        {
            return new Binding(owner, (source, element) => updateTarget(source, (T)element), null, null);
        }

        internal static Binding BindToSource<T>(TemplateItem owner, Action<T, BindingSource> updateSource, BindingTrigger[] triggers)
            where T : UIElement
        {
            return new Binding(owner, null, (element, source) => updateSource((T)element, source), triggers);
        }

        internal static Binding BindTwoWay<T>(TemplateItem owner, Action<BindingSource, T> updateTarget, Action<T, BindingSource> updateSource, BindingTrigger[] triggers)
            where T : UIElement
        {
            return new Binding(owner, (source, element) => updateTarget(source, (T)element), (element, source) => updateSource((T)element, source), triggers);
        }

        private Binding(TemplateItem owner, Action<BindingSource, UIElement> updateTarget, Action<UIElement, BindingSource> updateSource, BindingTrigger[] triggers)
        {
            Owner = owner;
            _updateTarget = updateTarget;
            _updateSource = updateSource;
            if (triggers == null || triggers.Length == 0)
                _triggers = EmptyArray<BindingTrigger>.Singleton;
            else
                _triggers = triggers;
        }

        internal TemplateItem Owner { get; private set; }

        Action<BindingSource, UIElement> _updateTarget;
        internal void UpdateTarget(UIElement element)
        {
            if (_updateTarget != null)
            {
                var source = BindingSource.Current;
                source.Enter(this, element);
                try
                {
                    _updateTarget(source, element);
                }
                finally
                {
                    source.Exit();                    
                }
            }
        }

        Action<UIElement, BindingSource> _updateSource;
        internal void UpdateSource(UIElement element)
        {
            if (_updateSource != null)
            {
                var source = BindingSource.Current;
                source.Enter(this, element);
                try
                {
                    _updateSource(element, source);
                }
                finally
                {
                    source.Exit();
                }
            }
        }

        IReadOnlyList<BindingTrigger> _triggers;
        internal IReadOnlyList<BindingTrigger> Triggers
        {
            get { return _triggers; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    internal sealed class Binding
    {
        internal static Binding Bind<T>(Action<T> updateTarget)
            where T : UIElement
        {
            return new Binding(x => updateTarget((T)x), null, null);
        }

        internal static Binding BindToSource<T>(Action<T> updateSource, BindingTrigger[] triggers)
            where T : UIElement
        {
            return new Binding(null, x => updateSource((T)x), triggers);
        }

        internal static Binding BindTwoWay<T>(Action<T> updateTarget, Action<T> updateSource, BindingTrigger[] triggers)
            where T : UIElement
        {
            return new Binding(x => updateTarget((T)x), x => updateSource((T)x), triggers);
        }

        private Binding(Action<UIElement> updateTarget, Action<UIElement> updateSource, BindingTrigger[] triggers)
        {
            _updateTarget = updateTarget;
            _updateSource = updateSource;
            if (triggers == null || triggers.Length == 0)
                _triggers = s_emptyTriggers;
            else
                _triggers = triggers;
        }

        Action<UIElement> _updateTarget;
        internal void UpdateTarget(UIElement element)
        {
            if (_updateTarget != null)
                _updateTarget(element);
        }

        Action<UIElement> _updateSource;
        internal void UpdateSource(UIElement element)
        {
            if (_updateSource != null)
                _updateSource(element);
        }

        private static BindingTrigger[] s_emptyTriggers = new BindingTrigger[0];
        IReadOnlyList<BindingTrigger> _triggers;
        internal IReadOnlyList<BindingTrigger> Triggers
        {
            get { return _triggers; }
        }
    }
}

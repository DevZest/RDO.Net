using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class DataBinding
    {
        internal static DataBinding Bind<T>(Action<T> updateTarget)
            where T : UIElement
        {
            return new DataBinding(x => updateTarget((T)x), null, null);
        }

        internal static DataBinding BindToSource<T>(Action<T> updateSource, ElementTrigger[] triggers)
            where T : UIElement
        {
            return new DataBinding(null, x => updateSource((T)x), triggers);
        }

        internal static DataBinding BindTwoWay<T>(Action<T> updateTarget, Action<T> updateSource, ElementTrigger[] triggers)
            where T : UIElement
        {
            return new DataBinding(x => updateTarget((T)x), x => updateSource((T)x), triggers);
        }

        private DataBinding(Action<UIElement> updateTarget, Action<UIElement> updateSource, ElementTrigger[] triggers)
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

        private static ElementTrigger[] s_emptyTriggers = new ElementTrigger[0];
        IReadOnlyList<ElementTrigger> _triggers;
        internal IReadOnlyList<ElementTrigger> Triggers
        {
            get { return _triggers; }
        }
    }
}

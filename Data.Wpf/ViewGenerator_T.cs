using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class ViewGenerator<T> : ViewGenerator
        where T : UIElement
    {
        internal ViewGenerator(Func<T> creator, Action<T> initializer)
        {
            Debug.Assert(creator != null);
            Debug.Assert(initializer != null);
            _creator = creator;
        }

        Func<T> _creator;
        Action<T> _initializer;

        internal sealed override UIElement CreateUIElement()
        {
            return _creator();
        }

        internal virtual T CreateUIElementOverride()
        {
            return _creator();
        }

        internal sealed override void InitializeUIElement(UIElement uiElement)
        {
            InitializeUIElementOverride((T)uiElement);
        }

        internal virtual void InitializeUIElementOverride(T uiElement)
        {
            _initializer(uiElement);
        }
    }
}

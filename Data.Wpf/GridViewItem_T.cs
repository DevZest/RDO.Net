using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class GridItem<T> : GridItem
        where T : UIElement, new()
    {
        internal GridItem(Action<T> initializer)
        {
            _initializer = initializer;
        }

        Action<T> _initializer;

        internal sealed override UIElement CreateUIElement()
        {
            return new T();
        }

        internal sealed override void InitUIElement(UIElement uiElement)
        {
            InitUIElementOverride((T)uiElement);
        }

        internal virtual void InitUIElementOverride(T uiElement)
        {
            if (_initializer != null)
                _initializer(uiElement);
        }
    }
}

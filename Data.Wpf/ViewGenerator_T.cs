﻿using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public abstract class ViewGenerator<T> : ViewGenerator
        where T : UIElement, new()
    {
        internal ViewGenerator(Action<T> initializer)
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

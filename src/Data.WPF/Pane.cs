using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class Pane : ContentPresenter
    {
        private List<UIElement> _children = new List<UIElement>();
        internal IReadOnlyList<UIElement> Children
        {
            get { return _children; }
        }

        internal bool IsNew { get; private set; } = true;
        internal void ClearIsNew()
        {
            IsNew = false;
        }

        internal void AddChild(UIElement child, string name)
        {
            var placeholder = GetPlaceholder(name);
            if (placeholder == null)
                throw new InvalidOperationException();
            _children.Add(child);
            placeholder.Content = child;
        }

        protected virtual ContentPresenter GetPlaceholder(string name)
        {
            return FindName(name) as ContentPresenter;
        }
    }
}

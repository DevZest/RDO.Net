using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class Pane : ContentPresenter
    {
        private List<UIElement> _children = new List<UIElement>();
        private IReadOnlyList<UIElement> Children
        {
            get { return _children; }
        }

        private bool IsNew { get; set; }

        private void AddChild(UIElement child, string name)
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

        internal Pane BeginSetup(IReadOnlyList<Binding> bindings, IReadOnlyList<string> names)
        {
            Debug.Assert(bindings.Count == names.Count);
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                binding.BeginSetup();
                var name = names[i];
                AddChild(binding.GetSettingUpElement(), name);
            }
            IsNew = true;
            return this;
        }

        internal Pane BeginSetup(IReadOnlyList<Binding> bindings)
        {
            Debug.Assert(bindings.Count == Children.Count);
            if (IsNew)
                return this;

            for (int i = 0; i < bindings.Count; i++)
                bindings[i].BeginSetup(Children[i]);
            return this;
        }

        internal void Refresh(IReadOnlyList<Binding> bindings)
        {
            Debug.Assert(bindings.Count == Children.Count);
            for (int i = 0; i < bindings.Count; i++)
                bindings[i].Refresh(Children[i]);
        }

        internal void Cleanup(IReadOnlyList<Binding> bindings)
        {
            Debug.Assert(bindings.Count == Children.Count);
            for (int i = 0; i < bindings.Count; i++)
                bindings[i].Cleanup(Children[i], false);
        }

        internal void EndSetup(IReadOnlyList<Binding> bindings)
        {
            Debug.Assert(bindings.Count == Children.Count);
            for (int i = 0; i < bindings.Count; i++)
                bindings[i].EndSetup();
            IsNew = false;
        }

        internal void FlushInput(IReadOnlyList<TwoWayBinding> bindings)
        {
            Debug.Assert(bindings.Count == Children.Count);
            for (int i = 0; i < bindings.Count; i++)
                bindings[i].FlushInput(Children[i]);
        }
    }
}

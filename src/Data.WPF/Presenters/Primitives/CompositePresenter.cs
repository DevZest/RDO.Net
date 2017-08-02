using DevZest.Data.Views.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters.Primitives
{
    public sealed class CompositePresenter
    {
        public CompositePresenter(ICompositeView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            _view = view;
        }

        private readonly ICompositeView _view;

        private readonly List<UIElement> _children = new List<UIElement>();
        public IReadOnlyList<UIElement> Children
        {
            get { return _children; }
        }

        private bool IsNew { get; set; }

        private void AddChild(UIElement child, string name)
        {
            var placeholder = _view.GetPlaceholder(name);
            if (placeholder == null)
                throw new InvalidOperationException();
            _children.Add(child);
            placeholder.Content = child;
        }

        internal ICompositeView InitChildren(IReadOnlyList<Binding> bindings, IReadOnlyList<string> names)
        {
            Debug.Assert(bindings.Count == names.Count);
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                binding.BeginSetup(null);
                var name = names[i];
                AddChild(binding.GetSettingUpElement(), name);
            }
            IsNew = true;
            return _view;
        }

        internal void BeginSetup(IReadOnlyList<Binding> bindings)
        {
            Debug.Assert(bindings.Count == Children.Count);
            if (IsNew)
                return;

            for (int i = 0; i < bindings.Count; i++)
                bindings[i].BeginSetup(Children[i]);
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
                bindings[i].Cleanup(Children[i]);
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

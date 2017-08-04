using DevZest.Data.Views.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class CompositeBindingDispatcher
    {
        protected CompositeBindingDispatcher()
        {
        }

        protected abstract ICompositeView View { get; }

        public abstract IReadOnlyList<UIElement> Children { get; }

        private bool IsNew { get; set; }

        protected abstract void AddChild(UIElement child, string name);

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
            return View;
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

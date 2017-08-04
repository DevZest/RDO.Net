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

        private ICompositeBinding _compositeBinding;

        public IReadOnlyList<Binding> Bindings
        {
            get { return _compositeBinding?.Bindings; }
        }

        public IReadOnlyList<string> Names
        {
            get { return _compositeBinding?.Names; }
        }

        public abstract IReadOnlyList<UIElement> Children { get; }

        private bool IsNew { get; set; }

        protected abstract void AddChild(UIElement child, string name);

        internal ICompositeView Initialize(ICompositeBinding compositeBinding)
        {
            Debug.Assert(compositeBinding != null);
            _compositeBinding = compositeBinding;
            var bindings = Bindings;
            var names = Names;
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

        internal void BeginSetup()
        {
            var bindings = Bindings;
            Debug.Assert(bindings.Count == Children.Count);
            if (IsNew)
                return;

            for (int i = 0; i < bindings.Count; i++)
                bindings[i].BeginSetup(Children[i]);
        }

        internal void Refresh()
        {
            var bindings = Bindings;
            Debug.Assert(bindings.Count == Children.Count);
            for (int i = 0; i < bindings.Count; i++)
                bindings[i].Refresh(Children[i]);
        }

        internal void Cleanup()
        {
            var bindings = Bindings;
            Debug.Assert(bindings.Count == Children.Count);
            for (int i = 0; i < bindings.Count; i++)
                bindings[i].Cleanup(Children[i]);
        }

        internal void EndSetup()
        {
            var bindings = Bindings;
            Debug.Assert(bindings.Count == Children.Count);
            for (int i = 0; i < bindings.Count; i++)
                bindings[i].EndSetup();
            IsNew = false;
        }

        internal void FlushInput()
        {
            var bindings = Bindings;
            Debug.Assert(bindings.Count == Children.Count);
            for (int i = 0; i < bindings.Count; i++)
                ((TwoWayBinding)bindings[i]).FlushInput(Children[i]);
        }
    }
}

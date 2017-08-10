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
                var name = names[i];
                Initialize(i, binding, name);
            }
            return View;
        }

        protected virtual void Initialize(int index, Binding binding, string name)
        {
            binding.BeginSetup(null);
            AddChild(binding.GetSettingUpElement(), name);
        }

        internal void BeginSetup()
        {
            var bindings = Bindings;
            Debug.Assert(bindings.Count == Children.Count);

            for (int i = 0; i < bindings.Count; i++)
                BeginSetup(i, bindings[i], Children[i]);
        }

        protected virtual void BeginSetup(int index, Binding binding, UIElement element)
        {
            binding.BeginSetup(element);
        }

        internal void EndSetup()
        {
            var bindings = Bindings;
            Debug.Assert(bindings.Count == Children.Count);
            for (int i = 0; i < bindings.Count; i++)
                EndSetup(i, bindings[i]);
        }

        protected virtual void EndSetup(int index, Binding binding)
        {
            binding.EndSetup();
        }

        internal void Refresh()
        {
            var bindings = Bindings;
            Debug.Assert(bindings.Count == Children.Count);
            for (int i = 0; i < bindings.Count; i++)
                Refresh(i, bindings[i], Children[i]);
        }

        protected virtual void Refresh(int index, Binding binding, UIElement element)
        {
            binding.Refresh(element);
        }

        internal void Cleanup()
        {
            var bindings = Bindings;
            Debug.Assert(bindings.Count == Children.Count);
            for (int i = 0; i < bindings.Count; i++)
                Cleanup(i, bindings[i], Children[i]);
        }

        protected virtual void Cleanup(int index, Binding binding, UIElement element)
        {
            binding.Cleanup(element);
        }

        internal void FlushInput()
        {
            var bindings = Bindings;
            Debug.Assert(bindings.Count == Children.Count);
            for (int i = 0; i < bindings.Count; i++)
                FlushInput(i, (TwoWayBinding)bindings[i], Children[i]);
        }

        protected virtual void FlushInput(int index, TwoWayBinding binding, UIElement element)
        {
            binding.FlushInput(element);
        }
    }
}

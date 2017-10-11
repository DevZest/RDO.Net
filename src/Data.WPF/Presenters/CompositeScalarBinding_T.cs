using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public sealed class CompositeScalarBinding<T> : ScalarBindingBase<T>
        where T : UIElement, new()
    {
        private List<ScalarBinding> _childBindings = new List<ScalarBinding>();
        private List<Func<T, UIElement>> _childGetters = new List<Func<T, UIElement>>();

        public CompositeScalarBinding<T> AddChild<TChild>(ScalarBinding<TChild> childBinding, Func<T, TChild> childGetter)
            where TChild : UIElement, new()
        {
            Binding.VerifyAdding(childBinding, nameof(childBinding));

            if (childGetter == null)
                throw new ArgumentNullException(nameof(childGetter));

            VerifyNotSealed();
            _childBindings.Add(childBinding);
            _childGetters.Add(childGetter);
            childBinding.Seal(this, _childBindings.Count - 1);
            return this;
        }

        public IReadOnlyList<Binding> ChildBindings
        {
            get { return _childBindings; }
        }

        internal override UIElement GetChild(UIElement parent, int index)
        {
            return _childGetters[index]((T)parent);
        }

        internal override void BeginSetup(int startOffset, UIElement[] elements)
        {
            base.BeginSetup(startOffset, elements);
            for (int i = 0; i < _childBindings.Count; i++)
            {
                var childBinding = _childBindings[i];
                var childGetter = _childGetters[i];
                var children = GetChildren(SettingUpElements, childGetter);
                if (children != null)
                    childBinding.BeginSetup(startOffset, children);
            }
        }

        private static UIElement[] GetChildren(IReadOnlyList<T> parents, Func<T, UIElement> childGetter)
        {
            var result = new UIElement[parents.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var child = childGetter(parents[i]);
                if (child == null)
                    return null;
                result[i] = child;
            }
            return result;
        }

        internal override void PerformEnterSetup(int flowIndex)
        {
            base.PerformEnterSetup(flowIndex);
            for (int i = 0; i < _childBindings.Count; i++)
                _childBindings[i].PerformEnterSetup(flowIndex);
        }

        internal override void PerformExitSetup()
        {
            base.PerformExitSetup();
            for (int i = 0; i < _childBindings.Count; i++)
                _childBindings[i].PerformExitSetup();
        }

        internal override void BeginSetup(UIElement value)
        {
            base.BeginSetup(value);
            for (int i = 0; i < _childBindings.Count; i++)
            {
                var childBinding = _childBindings[i];
                var childGetter = _childGetters[i];
                var child = childGetter(SettingUpElement);
                if (child != null)
                    childBinding.BeginSetup(child);
            }
        }

        internal override void EndSetup()
        {
            base.EndSetup();
            for (int i = 0; i < _childBindings.Count; i++)
                _childBindings[i].EndSetup();
        }

        internal sealed override void PerformSetup(ScalarPresenter scalarPresenter)
        {
            for (int i = 0; i < _childBindings.Count; i++)
            {
                var childBinding = _childBindings[i];
                if (childBinding.GetSettingUpElement() != null)
                    childBinding.Setup(scalarPresenter.FlowIndex);
            }
        }

        private bool _isRefreshing;
        public override bool IsRefreshing
        {
            get { return _isRefreshing; }
        }

        internal sealed override void Refresh(UIElement element)
        {
            _isRefreshing = true;
            PerformRefresh((T)element);
            _isRefreshing = false;
        }

        private void PerformRefresh(T element)
        {
            for (int i = 0; i < _childBindings.Count; i++)
            {
                var childBinding = _childBindings[i];
                var childGetter = _childGetters[i];
                var child = childGetter(element);
                if (child != null)
                    childBinding.Refresh(child);
            }
        }

        internal sealed override void Cleanup(UIElement element)
        {
            PerformCleanup((T)element);
        }

        private void PerformCleanup(T element)
        {
            for (int i = 0; i < _childBindings.Count; i++)
            {
                var childBinding = _childBindings[i];
                var childGetter = _childGetters[i];
                var child = childGetter(element);
                if (child != null)
                    childBinding.Cleanup(child);
            }
        }

        internal sealed override void FlushInput(UIElement element)
        {
            PerformFlushInput((T)element);
        }

        private void PerformFlushInput(T element)
        {
            for (int i = 0; i < _childBindings.Count; i++)
            {
                var childBinding = _childBindings[i];
                var childGetter = _childGetters[i];
                var child = childGetter(element);
                if (child != null)
                    childBinding.FlushInput(child);
            }
        }
    }
}

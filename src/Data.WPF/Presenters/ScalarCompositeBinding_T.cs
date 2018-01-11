using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters
{
    public sealed class ScalarCompositeBinding<T> : ScalarBindingBase<T>
        where T : UIElement, new()
    {
        private List<ScalarBinding> _childBindings = new List<ScalarBinding>();
        private List<Func<T, UIElement>> _childGetters = new List<Func<T, UIElement>>();

        public ScalarCompositeBinding<T> AddChild<TChild>(ScalarBinding<TChild> childBinding, Func<T, TChild> childGetter)
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

        public override IReadOnlyList<ScalarBinding> ChildBindings
        {
            get { return _childBindings; }
        }

        public override IScalarInput ScalarInput
        {
            get { return null; }
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

        internal override void BeginSetup(UIElement value)
        {
            base.BeginSetup(value);
            for (int i = 0; i < _childBindings.Count; i++)
            {
                var childBinding = _childBindings[i];
                var childGetter = _childGetters[i];
                var child = childGetter(GetSettingUpElement(0));
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

        internal sealed override void PerformSetup(T element, ScalarPresenter scalarPresenter)
        {
            var flowIndex = scalarPresenter.FlowIndex;
            for (int i = 0; i < _childBindings.Count; i++)
            {
                var childBinding = _childBindings[i];
                var child = _childGetters[i](element);
                if (child != null)
                {
                    child.SetScalarFlowIndex(flowIndex);
                    childBinding.Setup(flowIndex);
                }
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
                {
                    childBinding.Cleanup(child);
                    child.SetScalarFlowIndex(0);
                }
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

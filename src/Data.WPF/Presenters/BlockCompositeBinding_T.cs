using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents block level composite data binding.
    /// </summary>
    /// <typeparam name="T">Type of target UI element.</typeparam>
    public sealed class BlockCompositeBinding<T> : BlockBindingBase<T>
        where T : UIElement, new()
    {
        private List<BlockBinding> _childBindings = new List<BlockBinding>();
        private List<Func<T, UIElement>> _childGetters = new List<Func<T, UIElement>>();

        /// <summary>
        /// Adds child binding.
        /// </summary>
        /// <typeparam name="TChild">Type of child binding target UI element.</typeparam>
        /// <param name="childBinding">The child binding.</param>
        /// <param name="childGetter">The getter to return child UI element.</param>
        /// <returns>This composite binding for fluent coding.</returns>
        public BlockCompositeBinding<T> AddChild<TChild>(BlockBinding<TChild> childBinding, Func<T, TChild> childGetter)
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

        /// <summary>
        /// Gets collection of child bindings.
        /// </summary>
        public IReadOnlyList<Binding> ChildBindings
        {
            get { return _childBindings; }
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            base.BeginSetup(value);
            for (int i = 0; i < _childBindings.Count; i++)
            {
                var childBinding = _childBindings[i];
                var child = _childGetters[i](SettingUpElement);
                if (child != null)
                    childBinding.BeginSetup(child);
            }
        }

        internal sealed override void EndSetup()
        {
            base.EndSetup();
            for (int i = 0; i < _childBindings.Count; i++)
                _childBindings[i].EndSetup();
        }

        internal sealed override void PerformSetup(BlockView blockView)
        {
            for (int i = 0; i < _childBindings.Count; i++)
            {
                var childBinding = _childBindings[i];
                if (childBinding.GetSettingUpElement() != null)
                    childBinding.Setup(blockView);
            }
        }

        internal sealed override void Refresh(UIElement element)
        {
            PerformRefresh((T)element);
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

        internal override UIElement GetChild(UIElement parent, int index)
        {
            return _childGetters[index]((T)parent);
        }
    }
}

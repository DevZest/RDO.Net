using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents row level composite data binding.
    /// </summary>
    /// <typeparam name="T">The type of view element.</typeparam>
    public sealed class RowCompositeBinding<T> : RowBindingBase<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RowCompositeBinding{T}"/> class.
        /// </summary>
        public RowCompositeBinding()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RowCompositeBinding{T}"/> class.
        /// </summary>
        /// <param name="onRefresh">The delegate to refresh the binding.</param>
        public RowCompositeBinding(Action<T, RowPresenter> onRefresh)
        {
            _onRefresh = onRefresh;
        }

        private List<RowBinding> _childBindings = new List<RowBinding>();
        private List<Func<T, UIElement>> _childGetters = new List<Func<T, UIElement>>();

        /// <inheritdoc/>
        public override IReadOnlyList<RowBinding> ChildBindings
        {
            get { return _childBindings; }
        }

        /// <inheritdoc/>
        public override Input<RowBinding, IColumns> RowInput
        {
            get { return null; }
        }

        /// <summary>
        /// Adds child binding.
        /// </summary>
        /// <typeparam name="TChild">Type of child view element.</typeparam>
        /// <param name="childBinding">The child binding.</param>
        /// <param name="childGetter">Getter to return child view element.</param>
        /// <returns>This row composite binding for fluent coding.</returns>
        public RowCompositeBinding<T> AddChild<TChild>(RowBindingBase<TChild> childBinding, Func<T, TChild> childGetter)
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

        internal sealed override void PerformSetup(RowPresenter rowPresenter)
        {
            for (int i = 0; i < _childBindings.Count; i++)
            {
                var childBinding = _childBindings[i];
                if (childBinding.GetSettingUpElement() != null)
                    childBinding.Setup(rowPresenter);
            }
            _onRefresh?.Invoke(SettingUpElement, rowPresenter);

            var rowElement = SettingUpElement as IRowElement;
            rowElement?.Setup(rowPresenter);
        }

        private bool _isRefreshing;
        /// <inheritdoc/>
        public override bool IsRefreshing
        {
            get { return _isRefreshing; }
        }

        private Action<T, RowPresenter> _onRefresh;
        internal sealed override void Refresh(UIElement element)
        {
            _isRefreshing = true;
            var v = (T)element;
            PerformRefresh(v);
            _onRefresh?.Invoke(v, element.GetRowPresenter());
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

            var rowElement = element as IRowElement;
            rowElement?.Refresh(element.GetRowPresenter());
        }

        internal sealed override void PerformCleanup(UIElement element)
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

            var rowElement = element as IRowElement;
            rowElement?.Cleanup(element.GetRowPresenter());
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

        internal override UIElement GetChild(UIElement parent, int index)
        {
            return _childGetters[index]((T)parent);
        }
    }
}

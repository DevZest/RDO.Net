﻿using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class ScalarBinding<T> : ScalarBindingBase<T>
        where T : UIElement, new()
    {
        public ScalarBinding(Action<T> onRefresh)
        {
            if (onRefresh != null)
                _onRefresh = (e, sp) => onRefresh(e);
        }

        public ScalarBinding(Action<T> onRefresh, Action<T> onSetup, Action<T> onCleanup)
            : this(onRefresh)
        {
            if (onSetup != null)
                _onSetup = (v, p) => onSetup(v);
            if (onCleanup != null)
                _onCleanup = (v, p) => onCleanup(v);
        }

        public ScalarBinding(Action<T, ScalarPresenter> onRefresh)
        {
            _onRefresh = onRefresh;
        }

        public ScalarBinding(Action<T, ScalarPresenter> onRefresh, Action<T, ScalarPresenter> onSetup, Action<T, ScalarPresenter> onCleanup)
            : this(onRefresh)
        {
            _onSetup = onSetup;
            _onCleanup = onCleanup;
        }

        public ScalarInput<T> Input { get; private set; }

        internal sealed override void FlushInput(UIElement element)
        {
            if (Input != null)
                Input.Flush((T)element);
        }

        internal sealed override void PerformSetup(T element, ScalarPresenter scalarPresenter)
        {
            Setup(element, scalarPresenter);
            Refresh(element, scalarPresenter);
            if (Input != null)
                Input.Attach(element);
        }

        private Action<T, ScalarPresenter> _onSetup;
        private void Setup(T element, ScalarPresenter scalarPresenter)
        {
            var behaviors = Behaviors;
            for (int i = 0; i < behaviors.Count; i++)
                behaviors[i].Setup(element, scalarPresenter);
            if (_onSetup != null)
                _onSetup(element, scalarPresenter);
            var scalarElement = element as IScalarElement;
            if (scalarElement != null)
                scalarElement.Setup(scalarPresenter);
        }

        private bool _isRefreshing;
        public override bool IsRefreshing
        {
            get { return _isRefreshing; }
        }

        private Action<T, ScalarPresenter> _onRefresh;
        internal void Refresh(T element, ScalarPresenter scalarPresenter)
        {
            _isRefreshing = true;
            var behaviors = Behaviors;
            for (int i = 0; i < behaviors.Count; i++)
                behaviors[i].Refresh(element, scalarPresenter);
            if (_onRefresh != null)
                _onRefresh(element, scalarPresenter);
            var scalarElement = element as IScalarElement;
            if (scalarElement != null)
                scalarElement.Refresh(scalarPresenter);
            _isRefreshing = false;
        }

        private Action<T, ScalarPresenter> _onCleanup;
        private void Cleanup(T element, ScalarPresenter scalarPresenter)
        {
            var behaviors = Behaviors;
            for (int i = 0; i < behaviors.Count; i++)
                behaviors[i].Cleanup(element, scalarPresenter);
            var scalarElement = element as IScalarElement;
            if (scalarElement != null)
                scalarElement.Cleanup(scalarPresenter);
            if (_onCleanup != null)
                _onCleanup(element, scalarPresenter);
        }

        private T Restore(UIElement element)
        {
            var result = (T)element;
            ScalarPresenter.EnterSetup(element.GetScalarFlowIndex());
            return result;
        }

        internal sealed override void Refresh(UIElement element)
        {
            var e = Restore(element);
            if (Input != null)
                Input.Refresh(e, ScalarPresenter);
            else
                Refresh(e, ScalarPresenter);
            ScalarPresenter.ExitSetup();
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var e = Restore(element);
            if (Input != null)
                Input.Detach(e);
            Cleanup(e, ScalarPresenter);
            e.SetScalarFlowIndex(0);
            ScalarPresenter.ExitSetup();
        }

        public ScalarInput<T> BeginInput(Trigger<T> flushTrigger)
        {
            VerifyNotSealed();
            if (Input != null)
                throw new InvalidOperationException(DiagnosticMessages.TwoWayBinding_InputAlreadyExists);

            return Input = new ScalarInput<T>(this, flushTrigger);
        }

        public ScalarInput<T> BeginInput(DependencyProperty dependencyProperty)
        {
            return BeginInput(new PropertyChangedTrigger<T>(dependencyProperty));
        }

        public ScalarBinding<T> WithInput<TData>(Trigger<T> flushTrigger, Scalar<TData> data, Func<T, TData> getValue)
        {
            return BeginInput(flushTrigger).WithFlush(data, getValue).EndInput();
        }

        public ScalarBinding<T> WithInput<TData>(DependencyProperty dependencyProperty, Scalar<TData> data, Func<T, TData> getValue)
        {
            return WithInput(new PropertyChangedTrigger<T>(dependencyProperty), data, getValue);
        }

        public new T this[int flowIndex]
        {
            get { return (T)base[flowIndex]; }
        }

        public ScalarBinding<T> OverrideSetup(Action<T, ScalarPresenter, Action<T, ScalarPresenter>> overrideSetup)
        {
            if (overrideSetup == null)
                throw new ArgumentNullException(nameof(overrideSetup));
            _onSetup = _onSetup.Override(overrideSetup);
            return this;
        }

        public ScalarBinding<T> OverrideRefresh(Action<T, ScalarPresenter, Action<T, ScalarPresenter>> overrideRefresh)
        {
            if (overrideRefresh == null)
                throw new ArgumentNullException(nameof(overrideRefresh));
            _onRefresh = _onRefresh.Override(overrideRefresh);
            return this;
        }

        public ScalarBinding<T> OverrideCleanup(Action<T, ScalarPresenter, Action<T, ScalarPresenter>> overrideCleanup)
        {
            if (overrideCleanup == null)
                throw new ArgumentNullException(nameof(overrideCleanup));
            _onCleanup = _onRefresh.Override(overrideCleanup);
            return this;
        }

        private List<IScalarBindingBehavior<T>> _behaviors;
        public IReadOnlyList<IScalarBindingBehavior<T>> Behaviors
        {
            get
            {
                if (_behaviors == null)
                    return Array<IScalarBindingBehavior<T>>.Empty;
                else
                    return _behaviors;
            }
        }

        internal void InternalAddBehavior(IScalarBindingBehavior<T> behavior)
        {
            Debug.Assert(behavior != null);
            if (_behaviors == null)
                _behaviors = new List<IScalarBindingBehavior<T>>();
            _behaviors.Add(behavior);
        }

        internal override UIElement GetChild(UIElement parent, int index)
        {
            throw new NotSupportedException();
        }
    }
}

using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents strongly typed scalar data binding.
    /// </summary>
    /// <typeparam name="T">The type of view element.</typeparam>
    public sealed class ScalarBinding<T> : ScalarBindingBase<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ScalarBinding{T}"/> class.
        /// </summary>
        /// <param name="onRefresh">The delegate to refresh the binding.</param>
        public ScalarBinding(Action<T> onRefresh)
        {
            if (onRefresh != null)
                _onRefresh = (e, sp) => onRefresh(e);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ScalarBinding{T}"/> class.
        /// </summary>
        /// <param name="onRefresh">The delegate to refresh the binding.</param>
        /// <param name="onSetup">The delegate to setup the binding.</param>
        /// <param name="onCleanup">The delegate to cleanup the binding.</param>
        public ScalarBinding(Action<T> onRefresh, Action<T> onSetup, Action<T> onCleanup)
            : this(onRefresh)
        {
            if (onSetup != null)
                _onSetup = (v, p) => onSetup(v);
            if (onCleanup != null)
                _onCleanup = (v, p) => onCleanup(v);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ScalarBinding{T}"/> class.
        /// </summary>
        /// <param name="onRefresh">The delegate to refresh the binding.</param>
        public ScalarBinding(Action<T, ScalarPresenter> onRefresh)
        {
            _onRefresh = onRefresh;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ScalarBinding{T}"/> class.
        /// </summary>
        /// <param name="onRefresh">The delegate to refresh the binding.</param>
        /// <param name="onSetup">The delegate to setup the binding.</param>
        /// <param name="onCleanup">The delegate to cleanup the binding.</param>
        public ScalarBinding(Action<T, ScalarPresenter> onRefresh, Action<T, ScalarPresenter> onSetup, Action<T, ScalarPresenter> onCleanup)
            : this(onRefresh)
        {
            _onSetup = onSetup;
            _onCleanup = onCleanup;
        }

        /// <inheritdoc/>
        public override IReadOnlyList<ScalarBinding> ChildBindings
        {
            get { return Array.Empty<ScalarBinding>(); }
        }

        /// <summary>
        /// Gets the input that handles flushing from view to presenter.
        /// </summary>
        public ScalarInput<T> Input { get; internal set; }

        /// <inheritdoc/>
        public override Input<ScalarBinding, IScalars> ScalarInput
        {
            get { return Input; }
        }

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
        /// <inheritdoc/>
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

        /// <summary>
        /// Begins input implementation.
        /// </summary>
        /// <param name="flushingTrigger">The flushing trigger.</param>
        /// <param name="progressiveFlushingTrigger">The progressive flushing trigger.</param>
        /// <returns>The scalar input.</returns>
        public ScalarInput<T> BeginInput(Trigger<T> flushingTrigger, Trigger<T> progressiveFlushingTrigger = null)
        {
            VerifyNotSealed();
            if (Input != null)
                throw new InvalidOperationException(DiagnosticMessages.TwoWayBinding_InputAlreadyExists);

            return Input = new ScalarInput<T>(this, flushingTrigger, progressiveFlushingTrigger);
        }

        /// <summary>
        /// Begins input implementation.
        /// </summary>
        /// <param name="dependencyProperty">The dependency property for flushing.</param>
        /// <param name="progressiveFlushingRoutedEvent">The routed event for progressive flushing.</param>
        /// <returns>The scalar input.</returns>
        public ScalarInput<T> BeginInput(DependencyProperty dependencyProperty, RoutedEvent progressiveFlushingRoutedEvent = null)
        {
            return BeginInput(new PropertyChangedTrigger<T>(dependencyProperty), progressiveFlushingRoutedEvent == null ? null : new RoutedEventTrigger<T>(progressiveFlushingRoutedEvent));
        }

        /// <summary>
        /// Sets input implementation from specified scalar data.
        /// </summary>
        /// <typeparam name="TData">Data type of scalar data.</typeparam>
        /// <param name="flushingTrigger">The flushing trigger.</param>
        /// <param name="progressiveFlushingTrigger">The progressive flushing trigger.</param>
        /// <param name="data">The scalar data.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This scalar binding for fluent coding.</returns>
        public ScalarBinding<T> WithInput<TData>(Trigger<T> flushingTrigger, Trigger<T> progressiveFlushingTrigger, Scalar<TData> data, Func<T, TData> getValue)
        {
            return BeginInput(flushingTrigger, progressiveFlushingTrigger).WithFlush(data, getValue).EndInput();
        }

        /// <summary>
        /// Sets input implementation from specified scalar data.
        /// </summary>
        /// <typeparam name="TData">Data type of scalar data.</typeparam>
        /// <param name="flushingTrigger">The flushing trigger.</param>
        /// <param name="data">The scalar data.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This scalar binding for fluent coding.</returns>
        public ScalarBinding<T> WithInput<TData>(Trigger<T> flushingTrigger, Scalar<TData> data, Func<T, TData> getValue)
        {
            return BeginInput(flushingTrigger).WithFlush(data, getValue).EndInput();
        }

        /// <summary>
        /// Sets input implementation with specified scalar data.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="dependencyProperty">The dependency property for flushing.</param>
        /// <param name="progressiveFlushingRoutedEvent">The routed event for progressive flushing.</param>
        /// <param name="data">The scalar data.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This scalar binding for fluent coding.</returns>
        public ScalarBinding<T> WithInput<TData>(DependencyProperty dependencyProperty, RoutedEvent progressiveFlushingRoutedEvent, Scalar<TData> data, Func<T, TData> getValue)
        {
            return WithInput(new PropertyChangedTrigger<T>(dependencyProperty), new RoutedEventTrigger<T>(progressiveFlushingRoutedEvent), data, getValue);
        }

        /// <summary>
        /// Sets input implementation with specified scalar data.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="dependencyProperty">The dependency property for flushing.</param>
        /// <param name="data">The scalar data.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This scalar binding for fluent coding.</returns>
        public ScalarBinding<T> WithInput<TData>(DependencyProperty dependencyProperty, Scalar<TData> data, Func<T, TData> getValue)
        {
            return WithInput(new PropertyChangedTrigger<T>(dependencyProperty), data, getValue);
        }

        /// <summary>
        /// Sets input implementation with specified scalar data.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="routedEvent">The routed event for flushing.</param>
        /// <param name="progressiveFlushingRoutedEvent">The routed event for progressive flushing.</param>
        /// <param name="data">The scalar data.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This scalar binding for fluent coding.</returns>
        public ScalarBinding<T> WithInput<TData>(RoutedEvent routedEvent, RoutedEvent progressiveFlushingRoutedEvent, Scalar<TData> data, Func<T, TData> getValue)
        {
            return WithInput(new RoutedEventTrigger<T>(routedEvent), new RoutedEventTrigger<T>(progressiveFlushingRoutedEvent), data, getValue);
        }

        /// <summary>
        /// Gets the view element at specified flow index.
        /// </summary>
        /// <param name="flowIndex">The flow index.</param>
        /// <returns>The view element.</returns>
        public new T this[int flowIndex]
        {
            get { return (T)base[flowIndex]; }
        }

        /// <summary>
        /// Applies delegate to setup this binding.
        /// </summary>
        /// <param name="setup">The delegate to setup this binding.</param>
        /// <returns>This scalar binding for fluent coding.</returns>
        public ScalarBinding<T> ApplySetup(Action<T, ScalarPresenter> setup)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));
            _onSetup = _onSetup.Apply(setup);
            return this;
        }

        /// <summary>
        /// Applies delegate to Refresh this binding.
        /// </summary>
        /// <param name="refresh">The delegate to refresh this binding.</param>
        /// <returns>This scalar binding for fluent coding.</returns>
        public ScalarBinding<T> ApplyRefresh(Action<T, ScalarPresenter> refresh)
        {
            if (refresh == null)
                throw new ArgumentNullException(nameof(refresh));
            _onRefresh = _onRefresh.Apply(refresh);
            return this;
        }

        /// <summary>
        /// Applies delegate to cleanup this binding.
        /// </summary>
        /// <param name="cleanup">The delegate to cleanup this binding.</param>
        /// <returns>This scalar binding for fluent coding.</returns>
        public ScalarBinding<T> ApplyCleanup(Action<T, ScalarPresenter> cleanup)
        {
            if (cleanup == null)
                throw new ArgumentNullException(nameof(cleanup));
            _onCleanup = _onCleanup.Apply(cleanup);
            return this;
        }

        private List<IScalarBindingBehavior<T>> _behaviors;
        /// <summary>
        /// Gets the scalar binding behaviiors.
        /// </summary>
        public IReadOnlyList<IScalarBindingBehavior<T>> Behaviors
        {
            get
            {
                if (_behaviors == null)
                    return Array.Empty<IScalarBindingBehavior<T>>();
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
            var containerElement = parent as IContainerElement;
            if (containerElement != null)
                return containerElement.GetChild(index);
            throw new NotSupportedException();
        }
    }
}

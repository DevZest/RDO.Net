using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents strongly typed row level data binding.
    /// </summary>
    /// <typeparam name="T">The type of view element.</typeparam>
    public sealed class RowBinding<T> : RowBindingBase<T>
        where T : UIElement, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RowBinding{T}"/> class.
        /// </summary>
        /// <param name="onRefresh">The delegate to refresh the binding.</param>
        public RowBinding(Action<T, RowPresenter> onRefresh)
        {
            _onRefresh = onRefresh;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RowBinding{T}"/> class.
        /// </summary>
        /// <param name="onRefresh">The delegate to refresh the binding.</param>
        /// <param name="onSetup">The delegate to setup the binding.</param>
        /// <param name="onCleanup">The delegate to cleanup the binding.</param>
        public RowBinding(Action<T, RowPresenter> onRefresh, Action<T, RowPresenter> onSetup, Action<T, RowPresenter> onCleanup)
            : this(onRefresh)
        {
            _onSetup = onSetup;
            _onCleanup = onCleanup;
        }

        /// <inheritdoc/>
        public override IReadOnlyList<RowBinding> ChildBindings
        {
            get { return Array.Empty<RowBinding>(); }
        }

        /// <summary>
        /// Gets the input that handles flushing from view to presenter.
        /// </summary>
        public RowInput<T> Input { get; internal set; }

        /// <inheritdoc/>
        public override Input<RowBinding, IColumns> RowInput
        {
            get { return Input; }
        }

        internal sealed override void FlushInput(UIElement element)
        {
            if (Input != null)
                Input.Flush((T)element);
        }

        private IColumns Columns
        {
            get { return Input == null ? Data.Columns.Empty : Input.Target; }
        }

        internal sealed override void PerformSetup(RowPresenter rowPresenter)
        {
            Setup(SettingUpElement, rowPresenter);
            Refresh(SettingUpElement, rowPresenter);
            if (Input != null)
                Input.Attach(SettingUpElement);
        }

        private Action<T, RowPresenter> _onSetup;
        private void Setup(T element, RowPresenter rowPresenter)
        {
            var behaviors = Behaviors;
            for (int i = 0; i < behaviors.Count; i++)
                behaviors[i].Setup(element, rowPresenter);
            if (_onSetup != null)
                _onSetup(element, rowPresenter);
            var rowElement = element as IRowElement;
            if (rowElement != null)
                rowElement.Setup(rowPresenter);
        }

        private bool _isRefreshing;
        /// <inheritdoc/>
        public override bool IsRefreshing
        {
            get { return _isRefreshing; }
        }

        private Action<T, RowPresenter> _onRefresh;
        internal void Refresh(T element, RowPresenter rowPresenter)
        {
            _isRefreshing = true;
            var behaviors = Behaviors;
            for (int i = 0; i < behaviors.Count; i++)
                behaviors[i].Refresh(element, rowPresenter);
            if (_onRefresh != null)
                _onRefresh(element, rowPresenter);
            var rowElement = element as IRowElement;
            if (rowElement != null)
                rowElement.Refresh(rowPresenter);
            _isRefreshing = false;
        }

        private Action<T, RowPresenter> _onCleanup;
        private void Cleanup(T element, RowPresenter rowPresenter)
        {
            var behaviors = Behaviors;
            for (int i = 0; i < behaviors.Count; i++)
                behaviors[i].Cleanup(element, rowPresenter);
            var rowElement = element as IRowElement;
            if (rowElement != null)
                rowElement.Cleanup(rowPresenter);
            if (_onCleanup != null)
                _onCleanup(element, rowPresenter);
        }

        internal sealed override void Refresh(UIElement element)
        {
            var rowPresenter = element.GetRowPresenter();
            var e = (T)element;
            if (Input != null)
                Input.Refresh(e, rowPresenter);
            else
                Refresh(e, rowPresenter);
        }

        internal sealed override void PerformCleanup(UIElement element)
        {
            var rowPresenter = element.GetRowPresenter();
            var e = (T)element;
            if (Input != null)
                Input.Detach(e);
            Cleanup(e, rowPresenter);
        }

        /// <summary>
        /// Begins input implementation.
        /// </summary>
        /// <param name="flushingTrigger">The flushing trigger.</param>
        /// <param name="progressiveFlushingTrigger">The progressive flushing trigger.</param>
        /// <returns>The row input.</returns>
        public RowInput<T> BeginInput(Trigger<T> flushingTrigger, Trigger<T> progressiveFlushingTrigger = null)
        {
            VerifyNotSealed();
            if (Input != null)
                throw new InvalidOperationException(DiagnosticMessages.TwoWayBinding_InputAlreadyExists);

            return Input = new RowInput<T>(this, flushingTrigger, progressiveFlushingTrigger);
        }

        /// <summary>
        /// Begins input implementation.
        /// </summary>
        /// <param name="dependencyProperty">The dependency property for flushing.</param>
        /// <param name="progressiveFlushingRoutedEvent">The routed event for progressive flushing.</param>
        /// <returns>The row input.</returns>
        public RowInput<T> BeginInput(DependencyProperty dependencyProperty, RoutedEvent progressiveFlushingRoutedEvent = null)
        {
            return BeginInput(new PropertyChangedTrigger<T>(dependencyProperty), progressiveFlushingRoutedEvent == null ? null : new RoutedEventTrigger<T>(progressiveFlushingRoutedEvent));
        }

        /// <summary>
        /// Begins input implementation.
        /// </summary>
        /// <param name="routedEvent">The routed event for flushing.</param>
        /// <param name="progressiveFlushingRoutedEvent">The routed event for progressive flushing.</param>
        /// <returns>The row input.</returns>
        public RowInput<T> BeginInput(RoutedEvent routedEvent, RoutedEvent progressiveFlushingRoutedEvent = null)
        {
            return BeginInput(new RoutedEventTrigger<T>(routedEvent), progressiveFlushingRoutedEvent == null ? null : new RoutedEventTrigger<T>(progressiveFlushingRoutedEvent));
        }

        /// <summary>
        /// Sets input implementation from specified column.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="flushingTrigger">The flushing trigger.</param>
        /// <param name="progressiveFlushingTrigger">The progressive flushing trigger.</param>
        /// <param name="column">The column.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This row binding for fluent coding.</returns>
        public RowBinding<T> WithInput<TData>(Trigger<T> flushingTrigger, Trigger<T> progressiveFlushingTrigger, Column<TData> column, Func<T, TData> getValue)
        {
            return BeginInput(flushingTrigger, progressiveFlushingTrigger).WithFlush(column, getValue).EndInput();
        }

        /// <summary>
        /// Sets input implementation from specified column.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="flushingTrigger">The flushing trigger.</param>
        /// <param name="progressiveFlushingTrigger">The progressive flushing trigger.</param>
        /// <param name="column">The column.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This row binding for fluent coding.</returns>
        public RowBinding<T> WithInput<TData>(Trigger<T> flushingTrigger, Trigger<T> progressiveFlushingTrigger, Column<TData> column, Func<RowPresenter, T, TData> getValue)
        {
            return BeginInput(flushingTrigger, progressiveFlushingTrigger).WithFlush(column, getValue).EndInput();
        }

        /// <summary>
        /// Sets input implementation with specified column.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="flushingTrigger">The flushing trigger.</param>
        /// <param name="column">The column.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This row binding for fluent coding.</returns>
        public RowBinding<T> WithInput<TData>(Trigger<T> flushingTrigger, Column<TData> column, Func<T, TData> getValue)
        {
            return BeginInput(flushingTrigger).WithFlush(column, getValue).EndInput();
        }

        /// <summary>
        /// Sets input implementation with specified column.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="dependencyProperty">The dependency property for flushing.</param>
        /// <param name="progressiveFlushingRoutedEvent">The routed event for progressive flushing.</param>
        /// <param name="column">The column.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This row binding for fluent coding.</returns>
        public RowBinding<T> WithInput<TData>(DependencyProperty dependencyProperty, RoutedEvent progressiveFlushingRoutedEvent, Column<TData> column, Func<T, TData> getValue)
        {
            return WithInput(new PropertyChangedTrigger<T>(dependencyProperty), new RoutedEventTrigger<T>(progressiveFlushingRoutedEvent), column, getValue);
        }

        /// <summary>
        /// Sets input implementation with specified column.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="dependencyProperty">The dependency property for flushing.</param>
        /// <param name="progressiveFlushingRoutedEvent">The routed event for progressive flushing.</param>
        /// <param name="column">The column.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This row binding for fluent coding.</returns>
        public RowBinding<T> WithInput<TData>(DependencyProperty dependencyProperty, RoutedEvent progressiveFlushingRoutedEvent, Column<TData> column, Func<RowPresenter, T, TData> getValue)
        {
            return WithInput(new PropertyChangedTrigger<T>(dependencyProperty), new RoutedEventTrigger<T>(progressiveFlushingRoutedEvent), column, getValue);
        }

        /// <summary>
        /// Sets input implementation with specified column.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="dependencyProperty">The dependency property for flushing.</param>
        /// <param name="column">The column.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This row binding for fluent coding.</returns>
        public RowBinding<T> WithInput<TData>(DependencyProperty dependencyProperty, Column<TData> column, Func<T, TData> getValue)
        {
            return WithInput(new PropertyChangedTrigger<T>(dependencyProperty), column, getValue);
        }

        /// <summary>
        /// Sets input implementation with specified column.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="routedEvent">The routed event for flushing.</param>
        /// <param name="progressiveFlushingRoutedEvent">The routed event for progressive flushing.</param>
        /// <param name="column">The column.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This row binding for fluent coding.</returns>
        public RowBinding<T> WithInput<TData>(RoutedEvent routedEvent, RoutedEvent progressiveFlushingRoutedEvent, Column<TData> column, Func<T, TData> getValue)
        {
            return WithInput(new RoutedEventTrigger<T>(routedEvent), new RoutedEventTrigger<T>(progressiveFlushingRoutedEvent), column, getValue);
        }

        /// <summary>
        /// Sets input implementation with specified column.
        /// </summary>
        /// <typeparam name="TData">Data type of column.</typeparam>
        /// <param name="routedEvent">The routed event for flushing.</param>
        /// <param name="column">The column.</param>
        /// <param name="getValue">The delegate to get data value from view element.</param>
        /// <returns>This row binding for fluent coding.</returns>
        public RowBinding<T> WithInput<TData>(RoutedEvent routedEvent, Column<TData> column, Func<T, TData> getValue)
        {
            return WithInput(new RoutedEventTrigger<T>(routedEvent), column, getValue);
        }

        /// <summary>
        /// Applies delegate to setup this binding.
        /// </summary>
        /// <param name="setup">The delegate to setup this binding.</param>
        /// <returns>This row binding for fluent coding.</returns>
        public RowBinding<T> ApplySetup(Action<T, RowPresenter> setup)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));
            _onSetup = _onSetup.Apply(setup);
            return this;
        }

        /// <summary>
        /// Applies delegate to refresh this binding.
        /// </summary>
        /// <param name="refresh">The delegate to refresh this binding.</param>
        /// <returns>This row binding for fluent coding.</returns>
        public RowBinding<T> ApplyRefresh(Action<T, RowPresenter> refresh)
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
        /// <returns>This row binding for fluent coding.</returns>
        public RowBinding<T> ApplyCleanup(Action<T, RowPresenter> cleanup)
        {
            if (cleanup == null)
                throw new ArgumentNullException(nameof(cleanup));
            _onCleanup = _onCleanup.Apply(cleanup);
            return this;
        }

        private List<IRowBindingBehavior<T>> _behaviors;
        /// <summary>
        /// Gets the row binding behaviors.
        /// </summary>
        public IReadOnlyList<IRowBindingBehavior<T>> Behaviors
        {
            get
            {
                if (_behaviors == null)
                    return Array.Empty<IRowBindingBehavior<T>>();
                else
                    return _behaviors;
            }
        }

        internal void InternalAddBehavior(IRowBindingBehavior<T> behavior)
        {
            Debug.Assert(behavior != null);
            if (_behaviors == null)
                _behaviors = new List<IRowBindingBehavior<T>>();
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

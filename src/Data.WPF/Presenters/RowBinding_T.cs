using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class RowBinding<T> : RowBindingBase<T>
        where T : UIElement, new()
    {
        public RowBinding(Action<T, RowPresenter> onRefresh)
        {
            _onRefresh = onRefresh;
        }

        public RowBinding(Action<T, RowPresenter> onRefresh, Action<T, RowPresenter> onSetup, Action<T, RowPresenter> onCleanup)
            : this(onRefresh)
        {
            _onSetup = onSetup;
            _onCleanup = onCleanup;
        }

        public RowInput<T> Input { get; private set; }

        internal override IRowInput RowInput
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

        internal sealed override void Cleanup(UIElement element)
        {
            var rowPresenter = element.GetRowPresenter();
            var e = (T)element;
            if (Input != null)
                Input.Detach(e);
            Cleanup(e, rowPresenter);
            e.SetRowPresenter(null);
        }

        public RowInput<T> BeginInput(Trigger<T> flushTrigger)
        {
            VerifyNotSealed();
            if (Input != null)
                throw new InvalidOperationException(DiagnosticMessages.TwoWayBinding_InputAlreadyExists);

            return Input = new RowInput<T>(this, flushTrigger);
        }

        public RowInput<T> BeginInput(DependencyProperty dependencyProperty)
        {
            return BeginInput(new PropertyChangedTrigger<T>(dependencyProperty));
        }

        public RowInput<T> BeginInput(RoutedEvent routedEvent)
        {
            return BeginInput(new RoutedEventTrigger<T>(routedEvent));
        }

        public RowBinding<T> WithInput<TData>(Trigger<T> flushTrigger, Column<TData> column, Func<T, TData> getValue)
        {
            return BeginInput(flushTrigger).WithFlush(column, getValue).EndInput();
        }

        public RowBinding<T> WithInput<TData>(DependencyProperty dependencyProperty, Column<TData> column, Func<T, TData> getValue)
        {
            return WithInput(new PropertyChangedTrigger<T>(dependencyProperty), column, getValue);
        }

        public RowBinding<T> WithInput<TData>(RoutedEvent routedEvent, Column<TData> column, Func<T, TData> getValue)
        {
            return WithInput(new RoutedEventTrigger<T>(routedEvent), column, getValue);
        }

        public RowBinding<T> OverrideSetup(Action<T, RowPresenter, Action<T, RowPresenter>> overrideSetup)
        {
            if (overrideSetup == null)
                throw new ArgumentNullException(nameof(overrideSetup));
            _onSetup = _onSetup.Override(overrideSetup);
            return this;
        }

        public RowBinding<T> OverrideRefresh(Action<T, RowPresenter, Action<T, RowPresenter>> overrideRefresh)
        {
            if (overrideRefresh == null)
                throw new ArgumentNullException(nameof(overrideRefresh));
            _onRefresh = _onRefresh.Override(overrideRefresh);
            return this;
        }

        public RowBinding<T> OverrideCleanup(Action<T, RowPresenter, Action<T, RowPresenter>> overrideCleanup)
        {
            if (overrideCleanup == null)
                throw new ArgumentNullException(nameof(overrideCleanup));
            _onCleanup = _onRefresh.Override(overrideCleanup);
            return this;
        }

        private List<IRowBindingBehavior<T>> _behaviors;
        public IReadOnlyList<IRowBindingBehavior<T>> Behaviors
        {
            get
            {
                if (_behaviors == null)
                    return Array<IRowBindingBehavior<T>>.Empty;
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
            throw new NotSupportedException();
        }
    }
}

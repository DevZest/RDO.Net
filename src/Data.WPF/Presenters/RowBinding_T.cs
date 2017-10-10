using DevZest.Data;
using DevZest.Data.Presenters.Plugins;
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

        internal override IRowInput GetInput()
        {
            return Input;
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
            var plugins = Plugins;
            for (int i = 0; i < plugins.Count; i++)
                plugins[i].Setup(element, rowPresenter);
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
            var plugins = Plugins;
            for (int i = 0; i < plugins.Count; i++)
                plugins[i].Refresh(element, rowPresenter);
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
            var plugins = Plugins;
            for (int i = 0; i < plugins.Count; i++)
                plugins[i].Cleanup(element, rowPresenter);
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
                throw new InvalidOperationException(Strings.TwoWayBinding_InputAlreadyExists);

            return Input = new RowInput<T>(this, flushTrigger);
        }

        public RowBinding<T> WithInput<TData>(Trigger<T> flushTrigger, Column<TData> column, Func<T, TData> getValue)
        {
            return BeginInput(flushTrigger).WithFlush(column, getValue).EndInput();
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

        private List<IRowBindingPlugin> _plugins;
        public IReadOnlyList<IRowBindingPlugin> Plugins
        {
            get
            {
                if (_plugins == null)
                    return Array<IRowBindingPlugin>.Empty;
                else
                    return _plugins;
            }
        }

        internal void InternalAddPlugin(IRowBindingPlugin plugin)
        {
            Debug.Assert(plugin != null);
            if (_plugins == null)
                _plugins = new List<IRowBindingPlugin>();
            _plugins.Add(plugin);
        }

        internal override UIElement GetChild(UIElement parent, int index)
        {
            throw new NotSupportedException();
        }
    }
}

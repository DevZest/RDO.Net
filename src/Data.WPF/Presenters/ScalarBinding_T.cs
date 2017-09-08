using DevZest.Data.Presenters.Plugins;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class ScalarBinding<T> : ScalarBinding
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
                _onSetup = (e, sp) => onSetup(e);
            if (onCleanup != null)
                _onCleanup = (e, sp) => onCleanup(e);
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

        private T[] Create(int startOffset)
        {
            _settingUpStartOffset = startOffset;

            if (startOffset == FlowRepeatCount)
                return Array<T>.Empty;

            int count = FlowRepeatCount - startOffset;
            var result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = Create();
                result[i].SetScalarFlowIndex(startOffset + i);
            }
            return result;
        }

        private T Create()
        {
            var result = new T();
            OnCreated(result);
            if (Parent != null)
                result.SetScalarFlowIndex(ScalarPresenter.FlowIndex);
            return result;
        }

        private int _settingUpStartOffset;
        private T[] _settingUpElements;
        private IReadOnlyList<T> SettingUpElements
        {
            get { return _settingUpElements; }
        }

        public T SettingUpElement { get; private set; }

        internal sealed override UIElement GetSettingUpElement()
        {
            Debug.Assert(Parent != null || !FlowRepeatable);
            return SettingUpElement;
        }

        internal sealed override void Initialize(int startOffset)
        {
            if (FlowRepeatable)
                _settingUpElements = Create(startOffset);
            else if (startOffset == 0)
                SettingUpElement = Create();
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            Debug.Assert(Parent != null || !FlowRepeatable);
            SettingUpElement = value == null ? Create() : (T)value;
        }

        internal sealed override UIElement Setup(int flowIndex)
        {
            EnterSetup(flowIndex);

            var result = SettingUpElement;
            Setup(result, ScalarPresenter);
            Refresh(result, ScalarPresenter);
            if (Input != null)
                Input.Attach(result);

            ExitSetup();
            return result;
        }

        internal sealed override void PrepareSettingUpElement(int flowIndex)
        {
            if (FlowRepeatable)
            {
                Debug.Assert(SettingUpElements != null);
                SettingUpElement = SettingUpElements[flowIndex - _settingUpStartOffset];
            }
        }

        internal override void ClearSettingUpElement()
        {
            if (FlowRepeatable)
                SettingUpElement = null;
        }

        internal sealed override void EndSetup()
        {
            _settingUpElements = null;
            SettingUpElement = null;
        }

        private Action<T, ScalarPresenter> _onSetup;
        private void Setup(T element, ScalarPresenter scalarPresenter)
        {
            var plugins = Plugins;
            for (int i = 0; i < plugins.Count; i++)
                plugins[i].Setup(element, scalarPresenter);
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
        private void Refresh(T element, ScalarPresenter scalarPresenter)
        {
            _isRefreshing = true;
            var plugins = Plugins;
            for (int i = 0; i < plugins.Count; i++)
                plugins[i].Refresh(element, scalarPresenter);
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
            var plugins = Plugins;
            for (int i = 0; i < plugins.Count; i++)
                plugins[i].Cleanup(element, scalarPresenter);
            var scalarElement = element as IScalarElement;
            if (scalarElement != null)
                scalarElement.Cleanup(scalarPresenter);
            if (_onCleanup != null)
                _onCleanup(element, scalarPresenter);
        }

        private T Restore(UIElement element)
        {
            var result = (T)element;
            ScalarPresenter.SetFlowIndex(element.GetScalarFlowIndex());
            return result;
        }

        internal sealed override void Refresh(UIElement element)
        {
            var e = Restore(element);
            if (Input != null)
                Input.Refresh(e, ScalarPresenter);
            else
                Refresh(e, ScalarPresenter);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var e = Restore(element);
            if (Input != null)
                Input.Detach(e);
            Cleanup(e, ScalarPresenter);
            e.SetScalarFlowIndex(0);
        }

        public ScalarInput<T> BeginInput(Trigger<T> flushTrigger)
        {
            VerifyNotSealed();
            if (Input != null)
                throw new InvalidOperationException(Strings.TwoWayBinding_InputAlreadyExists);

            return Input = new ScalarInput<T>(this, flushTrigger);
        }

        public ScalarBinding<T> WithInput<TData>(Trigger<T> flushTrigger, Scalar<TData> data, Func<T, TData> getValue)
        {
            return BeginInput(flushTrigger).WithFlush(data, getValue).EndInput();
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

        private List<IScalarBindingPlugin> _plugins;
        public IReadOnlyList<IScalarBindingPlugin> Plugins
        {
            get
            {
                if (_plugins == null)
                    return Array<IScalarBindingPlugin>.Empty;
                else
                    return _plugins;
            }
        }

        internal void InternalAddPlugin(IScalarBindingPlugin plugin)
        {
            Debug.Assert(plugin != null);
            if (_plugins == null)
                _plugins = new List<IScalarBindingPlugin>();
            _plugins.Add(plugin);
        }
    }
}

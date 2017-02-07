using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarBinding<T> : ScalarBinding
        where T : UIElement, new()
    {
        public ScalarBinding(Action<T> onRefresh)
        {
            _onRefresh = onRefresh;
        }

        public ScalarBinding(Action<T> onRefresh, Action<T> onSetup, Action<T> onCleanup)
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

        List<T> _cachedElements;

        private T Create()
        {
            var result = new T();
            OnCreated(result);
            return result;
        }

        public T SettingUpElement { get; private set; }

        internal sealed override UIElement GetSettingUpElement()
        {
            return SettingUpElement;
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            SettingUpElement = (T)value;
        }

        internal sealed override void BeginSetup()
        {
            SettingUpElement = CachedList.GetOrCreate(ref _cachedElements, Create);
        }

        internal sealed override UIElement Setup()
        {
            Debug.Assert(SettingUpElement != null);
            Setup(SettingUpElement);
            Refresh(SettingUpElement);
            if (Input != null)
                Input.Attach(SettingUpElement);
            return SettingUpElement;
        }

        internal sealed override void EndSetup()
        {
            SettingUpElement = null;
        }

        private Action<T> _onSetup;
        private void Setup(T element)
        {
            if (_onSetup != null)
                _onSetup(element);
        }

        private Action<T> _onRefresh;
        private void Refresh(T element)
        {
            if (_onRefresh != null)
                _onRefresh(element);
        }

        private Action<T> _onCleanup;
        private void Cleanup(T element)
        {
            if (_onCleanup != null)
                _onCleanup(element);
        }

        internal sealed override void Refresh(UIElement element)
        {
            var e = (T)element;
            if (Input != null)
                Input.Refresh(e);
            else
                Refresh(e);
        }

        internal sealed override void Cleanup(UIElement element, bool recycle)
        {
            var e = (T)element;
            if (Input != null)
                Input.Detach(e);
            Cleanup(e);
            if (recycle)
                CachedList.Recycle(ref _cachedElements, e);
        }

        public ScalarBinding<T> WithIsMultidimensional(bool value)
        {
            VerifyNotSealed();
            IsMultidimensional = value;
            return this;
        }

        public ScalarInput<T> BeginInput(Trigger<T> flushTrigger)
        {
            if (Input != null)
                throw new InvalidOperationException();

            return Input = new Windows.ScalarInput<T>(this, flushTrigger);
        }

        public ScalarBinding<T> WithInput<TData>(Trigger<T> flushTrigger, Scalar<TData> data, Func<T, TData> getValue)
        {
            return BeginInput(flushTrigger).WithFlush(data, getValue).EndInput();
        }
    }
}

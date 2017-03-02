using DevZest.Windows.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Windows.Data
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

            if (startOffset == FlowCount)
                return Array<T>.Empty;

            int count = FlowCount - startOffset;
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
            Debug.Assert(Parent != null || !Flowable);
            return SettingUpElement;
        }

        internal sealed override void BeginSetup(int startOffset)
        {
            if (Flowable)
                _settingUpElements = Create(startOffset);
            else if (startOffset == 0)
                SettingUpElement = Create();
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            Debug.Assert(Parent != null || !Flowable);
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
            if (Flowable)
            {
                Debug.Assert(SettingUpElements != null);
                SettingUpElement = SettingUpElements[flowIndex - _settingUpStartOffset];
            }
        }

        internal override void ClearSettingUpElement()
        {
            if (Flowable)
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
            if (_onSetup != null)
                _onSetup(element, scalarPresenter);
        }

        private Action<T, ScalarPresenter> _onRefresh;
        private void Refresh(T element, ScalarPresenter scalarPresenter)
        {
            if (_onRefresh != null)
                _onRefresh(element, scalarPresenter);
        }

        private Action<T, ScalarPresenter> _onCleanup;
        private void Cleanup(T element, ScalarPresenter scalarPresenter)
        {
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
    }
}

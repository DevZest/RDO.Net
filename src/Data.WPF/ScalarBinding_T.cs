﻿using DevZest.Data.Windows.Primitives;
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

        private T[] Create(int startOffset)
        {
            _settingUpStartOffset = startOffset;

            if (startOffset == BlockDimensions)
                return Array<T>.Empty;

            int count = BlockDimensions - startOffset;
            var result = new T[count];
            for (int i = 0; i < count; i++)
                result[i] = Create();
            return result;
        }

        private T Create()
        {
            var result = new T();
            OnCreated(result);
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
            Debug.Assert(!IsMultidimensional);
            return SettingUpElement;
        }

        internal sealed override void BeginSetup(int startOffset)
        {
            if (IsMultidimensional)
                _settingUpElements = Create(startOffset);
            else if (startOffset == 0)
                SettingUpElement = Create();
        }

        internal sealed override void BeginSetup(UIElement value)
        {
            Debug.Assert(!IsMultidimensional);
            SettingUpElement = value == null ? Create() : (T)value;
        }

        internal sealed override UIElement Setup(int blockDimension)
        {
            if (IsMultidimensional)
            {
                Debug.Assert(SettingUpElements != null);
                SettingUpElement = SettingUpElements[blockDimension - _settingUpStartOffset];
            }

            return Setup();
        }

        private UIElement Setup()
        {
            var result = SettingUpElement;
            Setup(result);
            Refresh(result);
            if (Input != null)
                Input.Attach(result);
            SettingUpElement = null;
            return result;
        }

        internal sealed override void EndSetup()
        {
            _settingUpElements = null;
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

        internal sealed override void Cleanup(UIElement element)
        {
            var e = (T)element;
            if (Input != null)
                Input.Detach(e);
            Cleanup(e);
        }

        public ScalarBinding<T> WithIsMultidimensional(bool value)
        {
            VerifyNotSealed();
            IsMultidimensional = value;
            return this;
        }

        public ScalarInput<T> BeginInput(Trigger<T> flushTrigger)
        {
            VerifyNotSealed();
            if (Input != null)
                throw new InvalidOperationException(Strings.TwoWayBinding_InputAlreadyExists);

            return Input = new Windows.ScalarInput<T>(this, flushTrigger);
        }

        public ScalarBinding<T> WithInput<TData>(Trigger<T> flushTrigger, Scalar<TData> data, Func<T, TData> getValue)
        {
            return BeginInput(flushTrigger).WithFlush(data, getValue).EndInput();
        }

        public new T this[int blockDimension]
        {
            get { return (T)base[blockDimension]; }
        }
    }
}

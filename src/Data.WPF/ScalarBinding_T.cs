using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ScalarBinding<T> : ScalarBinding
        where T : UIElement, new()
    {
        private ScalarInput<T> _input;
        public ScalarInput<T> Input
        {
            get { return _input; }
            protected set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                VerifyNotSealed();
                value.Seal(this);
                _input = value;
            }
        }

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

        protected virtual void Setup(T element)
        {
        }

        protected abstract void Refresh(T element);

        protected virtual void Cleanup(T element)
        {
        }

        internal sealed override void Refresh(UIElement element)
        {
            var e = (T)element;
            if (Input != null)
                Input.RefreshValidation(e);
            if (!HasInputError(element))
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

        private bool HasInputError(UIElement element)
        {
            return Input == null ? false : Input.HasInputError(element);
        }
    }
}

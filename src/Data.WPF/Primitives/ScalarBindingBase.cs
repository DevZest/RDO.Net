using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ScalarBindingBase<T> : ScalarBinding
        where T : UIElement, new()
    {
        private ReverseScalarBinding<T> _reverseBinding;
        public ReverseScalarBinding<T> ReverseBinding
        {
            get { return _reverseBinding; }
            protected set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                VerifyNotSealed();
                _reverseBinding = value;
                Input.Seal(this);
            }
        }

        public Input<T> Input
        {
            get { return _reverseBinding == null ? null : _reverseBinding.Input; }
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

        internal sealed override void BeginSetup()
        {
            SettingUpElement = CachedList.GetOrCreate(ref _cachedElements, Create);
        }

        internal sealed override UIElement Setup()
        {
            Debug.Assert(SettingUpElement != null);
            Setup(SettingUpElement);
            Refresh(SettingUpElement);
            return SettingUpElement;
        }

        internal sealed override void EndSetup()
        {
            SettingUpElement = null;
        }

        protected abstract void Setup(T element);

        protected abstract void Refresh(T element);

        protected abstract void Cleanup(T element);

        internal sealed override void Refresh(UIElement element)
        {
            Refresh((T)element);
        }

        internal sealed override void Cleanup(UIElement element)
        {
            var e = (T)element;
            Cleanup(e);
            CachedList.Recycle(ref _cachedElements, e);
        }

        internal sealed override bool ShouldRefresh(bool isReload, UIElement element)
        {
            return isReload || Input == null ? true : Input.ShouldRefresh((T)element);
        }
    }
}

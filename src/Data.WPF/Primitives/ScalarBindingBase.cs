using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ScalarBindingBase<T> : ScalarBinding
        where T : UIElement, new()
    {
        private ScalarReverseBinding<T> _reverseBinding;
        public ScalarReverseBinding<T> ReverseBinding
        {
            get { return _reverseBinding; }
            protected set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                VerifyNotSealed();
                value.Seal(this);
                _reverseBinding = value;
            }
        }

        internal sealed override void FlushReverseBinding(UIElement element)
        {
            if (ReverseBinding != null)
                ReverseBinding.Flush((T)element);
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
            if (ReverseBinding != null)
                ReverseBinding.Attach(SettingUpElement);
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
            if (ReverseBinding != null)
                ReverseBinding.Detach(e);
            Cleanup(e);
            CachedList.Recycle(ref _cachedElements, e);
        }

        internal sealed override bool ShouldRefresh(UIElement element)
        {
            return _reverseBinding == null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class GridItem<T> : GridItem
        where T : UIElement, new()
    {
        protected GridItem(Model parentModel)
            : base(parentModel)
        {
        }

        private Action<T> _initializer;
        public Action<T> Initializer
        {
            get { return _initializer; }
            set
            {
                VerifyNotSealed();
                _initializer = value;
            }
        }

        List<T> _cachedUIElements;
        private T GetOrCreate()
        {
            if (_cachedUIElements == null || _cachedUIElements.Count == 0)
                return new T();

            var last = _cachedUIElements.Count - 1;
            var result = _cachedUIElements[last];
            _cachedUIElements.RemoveAt(last);
            return result;
        }

        private void Recycle(T uiElement)
        {
            Debug.Assert(uiElement != null);
            if (_cachedUIElements == null)
                _cachedUIElements = new List<T>();
            _cachedUIElements.Add(uiElement);
        }

        internal sealed override UIElement Generate()
        {
            return GetOrCreate();
        }

        internal sealed override void Initialize(UIElement uiElement)
        {
            var element = (T)uiElement;
            if (Initializer != null)
                Initializer(element);
            OnMounted(element);
        }

        protected virtual void OnMounted(T uiElement)
        {
        }

        internal sealed override void Recycle(UIElement uiElement)
        {
            var element = (T)uiElement;
            OnUnmounting(element);
            Recycle(element);
        }

        protected virtual void OnUnmounting(T uiElement)
        {
        }
    }
}

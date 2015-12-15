using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ScalarGridItem<T> : ScalarGridItem
        where T : UIElement, new()
    {
        protected ScalarGridItem(Model parentModel)
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

        internal sealed override UIElement Create()
        {
            return new T();
        }

        internal sealed override void OnMounted(UIElement uiElement)
        {
            var element = (T)uiElement;
            if (Initializer != null)
                Initializer(element);
            OnMounted(element);
        }

        protected virtual void OnMounted(T uiElement)
        {
        }

        internal sealed override void OnUnmounting(UIElement uiElement)
        {
            OnUnmounting((T)uiElement);
        }

        protected virtual void OnUnmounting(T uiElement)
        {
        }
    }
}

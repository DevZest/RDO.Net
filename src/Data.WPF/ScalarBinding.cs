using DevZest.Data.Windows.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ScalarBinding<T> : ScalarBindingBase<T>
        where T : UIElement, new()
    {
        private Action<T> _onSetup;
        public Action<T> OnSetup
        {
            get { return _onSetup; }
            set
            {
                VerifyNotSealed();
                _onSetup = value;
            }
        }

        protected sealed override void Setup(T element)
        {
            if (OnSetup != null)
                OnSetup(element);
        }

        private Action<T> _onRefresh;
        public Action<T> OnRefresh
        {
            get { return _onRefresh; }
            set
            {
                VerifyNotSealed();
                _onRefresh = value;
            }
        }

        protected sealed override void Refresh(T element)
        {
            if (OnRefresh != null)
                OnRefresh(element);
        }

        private Action<T> _onCleanup;
        public Action<T> OnCleanup
        {
            get { return _onCleanup; }
            set
            {
                VerifyNotSealed();
                _onCleanup = value;
            }
        }

        protected sealed override void Cleanup(T element)
        {
            if (OnCleanup != null)
                OnCleanup(element);
        }
    }
}

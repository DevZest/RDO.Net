using DevZest.Data.Windows.Primitives;
using System.Windows;
using System;

namespace DevZest.Data.Windows
{
    public sealed class RowBinding<T> : RowBindingBase<T>
        where T : UIElement, new()
    {
        private Action<T, RowPresenter> _onSetup;
        public Action<T, RowPresenter> OnSetup
        {
            get { return _onSetup; }
            set
            {
                VerifyNotSealed();
                _onSetup = value;
            }
        }

        protected sealed override void Setup(T element, RowPresenter rowPresenter)
        {
            if (OnSetup != null)
                OnSetup(element, rowPresenter);
        }

        private Action<T, RowPresenter> _onRefresh;
        public Action<T, RowPresenter> OnRefresh
        {
            get { return _onRefresh; }
            set
            {
                VerifyNotSealed();
                _onRefresh = value;
            }
        }

        protected sealed override void Refresh(T element, RowPresenter rowPresenter)
        {
            if (OnRefresh != null)
                OnRefresh(element, rowPresenter);
        }

        private Action<T, RowPresenter> _onCleanup;
        public Action<T, RowPresenter> OnCleanup
        {
            get { return _onCleanup; }
            set
            {
                VerifyNotSealed();
                _onCleanup = value;
            }
        }

        protected sealed override void Cleanup(T element, RowPresenter rowPresenter)
        {
            if (OnCleanup != null)
                OnCleanup(element, rowPresenter);
        }
    }
}

using DevZest.Data.Windows.Primitives;
using System.Windows;
using System;

namespace DevZest.Data.Windows
{
    public class RowBinding<T> : RowBindingBase<T>
        where T : UIElement, new()
    {
        public Action<T, RowPresenter> OnSetup { get; set; }

        protected sealed override void Setup(T element, RowPresenter rowPresenter)
        {
            if (OnSetup != null)
                OnSetup(element, rowPresenter);
        }

        public Action<T, RowPresenter> OnRefresh { get; set; }

        protected sealed override void Refresh(T element, RowPresenter rowPresenter)
        {
            if (OnRefresh != null)
                OnRefresh(element, rowPresenter);
        }

        public Action<T, RowPresenter> OnCleanup { get; set; }

        protected sealed override void Cleanup(T element, RowPresenter rowPresenter)
        {
            if (OnCleanup != null)
                OnCleanup(element, rowPresenter);
        }
    }
}

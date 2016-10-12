using DevZest.Data.Windows.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Windows
{
    public class ScalarBinding<T> : ScalarBindingBase<T>
        where T : UIElement, new()
    {
        public Action<T> OnSetup { get; set; }

        protected sealed override void Setup(T element)
        {
            if (OnSetup != null)
                OnSetup(element);
        }

        public Action<T> OnRefresh { get; set; }

        protected sealed override void Refresh(T element)
        {
            if (OnRefresh != null)
                OnRefresh(element);
        }

        public Action<T> OnCleanup { get; set; }

        protected sealed override void Cleanup(T element)
        {
            if (OnCleanup != null)
                OnCleanup(element);
        }
    }
}

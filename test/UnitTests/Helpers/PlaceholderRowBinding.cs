using DevZest.Data.Windows.Primitives;
using System;

namespace DevZest.Data.Windows.Helpers
{
    public sealed class PlaceholderRowBinding : RowBindingBase<Placeholder>
    {
        public PlaceholderRowBinding()
        {
        }

        public PlaceholderRowBinding(double desiredWidth, double desiredHeight)
        {
            DesiredWidth = desiredWidth;
            DesiredHeight = desiredHeight;
        }

        public double DesiredWidth { get; private set; }

        public double DesiredHeight { get; private set; }

        protected override void Setup(Placeholder element, RowPresenter rowPresenter)
        {
            element.DesiredWidth = DesiredWidth;
            element.DesiredHeight = DesiredHeight;
        }

        private Action<Placeholder, RowPresenter> _onRefresh;
        public Action<Placeholder, RowPresenter> OnRefresh
        {
            get { return _onRefresh; }
            set
            {
                VerifyNotSealed();
                _onRefresh = value;
            }
        }

        protected override void Refresh(Placeholder element, RowPresenter rowPresenter)
        {
            if (OnRefresh != null)
                OnRefresh(element, rowPresenter);
        }

        private Action<Placeholder, RowPresenter> _onCleanup;
        public Action<Placeholder, RowPresenter> OnCleanup
        {
            get { return _onCleanup; }
            set
            {
                VerifyNotSealed();
                _onCleanup = value;
            }
        }

        protected override void Cleanup(Placeholder element, RowPresenter rowPresenter)
        {
            if (OnCleanup != null)
                OnCleanup(element, rowPresenter);
        }
    }
}

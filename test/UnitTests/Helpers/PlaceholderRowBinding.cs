using DevZest.Data.Windows.Primitives;
using System;

namespace DevZest.Data.Windows.Helpers
{
    public class PlaceholderRowBinding : RowBindingBase<Placeholder>
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

        public Action<Placeholder, RowPresenter> OnRefresh { get; set; }

        protected override void Refresh(Placeholder element, RowPresenter rowPresenter)
        {
            if (OnRefresh != null)
                OnRefresh(element, rowPresenter);
        }

        public Action<Placeholder, RowPresenter> OnCleanup { get; set; }

        protected override void Cleanup(Placeholder element, RowPresenter rowPresenter)
        {
            if (OnCleanup != null)
                OnCleanup(element, rowPresenter);
        }
    }
}

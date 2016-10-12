using System;
using DevZest.Data.Windows.Primitives;

namespace DevZest.Data.Windows.Helpers
{
    public class PlaceholderScalarBinding : ScalarBindingBase<Placeholder>
    {
        public PlaceholderScalarBinding()
        {
        }

        public PlaceholderScalarBinding(double desiredWidth, double desiredHeight)
        {
            DesiredWidth = desiredWidth;
            DesiredHeight = desiredHeight;
        }

        public double DesiredWidth { get; private set; }

        public double DesiredHeight { get; private set; }

        protected override void Setup(Placeholder element)
        {
            element.DesiredWidth = DesiredWidth;
            element.DesiredHeight = DesiredHeight;
        }

        public Action<Placeholder> OnRefresh { get; set; }

        protected override void Refresh(Placeholder element)
        {
            if (OnRefresh != null)
                OnRefresh(element);
        }

        public Action<Placeholder> OnCleanup { get; set; }

        protected override void Cleanup(Placeholder element)
        {
            if (OnCleanup != null)
                OnCleanup(element);
        }
    }
}

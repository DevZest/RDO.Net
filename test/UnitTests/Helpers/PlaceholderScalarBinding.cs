using System;
using DevZest.Data.Windows.Primitives;

namespace DevZest.Data.Windows.Helpers
{
    public sealed class PlaceholderScalarBinding : ScalarBindingBase<Placeholder>
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

        private Action<Placeholder> _onRefresh;
        public Action<Placeholder> OnRefresh
        {
            get { return _onRefresh; }
            set
            {
                VerifyNotSealed();
                _onRefresh = value;
            }
        }

        protected override void Refresh(Placeholder element)
        {
            if (OnRefresh != null)
                OnRefresh(element);
        }

        private Action<Placeholder> _onCleanup;
        public Action<Placeholder> OnCleanup
        {
            get { return _onCleanup; }
            set
            {
                VerifyNotSealed();
                _onCleanup = value;
            }
        }

        protected override void Cleanup(Placeholder element)
        {
            if (OnCleanup != null)
                OnCleanup(element);
        }
    }
}

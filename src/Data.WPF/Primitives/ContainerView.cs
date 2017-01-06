using System.Diagnostics;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class ContainerView : Control
    {
        public abstract int ContainerOrdinal { get; }

        internal abstract ElementManager ElementManager { get; }

        internal LayoutXYManager LayoutXYManager
        {
            get { return ElementManager as LayoutXYManager; }
        }

        internal abstract void Reload();

        internal abstract void OnCurrentRowChanged(RowPresenter oldCurrentRow, bool reload);

        private GridSpan VariantByContainerGridSpan
        {
            get { return LayoutXYManager.VariantByContainerGridSpan; }
        }

        private double[] _cumulativeMeasuredLengths;
        private double[] CumulativeMeasuredLengths
        {
            get
            {
                Debug.Assert(VariantByContainerGridSpan.Count > 0);
                return _cumulativeMeasuredLengths ?? (_cumulativeMeasuredLengths = InitCumulativeMeasuredLengths());
            }
        }

        private double[] InitCumulativeMeasuredLengths()
        {
            Debug.Assert(VariantByContainerGridSpan.Count > 0);
            var result = new double[VariantByContainerGridSpan.Count];
            ClearMeasuredLengths();
            return result;
        }

        protected void ClearMeasuredLengths()
        {
            if (_cumulativeMeasuredLengths == null)
                return;

            double totalLength = 0;
            var gridSpan = LayoutXYManager.VariantByContainerGridSpan;
            Debug.Assert(gridSpan.Count == _cumulativeMeasuredLengths.Length);
            for (int i = 0; i < _cumulativeMeasuredLengths.Length; i++)
            {
                var gridTrack = gridSpan[i];
                if (!gridTrack.IsAutoLength)
                    totalLength += gridTrack.Length.Value;
                _cumulativeMeasuredLengths[i] = totalLength;
            }

            _startOffset = 0;
        }

        internal Span GetReleativeSpan(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack != null && gridTrack.VariantByContainer);
            return new Span(GetRelativeStartOffset(gridTrack), GetRelativeEndOffset(gridTrack));
        }

        private double GetRelativeStartOffset(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack != null && gridTrack.VariantByContainer);
            return GetRelativeEndOffset(gridTrack) - GetMeasuredLength(gridTrack);
        }

        private double GetRelativeEndOffset(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack != null && gridTrack.VariantByContainer);
            return CumulativeMeasuredLengths[gridTrack.VariantByContainerIndex];
        }

        internal double GetMeasuredLength(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack != null && gridTrack.VariantByContainer);
            int index = gridTrack.VariantByContainerIndex;
            return index == 0 ? CumulativeMeasuredLengths[0] : CumulativeMeasuredLengths[index] - CumulativeMeasuredLengths[index - 1];
        }

        internal void SetMeasuredLength(GridTrack gridTrack, double value)
        {
            Debug.Assert(gridTrack != null && gridTrack.VariantByContainer);
            var oldValue = GetMeasuredLength(gridTrack);
            var delta = value - oldValue;
            if (delta == 0)
                return;

            var index = gridTrack.VariantByContainerIndex;
            for (int i = index; i < CumulativeMeasuredLengths.Length; i++)
                CumulativeMeasuredLengths[i] += delta;
            LayoutXYManager.InvalidateContainerLengths();
        }

        private double MeasuredLength
        {
            get
            {
                Debug.Assert(VariantByContainerGridSpan.Count > 0);
                var cumulativeMeasuredLengths = CumulativeMeasuredLengths;
                return cumulativeMeasuredLengths[cumulativeMeasuredLengths.Length - 1];
            }
        }

        private double _startOffset;
        internal double StartOffset
        {
            get
            {
                Debug.Assert(VariantByContainerGridSpan.Count > 0);
                LayoutXYManager.RefreshContainerLengths();
                return _startOffset;
            }
            set
            {
                Debug.Assert(VariantByContainerGridSpan.Count > 0);
                _startOffset = value;
            }
        }

        internal double EndOffset
        {
            get { return StartOffset + MeasuredLength; }
        }

        internal abstract void Refresh();

        internal abstract void ReloadIfInvalid();

        internal abstract void Cleanup();

        protected virtual void OnSetup()
        {
        }

        internal void InternalCleanup()
        {
            OnCleanup();
            ClearMeasuredLengths();
        }

        protected virtual void OnCleanup()
        {
        }
    }
}

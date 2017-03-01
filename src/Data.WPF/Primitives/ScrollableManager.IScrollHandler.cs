using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    partial class ScrollableManager
    {
        public ScrollViewer ScrollOwner { get; set; }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        public FrameworkElement Panel
        {
            get { return ElementCollection.Parent; }
        }

        private void InvalidateMeasure()
        {
            if (Panel != null)
                Panel.InvalidateMeasure();
        }

        public abstract double ViewportWidth { get; }

        public abstract double ViewportHeight { get; }

        protected double ViewportMain { get; private set; }

        protected double ViewportCross { get; private set; }

        private void RefreshViewport(double valueMain, double valueCross)
        {
            if (ViewportMain.IsClose(valueMain) && ViewportCross.IsClose(valueCross))
                return;
            ViewportMain = valueMain;
            ViewportCross = valueCross;
            InvalidateScrollInfo();
        }

        public abstract double ExtentHeight { get; }

        public abstract double ExtentWidth { get; }

        protected double ExtentMain { get; private set; }

        protected double ExtentCross { get; private set; }

        private void RefreshExtent(double valueMain, double valueCross)
        {
            if (ExtentMain.IsClose(valueMain) && ExtentCross.IsClose(valueCross))
                return;
            ExtentMain = valueMain;
            ExtentCross = valueCross;
            InvalidateScrollInfo();
        }

        public abstract double HorizontalOffset { get; }

        public abstract double VerticalOffset { get; }

        private LogicalExtent _scrollToMain;
        private GridPlacement _scrollToMainPlacement;

        protected double ScrollOffsetMain { get; private set; }

        private double _scrollDeltaMain;
        private void SetScrollDeltaMain(double value, bool invalidateMeasure)
        {
            if (_scrollDeltaMain.IsClose(value))
                return;
            _scrollDeltaMain = value;
            if (invalidateMeasure)
                InvalidateMeasure();
        }

        private double _scrollDeltaCross;
        private void SetScrollDeltaCross(double value, bool invalidateMeasure)
        {
            if (_scrollDeltaCross.IsClose(value))
                return;
            _scrollDeltaCross = value;
            if (invalidateMeasure)
                InvalidateMeasure();
        }

        private static double CoerceScrollOffset(double value, double maxValue)
        {
            if (value < 0)
                value = 0;
            if (value > maxValue)
                value = maxValue;
            return value;
        }

        protected double ScrollOffsetCross { get; private set; }

        private void RefreshScollOffset(double valueMain, double valueCross)
        {
            bool invalidateScrollInfo = !ScrollOffsetMain.IsClose(valueMain) || !ScrollOffsetCross.IsClose(valueCross);
            ScrollOffsetMain = valueMain;
            ScrollOffsetCross = valueCross;
            SetScrollDeltaMain(0, false);
            SetScrollDeltaCross(0, false);
            if (invalidateScrollInfo)
                InvalidateScrollInfo();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            EnsureVisible(visual);
            return rectangle;
        }

        public abstract int MaxGridExtentX { get; }

        public abstract int MaxGridExtentY { get; }

        protected int MaxGridExtentMain
        {
            get { return HeadTracksCountMain + TotalContainerGridTracksMain + TailTracksCountMain; }
        }

        protected int MaxGridExtentCross
        {
            get
            {
                return HeadTracksCountCross + TailTracksCountCross
                  + FlowCount * RowTracksCountCross + (ContainerTracksCountCross - RowTracksCountCross);
            }
        }

        public double GetExtentX(int gridExtentX)
        {
            VerifyGridExtent(gridExtentX, nameof(gridExtentX), MaxGridExtentX);
            return GetExtentXCore(gridExtentX);
        }

        protected abstract double GetExtentXCore(int gridExtentX);

        public double GetExtentY(int gridExtentY)
        {
            VerifyGridExtent(gridExtentY, nameof(gridExtentY), MaxGridExtentY);
            return GetExtentYCore(gridExtentY);
        }

        protected abstract double GetExtentYCore(int gridExtentY);

        protected double GetExtentMain(int gridExtent)
        {
            Debug.Assert(gridExtent >= 0);
            return gridExtent >= MaxGridExtentMain
                ? GetLogicalMainTrack(MaxGridExtentMain - 1).EndExtent
                : GetLogicalMainTrack(gridExtent).StartExtent;
        }

        protected double GetExtentCross(int gridExtent)
        {
            Debug.Assert(gridExtent >= 0);
            return gridExtent >= MaxGridExtentCross
                ? GetLogicalCrossTrack(MaxGridExtentCross - 1).EndExtent
                : GetLogicalCrossTrack(gridExtent).StartExtent;
        }

        public double GetPositionX(int gridExtentX, GridPlacement placement)
        {
            VerifyGridExtent(gridExtentX, nameof(gridExtentX), placement, nameof(placement), MaxGridExtentX);
            return GetPositionXCore(gridExtentX, placement);
        }

        protected abstract double GetPositionXCore(int gridExtentX, GridPlacement placement);

        public double GetPositionY(int gridExtentY, GridPlacement placement)
        {
            VerifyGridExtent(gridExtentY, nameof(gridExtentY), placement, nameof(placement), MaxGridExtentY);
            return GetPositionYCore(gridExtentY, placement);
        }

        protected abstract double GetPositionYCore(int gridExtentY, GridPlacement placement);

        protected double GetPositionMain(int gridExtent, GridPlacement placement)
        {
            return placement == GridPlacement.Tail
                ? GetLogicalMainTrack(gridExtent - 1).EndPosition
                : GetLogicalMainTrack(gridExtent).StartPosition;
        }

        protected double GetPositionCross(int gridExtent, GridPlacement placement)
        {
            return placement == GridPlacement.Tail
                ? GetLogicalCrossTrack(gridExtent - 1).EndPosition
                : GetLogicalCrossTrack(gridExtent).StartPosition;
        }

        private void VerifyGridExtent(int gridExtent, string gridExtentParamName, int maxGridExtent)
        {
            if (gridExtent < 0 || gridExtent > maxGridExtent)
                throw new ArgumentOutOfRangeException(gridExtentParamName);
        }

        private void VerifyGridExtent(int gridExtent, string gridExtentParamName,
            GridPlacement placement, string placementParamName, int maxGridExtent)
        {
            VerifyGridExtent(gridExtent, gridExtentParamName, maxGridExtent);

            if (gridExtent == 0 && placement == GridPlacement.Tail)
                throw new ArgumentException(Strings.GridPlacement_InvalidTailValue, placementParamName);

            if (gridExtent == maxGridExtent && placement == GridPlacement.Head)
                throw new ArgumentException(Strings.GridPlacement_InvalidHeadValue, placementParamName);
        }

        public void ScrollTo(int gridExtent, double fraction, GridPlacement placement)
        {
            if (_scrollToMain.GridExtent == gridExtent && _scrollToMain.Fraction.IsClose(fraction)
                && _scrollToMainPlacement == placement
                && _scrollDeltaMain == 0)
                return;
            _scrollToMain = new LogicalExtent(gridExtent, fraction);
            _scrollToMainPlacement = placement;
            SetScrollDeltaMain(0, false);
            InvalidateMeasure();
        }


        public void ScrollTo(double horizontalOffset, double verticalOffset)
        {
            var offsetX = double.IsNaN(horizontalOffset) ? 0 : horizontalOffset - HorizontalOffset;
            var offsetY = double.IsNaN(verticalOffset) ? 0 : verticalOffset - VerticalOffset;
            ScrollBy(offsetX, offsetY);
        }

        public abstract void ScrollBy(double x, double y);

        protected void InternalScrollBy(double valueMain, double valueCross)
        {
            SetScrollDeltaMain(valueMain, true);
            SetScrollDeltaCross(valueCross, true);
        }

        protected sealed override void PrepareMeasureContainers()
        {
            if (!UpdateScrollToMain())
                CoerceScrollToMain();
            InitContainerViews();

            if (_scrollToMainPlacement == GridPlacement.Head)
                FillForward();
            else
                FillBackward();

        }

        private bool UpdateScrollToMain()
        {
            if (_scrollDeltaMain == 0 || Math.Abs(_scrollDeltaMain) <= ViewportMain)
                return false;

            AdjustScrollToMain(false);
            return true;
        }

        private void AdjustScrollToMain(bool assertRealized)
        {
            AdjustScrollToMain(_scrollDeltaMain, assertRealized);
        }

        private void AdjustScrollToMain(double delta, bool assertRealized)
        {
            _scrollToMain = Translate(ScrollToMainExtent + delta);
            SetScrollDeltaMain(0, false);
            CoerceScrollToMain();
            if (assertRealized)
                AssertAdjustedScrollToMain();
        }

        [Conditional("DEBUG")]
        private void AssertAdjustedScrollToMain()
        {
            var scrollTo = GetLogicalMainTrack(_scrollToMain.GridExtent);
            Debug.Assert(!scrollTo.IsEof);

            if (scrollTo.IsContainer)
                Debug.Assert(ContainerViewList.GetContainerView(scrollTo.ContainerOrdinal) != null, "_scrollToMain must be realized ContainerView.");
        }

        private void CoerceScrollToMain()
        {
            var min = MinScrollToMain;
            if (_scrollToMain.Value < min.Value)
                _scrollToMain = min;
            var max = MaxScrollToMain;
            if (_scrollToMain.Value > max.Value)
                _scrollToMain = max;
        }

        private void InitContainerViews()
        {
            if (_variantLengthHandler != null)
                _variantLengthHandler.ClearMeasuredLengths();
            ContainerViewList.VirtualizeAll();

            var initialOrdinal = GetInitialOrdinal();
            if (initialOrdinal >= 0)
            {
                ContainerViewList.RealizeFirst(initialOrdinal);
                ContainerViewList[0].Measure(Size.Empty);
            }
        }

        private int GetInitialOrdinal()
        {
            if (MaxContainerCount == 0)
                return -1;

            var logicalMainTrack = GetLogicalMainTrack(_scrollToMain.GridExtent);
            if (logicalMainTrack.IsEof)
                return MaxContainerCount - 1;

            var gridTrack = logicalMainTrack.GridTrack;
            if (gridTrack.IsHead)
                return 0;
            else if (gridTrack.IsContainer)
                return logicalMainTrack.ContainerOrdinal;
            else
                return MaxContainerCount - 1;
        }

        private void FillForward()
        {
            if (CanRealizeForward)
            {
                var lengthToRealize = LengthToRealizeForwardFromHead;
                if (lengthToRealize > 0)
                    RealizeForward(lengthToRealize);
            }

            if (CanRealizeBackward && _scrollDeltaMain < 0)
            {
                var lengthToRealize = LengthToRealizeBackwardFromHead;
                if (lengthToRealize > 0)
                    RealizeBackward(lengthToRealize);
            }

            if (_scrollDeltaMain != 0)
                AdjustScrollToMain(true);

            FillGap();
        }

        private bool CanRealizeForward
        {
            get { return ContainerViewList.Last != null && ContainerViewList.Last.ContainerOrdinal < MaxContainerCount - 1; }
        }

        private bool CanRealizeBackward
        {
            get { return ContainerViewList.First != null && ContainerViewList.First.ContainerOrdinal > 0; }
        }

        private double LengthToRealizeForwardFromHead
        {
            get
            {
                Debug.Assert(_scrollToMainPlacement == GridPlacement.Head);
                Debug.Assert(ContainerViewList.Count == 1);

                var start = GetLogicalMainTrack(_scrollToMain.GridExtent);
                if (start.IsEof || start.IsTail)
                    return 0;

                var result = GridTracksMain.AvailableLength + _scrollDeltaMain;

                var fraction = _scrollToMain.Fraction;
                if (start.IsContainer)
                    return result - GetOffset(ContainerViewList.GetContainerView(start.ContainerOrdinal), start.GridTrack, fraction);

                Debug.Assert(start.IsHead && !start.IsFrozenHead);
                Debug.Assert(ContainerViewList.Count == 1);
                var startTrack = start.GridTrack;
                var endTrack = GridTracksMain[HeadTracksCountMain - 1];
                var excludedLength = endTrack.EndOffset - (startTrack.StartOffset + startTrack.MeasuredLength * fraction);
                return result - excludedLength - GetLengthMain(ContainerViewList[0]);
            }
        }

        private double LengthToRealizeBackwardFromHead
        {
            get
            {
                Debug.Assert(_scrollToMainPlacement == GridPlacement.Head);
                Debug.Assert(ContainerViewList.Count > 0);
                Debug.Assert(_scrollDeltaMain < 0);

                var end = GetLogicalMainTrack(_scrollToMain.GridExtent);
                if (end.IsEof || end.IsHead)
                    return 0;

                var result = -_scrollDeltaMain;
                var fraction = _scrollToMain.Fraction;
                if (end.IsContainer)
                    return result - GetOffset(ContainerViewList.GetContainerView(end.ContainerOrdinal), end.GridTrack, fraction);

                Debug.Assert(end.IsTail && !end.IsFrozenTail);
                Debug.Assert(ContainerViewList.Count == 1 && ContainerViewList[0].ContainerOrdinal == MaxContainerCount - 1);
                var endTrack = end.GridTrack;
                var startTrack = GridTracksMain[HeadTracksCountMain + ContainerTracksCountMain];
                var excludedLength = (endTrack.StartOffset - endTrack.MeasuredLength * fraction) - startTrack.StartOffset;
                return result - excludedLength - GetLengthMain(ContainerViewList[0]);
            }
        }

        private double GetOffset(ContainerView containerView, GridTrack gridTrack, double fraction)
        {
            Debug.Assert(gridTrack.IsContainer);

            var firstGridTrack = GridTracksMain[HeadTracksCountMain];
            var gridSpan = new LogicalMainTrack(gridTrack, containerView);
            if (firstGridTrack == gridTrack)
                return gridSpan.Length * fraction;

            var length = gridSpan.EndExtent - new LogicalMainTrack(firstGridTrack, containerView).StartExtent;
            return length - gridSpan.Length * (1 - fraction);
        }


        private double RealizeForward(double availableLength)
        {
            Debug.Assert(availableLength > 0 && CanRealizeForward);

            double result = 0;
            for (int ordinal = ContainerViewList.Last.ContainerOrdinal + 1; ordinal < MaxContainerCount; ordinal++)
            {
                ContainerViewList.RealizeNext();
                var containerView = ContainerViewList.Last;
                containerView.Measure(Size.Empty);
                var measuredLength = GetLengthMain(containerView);
                result += measuredLength;
                availableLength -= measuredLength;
                if (availableLength <= 0)
                    break;
            }
            return result;
        }

        private void FillBackward()
        {
            throw new NotImplementedException();
        }

        private double RealizeBackward(double availableLength)
        {
            Debug.Assert(CanRealizeBackward);
            Debug.Assert(availableLength > 0);

            double result = 0;
            for (int ordinal = ContainerViewList.First.ContainerOrdinal - 1; ordinal >= 0; ordinal--)
            {
                ContainerViewList.RealizePrev();
                var containerView = ContainerViewList.First;
                containerView.Measure(Size.Empty);
                var measuredLength = GetLengthMain(containerView);
                result += measuredLength;
                availableLength -= measuredLength;
                if (availableLength <= 0)
                    break;
            }
            return result;
        }

        private void FillGap()
        {
            Debug.Assert(_scrollToMainPlacement == GridPlacement.Head && _scrollDeltaMain == 0);

            if (ContainerViewList.Count == 0)
                return;

            var headGapToFill = GetHeadGapToFill(true);
            if (headGapToFill > 0 && CanRealizeBackward)
                RealizeBackward(headGapToFill);

            var tailGapToFill = GetTailGapToFill(true);
            if (tailGapToFill > 0 && CanRealizeForward)
                RealizeForward(tailGapToFill);

            tailGapToFill = GetTailGapToFill(true);
            var extraLength = Math.Max(0, -GetFillPositionMain(ContainerViewList[0], true));
            tailGapToFill -= extraLength;
            if (tailGapToFill > 0 && CanRealizeBackward)
                RealizeBackward(tailGapToFill);
        }

        private double GetHeadGapToFill(bool ignoreFrozen)
        {
            var first = ContainerViewList.First;
            if (first == null)
                return 0;
            var firstStartPosition = GetFillPositionMain(first, ignoreFrozen);
            var scrollableStart = ignoreFrozen ? 0 : FrozenHeadLengthMain;
            return firstStartPosition > 0 ? firstStartPosition - scrollableStart : 0;
        }

        private double GetTailGapToFill(bool ignoreFrozen)
        {
            var availableLength = GridTracksMain.AvailableLength;
            if (double.IsPositiveInfinity(availableLength))
                return availableLength;

            var last = ContainerViewList.Last;
            if (last == null)
                return 0;
            var lastEndPosition = GetFillPositionMain(last, ignoreFrozen) + GetLengthMain(last);
            var scrollableEnd = availableLength;
            if (!ignoreFrozen)
                scrollableEnd -= FrozenTailLengthMain;
            return scrollableEnd > lastEndPosition ? scrollableEnd - lastEndPosition : 0;
        }

        /// <remarks>ScrollOffsetMain is not available during FillForward/FillBackward, position must be calculated differently.</remarks>
        private double GetFillPositionMain(ContainerView containerView, bool ignoreFrozne)
        {
            var startTrack = GridTracksMain.GetGridSpan(Template.ContainerRange).StartTrack;
            var startExtent = new LogicalMainTrack(startTrack, containerView).StartExtent;
            var scrollOffsetMain = Translate(_scrollToMain) - Translate(MinScrollToMain);
            return startExtent - scrollOffsetMain;
        }

        protected sealed override void PrepareMeasure()
        {
            base.PrepareMeasure();

            // The following operations must be done after ScalarBindings.PostAutoSizeBindings measured.
            CoerceGapToFill();
            RefreshViewport();
            RefreshExtent();  // Exec order matters: RefreshExtent relies on RefreshViewport
            RefreshScrollOffset();  // Exec order matters: RefreshScrollOffset relies on RefreshViewport and RefreshExtent
        }

        private void CoerceGapToFill()
        {
            var headGapToFill = GetHeadGapToFill(false);
            if (headGapToFill > 0)
                AdjustScrollToMain(-headGapToFill, true);

            var tailGapToFill = GetTailGapToFill(false);
            if (tailGapToFill == 0)
                return;

            var extraLength = Math.Max(0, Translate(MinScrollToMain) - GetFillPositionMain(ContainerViewList[0], false));
            var lengthToAdjust = Math.Min(extraLength, tailGapToFill);
            if (lengthToAdjust > 0)
                AdjustScrollToMain(-lengthToAdjust, true);
        }

        private void RefreshExtent()
        {
            var valueMain = MaxExtentMain;
            if (valueMain < ViewportMain)
                valueMain = ViewportMain;
            var valueCross = MaxExtentCross;
            if (valueCross < ViewportCross)
                valueCross = ViewportCross;
            RefreshExtent(valueMain, valueCross);
        }

        private void RefreshViewport()
        {
            var valueMain = CoerceViewport(GridTracksMain, MaxExtentMain, FrozenHeadLengthMain, FrozenTailLengthMain);
            var valueCross = CoerceViewport(GridTracksCross, MaxExtentCross, FrozenHeadLengthCross, FrozenTailLengthCross);
            RefreshViewport(valueMain, valueCross);
        }

        private static double CoerceViewport(IGridTrackCollection gridTracks, double maxOffset, double frozenHeadLength, double frozenTailLength)
        {
            if (gridTracks.SizeToContent)
                return maxOffset;

            var result = gridTracks.AvailableLength;
            var frozenLength = frozenHeadLength + frozenTailLength;
            return Math.Max(frozenLength, result);
        }

        private void RefreshScrollOffset()
        {
            var minScrollToMain = Translate(MinScrollToMain);
            var scrollToMainExtent = ScrollToMainExtent;
            Debug.Assert(scrollToMainExtent >= minScrollToMain);
            var valueMain = scrollToMainExtent - minScrollToMain;
            var valueCross = CoerceScrollOffset(ScrollOffsetCross + _scrollDeltaCross, ExtentCross - ViewportCross);
            RefreshScollOffset(valueMain, valueCross);
        }
    }
}

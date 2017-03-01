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

            AdjustScrollToMain();
            return true;
        }

        private void AdjustScrollToMain()
        {
            AdjustScrollToMain(_scrollDeltaMain);
        }

        private void AdjustScrollToMain(double delta)
        {
            _scrollToMain = Translate(ScrollToMainExtent + delta);
            SetScrollDeltaMain(0, false);
            CoerceScrollToMain();
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
            var logicalMainTrack = GetLogicalMainTrack(_scrollToMain.GridExtent);
            if (!logicalMainTrack.IsEof)
            {
                var lengthToFill = GridTracksMain.AvailableLength + _scrollDeltaMain;
                if (logicalMainTrack.IsContainer)
                {
                    if (ContainerViewList.Count == 1)
                        lengthToFill += GetFillForwardOffset(ContainerViewList[0], logicalMainTrack.GridTrack, _scrollToMain.Fraction);
                }
                else
                    lengthToFill += logicalMainTrack.GridTrack.MeasuredLength * _scrollToMain.Fraction;
                if (lengthToFill > 0)
                    FillForward(logicalMainTrack, lengthToFill);
            }
            if (_scrollDeltaMain != 0)
                AdjustScrollToMain();
            //FillGap();
        }

        private void FillForward(LogicalMainTrack start, double availableLength)
        {
            Debug.Assert(!start.IsEof);
            Debug.Assert(availableLength > 0);

            if (start.IsTail)
                return;

            if (start.IsContainer)
            {
                RealizeForward(start.ContainerOrdinal, availableLength);
                return;
            }

            Debug.Assert(start.IsHead && !start.IsFrozenHead);
            var startTrack = start.GridTrack;
            var endTrack = GridTracksMain[HeadTracksCountMain - 1];
            var measuredLength = endTrack.EndOffset - startTrack.StartOffset;
            availableLength -= measuredLength;
            if (availableLength <= 0)
                return;

            if (MaxContainerCount > 0)
                RealizeForward(0, availableLength);
        }

        private void RealizeForward(int containerOrdinal, double availableLength)
        {
            Debug.Assert(containerOrdinal >= 0 && containerOrdinal <= MaxContainerCount - 1);
            Debug.Assert(availableLength > 0);

            Debug.Assert((ContainerViewList.Count == 1 && ContainerViewList[0].ContainerOrdinal == containerOrdinal) ||
                ContainerViewList.Last.ContainerOrdinal == containerOrdinal - 1);

            for (int ordinal = containerOrdinal; ordinal < MaxContainerCount; ordinal++)
            {
                ContainerView containerView;
                if (ContainerViewList.Count == 1 && ContainerViewList[0].ContainerOrdinal == ordinal)
                    containerView = ContainerViewList[0];
                else
                {
                    ContainerViewList.RealizeNext();
                    containerView = ContainerViewList.Last;
                    containerView.Measure(Size.Empty);
                }
                var measuredLength = GetLengthMain(containerView);
                availableLength -= measuredLength;
                if (availableLength <= 0)
                    return;
            }
        }

        private double GetFillForwardOffset(ContainerView containerView, GridTrack gridTrack, double fraction)
        {
            Debug.Assert(gridTrack.IsContainer);

            var firstGridTrack = GridTracksMain[HeadTracksCountMain];
            var gridSpan = new LogicalMainTrack(gridTrack, containerView);
            if (firstGridTrack == gridTrack)
                return gridSpan.Length * fraction;

            var length = gridSpan.EndExtent - new LogicalMainTrack(firstGridTrack, containerView).StartExtent;
            return length - gridSpan.Length * (1 - fraction);
        }

        private void FillBackward()
        {
            throw new NotImplementedException();
        }

        private void FillBackward(LogicalMainTrack end, double availableLength)
        {
            Debug.Assert(!end.IsEof);
            Debug.Assert(availableLength > 0);

            if (end.IsHead)
                return;

            if (end.IsContainer)
            {
                RealizeBackward(end.ContainerOrdinal, availableLength);
                return;
            }

            Debug.Assert(end.IsTail && !end.IsFrozenTail);
            var startTrack = GridTracksMain[HeadTracksCountMain + ContainerTracksCountMain];
            var endTrack = end.GridTrack;
            var measuredLength = endTrack.EndOffset - startTrack.StartOffset;
            availableLength -= measuredLength;
            if (availableLength <= 0)
                return;

            if (MaxContainerCount > 0)
                RealizeBackward(MaxContainerCount - 1, availableLength);
        }

        private void RealizeBackward(int containerOrdinal, double availableLength)
        {
            Debug.Assert(containerOrdinal >= 0 && containerOrdinal <= MaxContainerCount - 1);
            Debug.Assert(availableLength > 0);

            Debug.Assert((ContainerViewList.Count == 1 && ContainerViewList[0].ContainerOrdinal == containerOrdinal)
                || ContainerViewList.First.ContainerOrdinal == containerOrdinal + 1);

            for (int ordinal = containerOrdinal; ordinal >= 0; ordinal--)
            {
                ContainerView containerView;
                if (ContainerViewList.Count == 1 && ContainerViewList[0].ContainerOrdinal == containerOrdinal)
                    containerView = ContainerViewList[0];
                else
                {
                    ContainerViewList.RealizePrev();
                    containerView = ContainerViewList.First;
                    containerView.Measure(Size.Empty);
                }
                var measuredLength = GetLengthMain(containerView);
                availableLength -= measuredLength;
                if (availableLength <= 0)
                    return;
            }
        }

        private void FillGap()
        {
            Debug.Assert(_scrollToMainPlacement == GridPlacement.Head && _scrollDeltaMain == 0);

            if (ContainerViewList.Count == 0)
                return;

            var headGapToFill = HeadGapToFill;
            if (headGapToFill > 0)
            {
                var first = ContainerViewList.First;
                if (first != null && first.ContainerOrdinal > 0)
                    RealizeBackward(first.ContainerOrdinal - 1, headGapToFill);
            }

            headGapToFill = HeadGapToFill;
            if (headGapToFill > 0)
                AdjustScrollToMain(-headGapToFill);

            var tailGapToFill = TailGapToFill;
            if (tailGapToFill > 0)
            {
                var last = ContainerViewList.Last;
                if (last != null && last.ContainerOrdinal < MaxContainerCount - 1)
                    RealizeForward(last.ContainerOrdinal + 1, tailGapToFill);
            }
        }

        private double HeadGapToFill
        {
            get
            {
                var first = ContainerViewList.First;
                if (first == null)
                    return 0;
                var firstGridExtent = HeadTracksCountMain + first.ContainerOrdinal * ContainerTracksCountMain;
                var firstStartPosition = GetPositionMain(firstGridExtent, GridPlacement.Head);
                var scrollableStart = FrozenHeadLengthMain;
                return firstStartPosition > scrollableStart ? firstStartPosition - scrollableStart : 0;
            }
        }

        private double TailGapToFill
        {
            get
            {
                var availableLength = GridTracksMain.AvailableLength;
                if (double.IsPositiveInfinity(availableLength))
                    return availableLength;

                var last = ContainerViewList.Last;
                if (last == null)
                    return 0;
                var lastGridExtent = HeadTracksCountMain + last.ContainerOrdinal * ContainerTracksCountMain;
                var lastEndPosition = GetPositionMain(lastGridExtent, GridPlacement.Tail);
                var scrollableEnd = availableLength - (FrozenHeadLengthMain + FrozenTailLengthMain);
                return scrollableEnd > lastEndPosition ? scrollableEnd - lastEndPosition : 0;
            }
        }

        protected sealed override void PrepareMeasure()
        {
            base.PrepareMeasure();
            // must be done after ScalarBindings.PostAutoSizeBindings measured.
            RefreshViewport();
            RefreshExtent();  // Exec order matters: RefreshExtent relies on RefreshViewport
            RefreshScrollOffset();  // Exec order matters: RefreshScrollOffset relies on RefreshViewport and RefreshExtent
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

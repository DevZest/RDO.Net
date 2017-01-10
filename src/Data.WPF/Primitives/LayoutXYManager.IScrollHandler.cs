using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutXYManager
    {
        public ScrollViewer ScrollOwner { get; set; }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        private void InvalidateMeasure()
        {
            if (ElementCollection.Parent != null)
                ElementCollection.Parent.InvalidateMeasure();
        }

        private void InvalidateArrange()
        {
            if (ElementCollection.Parent != null)
                ElementCollection.Parent.InvalidateArrange();
        }

        public abstract double ViewportX { get; }

        public abstract double ViewportY { get; }

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

        public abstract double ExtentY { get; }

        public abstract double ExtentX { get; }

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

        public abstract double ScrollOffsetX { get; set; }

        public abstract double ScrollOffsetY { get; set; }

        private double _oldScrollOffsetMain;
        private double _scrollOffsetMain;
        protected double ScrollOffsetMain
        {
            get { return _scrollOffsetMain; }
            set { SetScrollOffsetMain(value, true); }
        }

        private void SetScrollOffsetMain(double value, bool invalidateMeasure)
        {
            if (_scrollOffsetMain.IsClose(value))
                return;
            _scrollOffsetMain = CoerceScrollOffset(value, ExtentMain);
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

        private double _scrollOffsetCross;
        protected double ScrollOffsetCross
        {
            get { return _scrollOffsetCross; }
            set { SetScrollOffsetCross(value, true); }
        }

        private void SetScrollOffsetCross(double value, bool invalidateMeasure)
        {
            if (_scrollOffsetCross.IsClose(value))
                return;
            _scrollOffsetCross = CoerceScrollOffset(value, ExtentCross - ViewportCross);
            if (invalidateMeasure)
                InvalidateMeasure();
        }

        private void RefreshScollOffset(double valueMain, double valueCross)
        {
            bool invalidateScrollInfo = !ScrollOffsetMain.IsClose(valueMain) || !ScrollOffsetCross.IsClose(valueCross);
            _oldScrollOffsetMain = valueMain;
            SetScrollOffsetMain(valueMain, false);
            SetScrollOffsetCross(valueCross, false);
            if (invalidateScrollInfo)
                InvalidateScrollInfo();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            foreach (var scalarBinding in ScalarBindings)
            {
                for (int i = 0; i < scalarBinding.BlockDimensions; i++)
                {
                    var element = scalarBinding[i];
                    if (element == visual)
                    {
                        EnsureVisible(scalarBinding, i);
                        return rectangle;
                    }
                }
            }

            if (CurrentContainerView == visual)
            {
                EnsureVisible(CurrentContainerView);
                return rectangle;
            }

            foreach (var containerView in ContainerViewList)
            {
                if (containerView == visual)
                {
                    EnsureVisible(containerView);
                    return rectangle;
                }
            }

            return rectangle;
        }

        private void EnsureVisible(ScalarBinding scalarBinding, int blockDimension)
        {
            SetScrollOffsetMain(ScrollOffsetMain + GetEnsureVisibleOffsetMain(scalarBinding), true);
            SetScrollOffsetCross(ScrollOffsetCross + GetEnsureVisibleOffsetCross(scalarBinding, blockDimension), true);
        }

        private double GetEnsureVisibleOffsetMain(ScalarBinding scalarBinding)
        {
            var gridRange = scalarBinding.GridRange;
            return GetEnsureVisibleOffsetMain(GetStartGridOffset(gridRange), GetEndGridOffset(gridRange));
        }

        private double GetEnsureVisibleOffsetCross(ScalarBinding scalarBinding, int blockDimension)
        {
            return 0;
        }

        private void EnsureVisible(ContainerView containerView)
        {
            SetScrollOffsetMain(ScrollOffsetMain + GetEnsureVisibleOffsetMain(containerView), true);
            SetScrollOffsetCross(ScrollOffsetCross + GetEnsureVisibleOffsetCross(containerView), true);
        }

        private double GetEnsureVisibleOffsetMain(ContainerView containerView)
        {
            var gridSpan = GridTracksMain.GetGridSpan(Template.BlockRange);
            return GetEnsureVisibleOffsetMain(new GridOffset(gridSpan.StartTrack, containerView), new GridOffset(gridSpan.EndTrack, containerView));
        }

        private double GetEnsureVisibleOffsetCross(ContainerView containerView)
        {
            return 0;
        }

        private double GetEnsureVisibleOffsetMain(GridOffset startGridOffset, GridOffset endGridOffset)
        {
            if (startGridOffset.GridTrack.IsFrozenHead || endGridOffset.GridTrack.IsFrozenTail)
                return 0;

            var start = startGridOffset.Span.Start;
            var scrollStartMain = ScrollStartMain;
            if (start < scrollStartMain)
                return start - scrollStartMain;

            var end = endGridOffset.Span.End - ScrollOffsetMain;
            var scrollEnd = ViewportMain - FrozenTailLengthMain;
            if (end > scrollEnd)
                return end - scrollEnd;

            return 0;
        }
    }
}

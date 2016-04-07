using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public sealed class DataElementPanel : FrameworkElement, IScrollInfo
    {
        #region IScrollInfo

        private double ScrollLineHeight
        {
            get { return DataView.ScrollLineHeight; }
        }

        private double ScrollLineWidth
        {
            get { return DataView.ScrollLineWidth; }
        }

        bool _canVerticallyScroll;
        bool IScrollInfo.CanVerticallyScroll
        {
            get { return _canVerticallyScroll; }
            set { _canVerticallyScroll = value; }
        }

        bool _canHorizontallyScroll;
        bool IScrollInfo.CanHorizontallyScroll
        {
            get { return _canHorizontallyScroll; }
            set { _canHorizontallyScroll = value; }
        }

        double IScrollInfo.ExtentWidth
        {
            get { return DataPresenter == null ? 0.0d : DataPresenter.ExtentWidth; }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return DataPresenter == null ? 0.0d : DataPresenter.ExtentHeight; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return DataPresenter == null ? 0.0d : DataPresenter.ViewportWidth; }
        }

        double IScrollInfo.ViewportHeight
        {
            get { return DataPresenter == null ? 0.0d : DataPresenter.ViewportHeight; }
        }

        double IScrollInfo.HorizontalOffset
        {
            get { return DataPresenter == null ? 0.0d : DataPresenter.HorizontalOffset; }
        }

        double IScrollInfo.VerticalOffset
        {
            get { return DataPresenter == null ? 0.0d : DataPresenter.VerticalOffset; }
        }

        ScrollViewer IScrollInfo.ScrollOwner
        {
            get { return DataPresenter == null ? null : DataPresenter.ScrollOwner; }
            set
            {
                if (DataPresenter != null)
                    DataPresenter.ScrollOwner = value;
            }
        }

        void IScrollInfo.LineUp()
        {
            DataPresenter.VerticalOffset -= ScrollLineHeight;
        }

        void IScrollInfo.LineDown()
        {
            DataPresenter.VerticalOffset += ScrollLineHeight;
        }

        void IScrollInfo.LineLeft()
        {
            DataPresenter.HorizontalOffset -= ScrollLineWidth;
        }

        void IScrollInfo.LineRight()
        {
            DataPresenter.HorizontalOffset += ScrollLineWidth;
        }

        void IScrollInfo.PageUp()
        {
            DataPresenter.VerticalOffset -= DataPresenter.ViewportHeight;
        }

        void IScrollInfo.PageDown()
        {
            DataPresenter.VerticalOffset += DataPresenter.ViewportHeight;
        }

        void IScrollInfo.PageLeft()
        {
            DataPresenter.HorizontalOffset -= DataPresenter.ViewportWidth;
        }

        void IScrollInfo.PageRight()
        {
            DataPresenter.HorizontalOffset += DataPresenter.ViewportWidth;
        }

        void IScrollInfo.MouseWheelUp()
        {
            DataPresenter.VerticalOffset -= SystemParameters.WheelScrollLines * ScrollLineHeight;
        }

        void IScrollInfo.MouseWheelDown()
        {
            DataPresenter.VerticalOffset += SystemParameters.WheelScrollLines * ScrollLineHeight;
        }

        void IScrollInfo.MouseWheelLeft()
        {
            DataPresenter.HorizontalOffset -= SystemParameters.WheelScrollLines * ScrollLineWidth;
        }

        void IScrollInfo.MouseWheelRight()
        {
            DataPresenter.HorizontalOffset += SystemParameters.WheelScrollLines * ScrollLineWidth;
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
            DataPresenter.HorizontalOffset = offset;
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
            DataPresenter.VerticalOffset = offset;
        }

        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
        {
            return DataPresenter.MakeVisible(visual, rectangle);
        }

        #endregion

        static DataElementPanel()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(DataElementPanel), new FrameworkPropertyMetadata(BooleanBoxes.True));
        }

        public DataElementPanel()
        {
        }

        private DataView DataView
        {
            get { return TemplatedParent as DataView; }
        }

        private DataPresenter DataPresenter
        {
            get
            {
                var dataView = DataView;
                return dataView == null ? null : dataView.DataPresenter;
            }
        }

        internal IReadOnlyList<UIElement> Elements
        {
            get
            {
                var dataPresenter = DataPresenter;
                if (dataPresenter == null)
                    return EmptyArray<UIElement>.Singleton;

                if (dataPresenter.ElementCollection == null || dataPresenter.ElementCollection.Parent != this)
                    dataPresenter.SetElementsPanel(this);

                Debug.Assert(dataPresenter.ElementCollection.Parent == this);
                return dataPresenter.ElementCollection;
            }
        }

        protected override int VisualChildrenCount
        {
            get { return Elements.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return Elements[index];
        }

        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    var dataPresenter = DataPresenter;
        //    return dataPresenter == null ? base.MeasureOverride(availableSize) : dataPresenter.Measure(availableSize);
        //}

        //protected override Size ArrangeOverride(Size finalSize)
        //{
        //    var dataPresenter = DataPresenter;
        //    return dataPresenter == null ? base.ArrangeOverride(finalSize) : dataPresenter.Arrange(finalSize);
        //}
    }
}

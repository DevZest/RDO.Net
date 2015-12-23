using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    public sealed class DataSetPanel : FrameworkElement, IScrollInfo
    {
        #region IScrollInfo

        private double ScrollLineHeight
        {
            get { return View.ScrollLineHeight; }
        }

        private double ScrollLineWidth
        {
            get { return View.ScrollLineWidth; }
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
            get { return LayoutManager.ExtentSize.Width; }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return LayoutManager.ExtentSize.Height; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return LayoutManager.ViewportSize.Width; }
        }

        double IScrollInfo.ViewportHeight
        {
            get { return LayoutManager.ViewportSize.Height; }
        }

        double IScrollInfo.HorizontalOffset
        {
            get { return LayoutManager.HorizontalOffset; }
        }

        double IScrollInfo.VerticalOffset
        {
            get { return LayoutManager.VerticalOffset; }
        }

        ScrollViewer IScrollInfo.ScrollOwner
        {
            get { return LayoutManager.ScrollOwner; }
            set { LayoutManager.ScrollOwner = value; }
        }

        void IScrollInfo.LineUp()
        {
            LayoutManager.VerticalOffset -= ScrollLineHeight;
        }

        void IScrollInfo.LineDown()
        {
            LayoutManager.VerticalOffset += ScrollLineHeight;
        }

        void IScrollInfo.LineLeft()
        {
            LayoutManager.HorizontalOffset -= ScrollLineWidth;
        }

        void IScrollInfo.LineRight()
        {
            LayoutManager.HorizontalOffset += ScrollLineWidth;
        }

        void IScrollInfo.PageUp()
        {
            LayoutManager.VerticalOffset -= LayoutManager.ViewportSize.Height;
        }

        void IScrollInfo.PageDown()
        {
            LayoutManager.VerticalOffset += LayoutManager.ViewportSize.Height;
        }

        void IScrollInfo.PageLeft()
        {
            LayoutManager.HorizontalOffset -= LayoutManager.ViewportSize.Width;
        }

        void IScrollInfo.PageRight()
        {
            LayoutManager.HorizontalOffset += LayoutManager.ViewportSize.Width;
        }

        void IScrollInfo.MouseWheelUp()
        {
            LayoutManager.VerticalOffset -= SystemParameters.WheelScrollLines * ScrollLineHeight;
        }

        void IScrollInfo.MouseWheelDown()
        {
            LayoutManager.VerticalOffset += SystemParameters.WheelScrollLines * ScrollLineHeight;
        }

        void IScrollInfo.MouseWheelLeft()
        {
            LayoutManager.HorizontalOffset -= SystemParameters.WheelScrollLines * ScrollLineWidth;
        }

        void IScrollInfo.MouseWheelRight()
        {
            LayoutManager.HorizontalOffset += SystemParameters.WheelScrollLines * ScrollLineWidth;
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
            LayoutManager.HorizontalOffset = offset;
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
            LayoutManager.VerticalOffset = offset;
        }

        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
        {
            return LayoutManager.MakeVisible(visual, rectangle);
        }

        #endregion

        public DataSetPanel()
        {
        }

        private DataSetPanel(DataSetPanel parent)
        {
            Debug.Assert(parent != null && _parent == null);
            _parent = parent;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            var dataSetView = TemplatedParent as DataSetView;
            if (dataSetView == null)
                return;

            _presenter = dataSetView.Presenter;
            var layoutManager = LayoutManager;
            if (IsPinned)
            {
                Child = new DataSetPanel(this);
                AddLogicalChild(Child);
                AddVisualChild(Child);
            }
            layoutManager.Panel = this;
            InitElements();
            if (Child != null)
                Child.InitElements();
        }

        private DataSetPanel _parent;

        internal DataSetPanel Child { get; private set; }

        private DataSetPresenter _presenter;
        private DataSetPresenter Presenter
        {
            get { return _parent != null ? _parent.Presenter : _presenter; }
        }

        private DataSetView View
        {
            get
            {
                if (_parent != null)
                    return _parent.View;

                var presenter = Presenter;
                return presenter == null ? null : presenter.View;
            }
        }

        private LayoutManager LayoutManager
        {
            get
            {
                var presenter = Presenter;
                return presenter == null ? null : presenter.LayoutManager;
            }
        }

        private bool IsPinned
        {
            get
            {
                var layoutManager = LayoutManager;
                return layoutManager == null || _parent != null ? false : layoutManager.IsPinned;
            }
        }

        private IReadOnlyList<UIElement> _elements;
        private IReadOnlyList<UIElement> Elements
        {
            get { return _elements; }
        }

        private void InitElements()
        {
            Debug.Assert(LayoutManager != null);
            _elements = IsPinned ? LayoutManager.PinnedElements : LayoutManager.ScrollableElements;
        }

        int ElementsCount
        {
            get { return Elements == null ? 0 : Elements.Count; }
        }

        protected override int VisualChildrenCount
        {
            get { return Child == null ? ElementsCount : ElementsCount + 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return index < ElementsCount ? Elements[index] : Child;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            LayoutManager.Measure(availableSize);

            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            finalSize = base.ArrangeOverride(finalSize);

            if (LayoutManager != null)
                LayoutManager.ViewportSize = finalSize;
            return finalSize;
        }
    }
}

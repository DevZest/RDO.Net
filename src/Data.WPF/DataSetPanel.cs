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
            get { return LayoutManager == null ? 0.0d : LayoutManager.ExtentSize.Width; }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return LayoutManager == null ? 0.0d : LayoutManager.ExtentSize.Height; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return LayoutManager == null ? 0.0d : LayoutManager.ViewportSize.Width; }
        }

        double IScrollInfo.ViewportHeight
        {
            get { return LayoutManager == null ? 0.0d : LayoutManager.ViewportSize.Height; }
        }

        double IScrollInfo.HorizontalOffset
        {
            get { return LayoutManager == null ? 0.0d : LayoutManager.HorizontalOffset; }
        }

        double IScrollInfo.VerticalOffset
        {
            get { return LayoutManager == null ? 0.0d : LayoutManager.VerticalOffset; }
        }

        ScrollViewer _scrollOwner;
        ScrollViewer IScrollInfo.ScrollOwner
        {
            get { return _scrollOwner; }
            set
            {
                _scrollOwner = value;
                if (LayoutManager != null)
                    LayoutManager.ScrollOwner = value;
            }
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

        private static readonly DependencyProperty PresenterProperty = DependencyProperty.Register(nameof(Presenter), typeof(DataSetPresenter),
            typeof(DataSetPanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPresenterChanged));

        private static void OnPresenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataSetPanel)d).OnPresenterChanged((DataSetPresenter)e.OldValue);
        }

        public DataSetPanel()
        {
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            var binding = new Binding(DataSetView.PresenterProperty.Name);
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
            BindingOperations.SetBinding(this, PresenterProperty, binding);
        }

        private DataSetPanel(DataSetPanel parent)
        {
            Debug.Assert(parent != null && _parent == null);
            _parent = parent;
        }

        private DataSetPanel _parent;

        private DataSetPanel _child;
        internal DataSetPanel Child
        {
            get { return _child; }
            private set
            {
                if (_child == value)
                    return;

                if (_child != null)
                {
                    RemoveLogicalChild(_child);
                    RemoveVisualChild(_child);
                }

                _child = value;

                if (_child != null)
                {
                    AddLogicalChild(_child);
                    AddVisualChild(_child);
                }
            }
        }

        internal DataSetPresenter Presenter
        {
            get { return _parent != null ? _parent.Presenter : (DataSetPresenter)GetValue(PresenterProperty); }
        }

        private void OnPresenterChanged(DataSetPresenter oldValue)
        {
            if (oldValue != null)
            {
                var oldLayoutManager = oldValue.LayoutManager;
                oldLayoutManager.SetElementsParent(null, null);
                oldLayoutManager.ScrollOwner = null;
            }

            var layoutManager = LayoutManager;
            layoutManager.ScrollOwner = _scrollOwner;
            var isPinned = layoutManager == null ? false : layoutManager.IsPinned;

            if (layoutManager == null || !layoutManager.IsPinned)
                Child = null;
            else if (Child == null)
                Child = new DataSetPanel(this);

            DataSetPanel pinnedPanel, scrollablePanel;
            if (isPinned)
            {
                pinnedPanel = this;
                scrollablePanel = Child;
            }
            else
            {
                pinnedPanel = null;
                scrollablePanel = this;
            }

            if (layoutManager != null)
                layoutManager.SetElementsParent(pinnedPanel, scrollablePanel);

            if (pinnedPanel != null)
            {
                Debug.Assert(layoutManager != null);
                pinnedPanel._elements = layoutManager.PinnedElements;
            }
            Debug.Assert(scrollablePanel != null);
            scrollablePanel._elements = layoutManager == null ? null : layoutManager.ScrollableElements;
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

        internal LayoutManager LayoutManager
        {
            get
            {
                var presenter = Presenter;
                return presenter == null ? null : presenter.LayoutManager;
            }
        }

        private IReadOnlyList<UIElement> _elements;
        internal IReadOnlyList<UIElement> Elements
        {
            get { return _elements; }
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
            var layoutManager = LayoutManager;
            if (layoutManager == null)
                return base.MeasureOverride(availableSize);

            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var layoutManager = LayoutManager;
            if (layoutManager == null)
                return base.ArrangeOverride(finalSize);

            return base.ArrangeOverride(finalSize);
        }
    }
}

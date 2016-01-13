using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    public sealed class DataPanel : FrameworkElement, IScrollInfo
    {
        #region IScrollInfo

        private double ScrollLineHeight
        {
            get { return DataForm.ScrollLineHeight; }
        }

        private double ScrollLineWidth
        {
            get { return DataForm.ScrollLineWidth; }
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
            get { return LayoutManager == null ? 0.0d : LayoutManager.ExtentWidth; }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return LayoutManager == null ? 0.0d : LayoutManager.ExtentHeight; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return LayoutManager == null ? 0.0d : LayoutManager.ViewportWidth; }
        }

        double IScrollInfo.ViewportHeight
        {
            get { return LayoutManager == null ? 0.0d : LayoutManager.ViewportHeight; }
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
            LayoutManager.VerticalOffset -= LayoutManager.ViewportHeight;
        }

        void IScrollInfo.PageDown()
        {
            LayoutManager.VerticalOffset += LayoutManager.ViewportHeight;
        }

        void IScrollInfo.PageLeft()
        {
            LayoutManager.HorizontalOffset -= LayoutManager.ViewportWidth;
        }

        void IScrollInfo.PageRight()
        {
            LayoutManager.HorizontalOffset += LayoutManager.ViewportWidth;
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

        private static readonly DependencyProperty ViewProperty = DependencyProperty.Register(nameof(View), typeof(DataView),
            typeof(DataPanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnViewChanged));

        private static void OnViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataPanel)d).OnViewChanged((DataView)e.OldValue);
        }

        static DataPanel()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(DataPanel), new FrameworkPropertyMetadata(BooleanBoxes.True));
        }

        public DataPanel()
        {
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            var binding = new System.Windows.Data.Binding(DataForm.ViewProperty.Name);
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
            BindingOperations.SetBinding(this, ViewProperty, binding);
        }

        private DataPanel(DataPanel parent)
        {
            Debug.Assert(parent != null && _parent == null);
            _parent = parent;
        }

        private DataPanel _parent;

        private DataPanel _child;
        internal DataPanel Child
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

        internal DataView View
        {
            get { return _parent != null ? _parent.View : (DataView)GetValue(ViewProperty); }
        }

        private void OnViewChanged(DataView oldValue)
        {
            if (oldValue != null)
            {
                var oldLayoutManager = oldValue.LayoutManager;
                oldLayoutManager.Invalidated -= OnLayoutInvalidated;
                oldLayoutManager.SetElementsParent(null, null);
                oldLayoutManager.ScrollOwner = null;
            }

            var layoutManager = LayoutManager;
            var isPinned = layoutManager == null ? false : layoutManager.IsPinned;

            if (layoutManager == null || !layoutManager.IsPinned)
                Child = null;
            else if (Child == null)
                Child = new DataPanel(this);

            DataPanel pinnedPanel, scrollablePanel;
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
            {
                layoutManager.ScrollOwner = _scrollOwner;
                layoutManager.Invalidated += OnLayoutInvalidated;

                layoutManager.SetElementsParent(pinnedPanel, scrollablePanel);
                if (pinnedPanel != null)
                    pinnedPanel._elements = layoutManager.PinnedElements;
            }

            scrollablePanel._elements = layoutManager == null ? null : layoutManager.ScrollableElements;
        }

        private void OnLayoutInvalidated(object sender, EventArgs e)
        {
            InvalidateMeasure();
        }

        private DataForm DataForm
        {
            get { return TemplatedParent as DataForm; }
        }

        internal LayoutManager LayoutManager
        {
            get
            {
                var view = View;
                return view == null ? null : view.LayoutManager;
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
            return layoutManager == null ? base.MeasureOverride(availableSize) : layoutManager.Measure(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var layoutManager = LayoutManager;
            return layoutManager == null ? base.ArrangeOverride(finalSize) : layoutManager.Arrange(finalSize);
        }
    }
}

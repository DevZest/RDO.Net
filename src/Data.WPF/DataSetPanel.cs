using System;
using System.Collections;
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

        private static readonly DependencyProperty PresenterProperty = DependencyProperty.Register(nameof(Presenter), typeof(DataSetPresenter),
            typeof(DataSetPanel), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPresenterChanged));

        private static void OnPresenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataSetPanel)d).OnPresenterChanged();
        }

        public DataSetPanel()
        {
            var binding = new Binding(DataSetView.PresenterProperty.Name);
            binding.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
            BindingOperations.SetBinding(this, PresenterProperty, binding);
        }

        private DataSetPanel(DataSetPanel parent)
        {
            Debug.Assert(parent != null && _parent == null);
            _parent = parent;
            RefreshElements();
        }

        private DataSetPanel _parent;

        private DataSetPanel _child;
        private DataSetPanel Child
        {
            get { return _child; }
            set
            {
                if (_child == value)
                    return;

                if (_child != null)
                {
                    _child.Elements = null;
                    RemoveLogicalChild(_child);
                    RemoveVisualChild(_child);
                }

                _child = value;

                if (_child != null)
                {
                    Debug.Assert(_child._parent != null && _child.Elements != null);
                    AddLogicalChild(_child);
                    AddVisualChild(_child);
                }
            }
        }

        private void RefreshChild()
        {
            if (LayoutManager == null || !LayoutManager.IsPinned)
                Child = null;
            else if (Child != null)
                Child.RefreshElements();
            else
                Child = new DataSetPanel(this);
        }

        private DataSetPresenter Presenter
        {
            get { return _parent != null ? _parent.Presenter : (DataSetPresenter)GetValue(PresenterProperty); }
        }

        private void OnPresenterChanged()
        {
            RefreshElements();
            RefreshChild();
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

        private ObservableCollection<UIElement> _elements;
        private ObservableCollection<UIElement> Elements
        {
            get { return _elements; }
            set
            {
                if (_elements == value)
                    return;

                if (_elements != null)
                {
                    RemoveElements(_elements);
                    _elements.CollectionChanged -= OnElementsChanged;
                }
                _elements = value;
                if (_elements != null)
                {
                    AddElements(_elements);
                    _elements.CollectionChanged += OnElementsChanged;
                }
            }
        }
        private void RefreshElements()
        {
            var layoutManager = LayoutManager;
            if (layoutManager == null)
                Elements = null;
            else
                Elements = IsPinned ? layoutManager.PinnedElements : layoutManager.ScrollableElements;
        }

        int ElementsCount
        {
            get { return Elements == null ? 0 : Elements.Count; }
        }

        private void OnElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Reset)
                RemoveElements(e.OldItems);

            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Reset)
                AddElements(e.NewItems);
        }

        private void RemoveElements(ICollection items)
        {
            foreach (var item in items)
            {
                RemoveLogicalChild(item);
                RemoveVisualChild((UIElement)item);
            }
        }

        private void AddElements(ICollection items)
        {
            foreach (var item in items)
            {
                AddLogicalChild(item);
                AddVisualChild((UIElement)item);
            }
        }

        protected override int VisualChildrenCount
        {
            get { return _child == null ? ElementsCount : ElementsCount + 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return index < ElementsCount ? Elements[index] : _child;
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

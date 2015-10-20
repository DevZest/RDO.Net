using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevZest.Data.Wpf
{
    public class DataSetPanel : FrameworkElement, IScrollInfo
    {
        private static ConditionalWeakTable<DataSetView, DataSetPanel> s_panelsByView = new ConditionalWeakTable<DataSetView, DataSetPanel>();

        private static DataSetPanel GetDataSetPanel(DataSetView dataSetView)
        {
            Debug.Assert(dataSetView != null);
            DataSetPanel result;
            return s_panelsByView.TryGetValue(dataSetView, out result) ? result : null;
        }

        private static void SetDataSetPanel(DataSetView dataSetView, DataSetPanel value)
        {
            Debug.Assert(dataSetView != null);
            Debug.Assert(value != null);

            var oldValue = GetDataSetPanel(dataSetView);
            if (oldValue == value)
                return;

            if (oldValue != null)
            {
                s_panelsByView.Remove(dataSetView);
                oldValue.DataSetView = null;
            }

            s_panelsByView.Add(dataSetView, value);
            value.DataSetView = dataSetView;
        }

        private DataSetView _dataSetView;
        private DataSetView DataSetView
        {
            get { return _dataSetView; }
            set
            {
                if (_dataSetView == value)
                    return;

                if (_dataSetView != null)
                    _dataSetView.Elements.CollectionChanged -= OnViewElementsChanged;
                _dataSetView = value;
                if (_dataSetView != null)
                    _dataSetView.Elements.CollectionChanged += OnViewElementsChanged;
            }
        }

        private ViewElementCollection _viewElements;
        private ViewElementCollection ViewElements
        {
            get { return _viewElements; }
            set
            {
                if (_viewElements == value)
                    return;

                if (_viewElements != null)
                {
                    _viewElements.CollectionChanged -= OnViewElementsChanged;
                    foreach (var viewElement in _viewElements)
                    {
                        RemoveVisualChild(viewElement.UIElement);
                        RemoveLogicalChild(viewElement.UIElement);
                    }
                }

                _viewElements = value;

                if (_viewElements != null)
                {
                    foreach (var viewElement in _viewElements)
                    {
                        AddVisualChild(viewElement.UIElement);
                        AddLogicalChild(viewElement.UIElement);
                    }
                    _viewElements.CollectionChanged += OnViewElementsChanged;
                }
            }
        }

        private void OnViewElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.OldItems)
            {
                var viewElement = item as ViewElement;
                if (viewElement != null)
                {
                    RemoveLogicalChild(viewElement.UIElement);
                    RemoveVisualChild(viewElement.UIElement);
                }
            }

            foreach (var item in e.NewItems)
            {
                var viewElement = item as ViewElement;
                if (viewElement != null)
                {
                    AddVisualChild(viewElement.UIElement);
                    AddLogicalChild(viewElement.UIElement);
                }
            }
        }

        protected override int VisualChildrenCount
        {
            get { return ViewElements == null ? 0 : ViewElements.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return ViewElements[index].UIElement;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return DataSetView != null ? DataSetView.Measure(availableSize) : base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return DataSetView != null ? DataSetView.Arrange(finalSize) : base.ArrangeOverride(finalSize);
        }

        public override void OnApplyTemplate()
        {
            var dataSetControl = TemplatedParent as DataSetControl;
            var dataSetView = dataSetControl == null ? null : dataSetControl.View;
            if (dataSetView != null)
                SetDataSetPanel(dataSetView, this);
        }

        #region IScrollInfo

        ScrollViewer IScrollInfo.ScrollOwner
        {
            get { return DataSetView.ScrollOwner; }
            set { DataSetView.ScrollOwner = value; }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return DataSetView.ExtentHeight; }
        }

        double IScrollInfo.ExtentWidth
        {
            get { return DataSetView.ExtentWidth; }
        }

        double IScrollInfo.ViewportHeight
        {
            get { return DataSetView.ViewportHeight; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return DataSetView.ViewportWidth; }
        }

        double IScrollInfo.VerticalOffset
        {
            get { return DataSetView.VerticalOffset; }
        }

        double IScrollInfo.HorizontalOffset
        {
            get { return DataSetView.HorizontalOffset; }
        }

        bool IScrollInfo.CanVerticallyScroll
        {
            get { return DataSetView.CanVerticallyScroll; }
            set { DataSetView.CanVerticallyScroll = value; }
        }

        bool IScrollInfo.CanHorizontallyScroll
        {
            get { return DataSetView.CanHorizontallyScroll; }
            set { DataSetView.CanHorizontallyScroll = value; }
        }

        void IScrollInfo.LineUp()
        {
            DataSetView.LineUp();
        }

        void IScrollInfo.LineDown()
        {
            DataSetView.LineDown();
        }

        void IScrollInfo.LineLeft()
        {
            DataSetView.LineLeft();
        }

        void IScrollInfo.LineRight()
        {
            DataSetView.LineRight();
        }

        void IScrollInfo.PageUp()
        {
            DataSetView.PageUp();
        }

        void IScrollInfo.PageDown()
        {
            DataSetView.PageDown();
        }

        void IScrollInfo.PageLeft()
        {
            DataSetView.PageLeft();
        }

        void IScrollInfo.PageRight()
        {
            DataSetView.PageRight();
        }

        void IScrollInfo.MouseWheelUp()
        {
            DataSetView.MouseWheelUp();
        }

        void IScrollInfo.MouseWheelDown()
        {
            DataSetView.MouseWheelDown();
        }

        void IScrollInfo.MouseWheelLeft()
        {
            DataSetView.MouseWheelLeft();
        }

        void IScrollInfo.MouseWheelRight()
        {
            DataSetView.MouseWheelRight();
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
           DataSetView.SetHorizontalOffset(offset);
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
            DataSetView.SetVerticalOffset(offset);
        }

        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
        {
            return DataSetView.MakeVisible(visual, rectangle);
        }

        #endregion
    }
}

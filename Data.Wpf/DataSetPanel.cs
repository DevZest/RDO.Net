using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevZest.Data.Wpf
{
    public class DataSetPanel : VirtualizingPanel, IScrollInfo
    {
        private DataSetControl _dataSetControl;
        DataSetControl DataSetControl
        {
            get { return _dataSetControl; }
            set
            {
                if (_dataSetControl == value)
                    return;

                if (_dataSetControl != null)
                {
                    DependencyPropertyDescriptor.FromProperty(DataSetControl.OrientationProperty, typeof(DataSetControl))
                        .RemoveValueChanged(_dataSetControl, OnOrientationChanged);
                    DependencyPropertyDescriptor.FromProperty(DataSetControl.FrozenGridCountProperty, typeof(DataSetControl))
                        .RemoveValueChanged(_dataSetControl, OnFrozenGridCountChanged);
                }
                _dataSetControl = value;
                if (_dataSetControl != null)
                {
                    DependencyPropertyDescriptor.FromProperty(DataSetControl.OrientationProperty, typeof(DataSetControl))
                        .AddValueChanged(_dataSetControl, OnOrientationChanged);
                    DependencyPropertyDescriptor.FromProperty(DataSetControl.FrozenGridCountProperty, typeof(DataSetControl))
                        .AddValueChanged(_dataSetControl, OnFrozenGridCountChanged);
                }
            }
        }

        public override void OnApplyTemplate()
        {
            DataSetControl = TemplatedParent as DataSetControl;
        }

        private void OnOrientationChanged(object sender, EventArgs e)
        {
        }

        private void OnFrozenGridCountChanged(object sender, EventArgs e)
        {
        }

        List<UIElement> _visibleUIElements = new List<UIElement>();

        private void Reset()
        {
            if (Children.Count > 0)
                RemoveInternalChildRange(0, Children.Count); // Remove all visual elements

            foreach (var element in _visibleUIElements)
            {
                var viewManager = element.GetViewManager();
                Debug.Assert(viewManager != null);
                viewManager.ReturnUIElement(element);
            }

            _visibleUIElements.Clear();
            //_topmostElementsCount = 0;
        }

        private void Refresh()
        {

        }

        #region IScrollInfo

        ScrollManager _scrollManager;

        ScrollViewer IScrollInfo.ScrollOwner
        {
            get { return _scrollManager.ScrollOwner; }
            set { _scrollManager.ScrollOwner = value; }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return _scrollManager.ExtentHeight; }
        }

        double IScrollInfo.ExtentWidth
        {
            get { return _scrollManager.ExtentWidth; }
        }

        double IScrollInfo.ViewportHeight
        {
            get { return _scrollManager.ViewportHeight; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return _scrollManager.ViewportWidth; }
        }

        double IScrollInfo.VerticalOffset
        {
            get { return _scrollManager.VerticalOffset; }
        }

        double IScrollInfo.HorizontalOffset
        {
            get { return _scrollManager.HorizontalOffset; }
        }

        bool IScrollInfo.CanVerticallyScroll
        {
            get { return _scrollManager.CanVerticallyScroll; }
            set { _scrollManager.CanVerticallyScroll = value; }
        }

        bool IScrollInfo.CanHorizontallyScroll
        {
            get { return _scrollManager.CanHorizontallyScroll; }
            set { _scrollManager.CanHorizontallyScroll = value; }
        }

        void IScrollInfo.LineUp()
        {
            _scrollManager.LineUp();
        }

        void IScrollInfo.LineDown()
        {
            _scrollManager.LineDown();
        }

        void IScrollInfo.LineLeft()
        {
            _scrollManager.LineLeft();
        }

        void IScrollInfo.LineRight()
        {
            _scrollManager.LineRight();
        }

        void IScrollInfo.PageUp()
        {
            _scrollManager.PageUp();
        }

        void IScrollInfo.PageDown()
        {
            _scrollManager.PageDown();
        }

        void IScrollInfo.PageLeft()
        {
            _scrollManager.PageLeft();
        }

        void IScrollInfo.PageRight()
        {
            _scrollManager.PageRight();
        }

        void IScrollInfo.MouseWheelUp()
        {
            _scrollManager.MouseWheelUp();
        }

        void IScrollInfo.MouseWheelDown()
        {
            _scrollManager.MouseWheelDown();
        }

        void IScrollInfo.MouseWheelLeft()
        {
            _scrollManager.MouseWheelLeft();
        }

        void IScrollInfo.MouseWheelRight()
        {
            _scrollManager.MouseWheelRight();
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
           _scrollManager.SetHorizontalOffset(offset);
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
            _scrollManager.SetVerticalOffset(offset);
        }

        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
        {
            return _scrollManager.MakeVisible(visual, rectangle);
        }

        #endregion
    }
}

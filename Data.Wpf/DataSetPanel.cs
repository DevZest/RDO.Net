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
                var viewItem = element.GetViewItem();
                Debug.Assert(viewItem != null);
                viewItem.ReturnUIElement(element);
            }

            _visibleUIElements.Clear();
            //_topmostElementsCount = 0;
        }

        private void Refresh()
        {

        }

        #region IScrollInfo

        DataSetView DataSetView
        {
            get { return DataSetControl == null ? null : DataSetControl.View; }
        }

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

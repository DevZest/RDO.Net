﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class LayoutManager : ElementManager
    {
        internal LayoutManager(DataSet dataSet)
            : base(dataSet)
        {
        }

        private DataPanel _dataPanel;
        internal DataPanel DataPanel
        {
            get { return _dataPanel; }
            set
            {
                if (_dataPanel == value)
                    return;

                if (_dataPanel != null)
                    ClearElements();

                _dataPanel = value;

                if (_dataPanel != null)
                    InitializeElements(_dataPanel);
            }
        }

        internal double ViewportWidth { get; private set; }

        internal double ViewportHeight { get; private set; }

        internal double ExtentHeight { get; private set; }

        internal double ExtentWidth { get; private set; }

        private Size ExtentSize
        {
            set
            {
                if (ExtentHeight.IsClose(value.Height) && ExtentWidth.IsClose(value.Width))
                    return;
                ExtentHeight = value.Height;
                ExtentWidth = value.Width;
                InvalidateScrollInfo();
            }
        }

        private double _horizontalOffset;
        internal double HorizontalOffset
        {
            get { return _horizontalOffset; }
            set
            {
                if (_horizontalOffset.IsClose(value))
                    return;

                HorizontalOffsetDelta += (value - _horizontalOffset);
                _horizontalOffset = value;
                InvalidateScrollInfo();
            }
        }

        internal double HorizontalOffsetDelta { get; private set; }

        private double _verticalOffset;
        public double VerticalOffset
        {
            get { return _verticalOffset; }
            set
            {
                if (_verticalOffset.IsClose(value))
                    return;

                VerticalOffsetDelta += (value - _verticalOffset);
                _verticalOffset = value;
                InvalidateScrollInfo();
            }
        }

        protected double VerticalOffsetDelta { get; private set; }

        internal ScrollViewer ScrollOwner { get; set; }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }

        private Size ViewportSize
        {
            get { return new Size(ViewportWidth, ViewportHeight); }
            set
            {
                if (ViewportWidth.IsClose(value.Width) && ViewportHeight.IsClose(value.Height))
                    return;

                ViewportWidth = value.Width;
                ViewportHeight = value.Height;

                InvalidateScrollInfo();
            }
        }

        internal Size Measure(Size availableSize)
        {
            throw new NotImplementedException();
        }

        internal Size Arrange(Size finalSize)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    internal abstract partial class LayoutManager
    {
        internal static LayoutManager Create(DataView view)
        {
            return new LayoutZ(view);

            //var orientation = view.Template.ListOrientation;
            //if (orientation == ListOrientation.Z)
            //    return new LayoutZ(view);
            //else if (orientation == ListOrientation.Y)
            //    return new LayoutY(view);
            //else if (orientation == ListOrientation.XY)
            //    return new LayoutXY(view);
            //else if (orientation == ListOrientation.X)
            //    return new LayoutX(view);
            //else
            //{
            //    Debug.Assert(orientation == ListOrientation.YX);
            //    return new LayoutYX(view);
            //}
        }

        protected LayoutManager(DataView view)
        {
            Debug.Assert(view != null);
            _view = view;
            _realizedRows = new RealizedRowCollection(this);
        }

        private DataView _view;

        private IReadOnlyList<RowView> Rows
        {
            get { return _view; }
        }

        private RealizedRowCollection _realizedRows;

        public void InitRowFormConstructor(Func<RowForm> rowFormConstructor)
        {
            _realizedRows.InitRowFormConstructor(rowFormConstructor);
        }

        public GridTemplate Template
        {
            get { return _view.Template; }
        }

        private int VirtualizingThreshold
        {
            get { return _view.VirtualizingThreshold; }
        }

        private ListOrientation ListOrientation
        {
            get { return Template.ListOrientation; }
        }

        private IElementCollection _elements;
        public IReadOnlyList<UIElement> Elements
        {
            get
            {
                EnsureElementCollectionInitialized();
                return _elements;
            }
        }

        private IElementCollection Children
        {
            get
            {
                EnsureElementCollectionInitialized();
                return _elements;
            }
        }

        private FrameworkElement _elementsPanel;

        public void SetElementsPanel(FrameworkElement elementsPanel)
        {
            Debug.Assert(_elementsPanel != elementsPanel);

            _elementsPanel = elementsPanel;

            if (_elements != null)
            {
                _realizedRows.RemoveAll();
                var scalarUnits = Template.ScalarUnits;
                for (int i = 0; i < scalarUnits.Count; i++)
                {
                    var scalarUnit = scalarUnits[i];
                    var element = Elements[i];
                    scalarUnit.Cleanup(element);
                    element.SetDataView(null);
                }
                _elements.Clear();
                _elements = null;
            }
        }

        private void EnsureElementCollectionInitialized()
        {
            if (_elements != null)
                return;
            _elements = IElementCollectionFactory.Create(_elementsPanel);

            var scalarUnits = Template.ScalarUnits;

            for (int i = 0; i < scalarUnits.Count; i++)
            {
                var scalarUnit = scalarUnits[i];
                var element = scalarUnit.Generate();
                element.SetDataView(_view);
                scalarUnit.Initialize(element);
                _elements.Add(element);
            }
        }

        public double ViewportWidth { get; private set; }

        public double ViewportHeight { get; private set; }

        public double ExtentHeight { get; private set; }

        public double ExtentWidth { get; private set; }

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
        public double HorizontalOffset
        {
            get { return _horizontalOffset; }
            set
            {
                if (_horizontalOffset.IsClose(value))
                    return;

                HorizontalOffsetDelta += (value - _horizontalOffset);
                _horizontalOffset = value;
                InvalidateScrollInfo();
                Invalidate();
            }
        }

        protected double HorizontalOffsetDelta { get; private set; }

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
                Invalidate();
            }
        }

        protected double VerticalOffsetDelta { get; private set; }

        public ScrollViewer ScrollOwner { get; set; }

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

        public void OnRowAdded(int index)
        {
            Invalidate();
        }

        public void OnRowRemoved(int index, RowView row)
        {
            Invalidate();
        }

        public virtual void OnCurrentRowChanged()
        {
        }

        private bool _isInvalidated;
        public event EventHandler Invalidated;

        private void Invalidate()
        {
            if (_isInvalidated)
                return;

            _isInvalidated = true;
            var invalidated = Invalidated;
            if (invalidated != null)
                invalidated(this, EventArgs.Empty);
        }

        private const double DEFAULT_WIDTH = 200;
        private const double DEFAULT_HEIGHT = 200;

        public Size Measure(Size availableSize)
        {
            availableSize = VerifyInfinity(availableSize);

            MeasureOverride(availableSize);
            if (ScrollOwner != null)
            {
                ViewportSize = CalcViewportSize();
                ExtentSize = CalcExtentSize();
            }
            HorizontalOffsetDelta = 0;
            VerticalOffsetDelta = 0;
            _isInvalidated = false;
            return availableSize;
        }

        private Size VerifyInfinity(Size availableSize)
        {
            double width = availableSize.Width;
            if (double.IsPositiveInfinity(width) && !Template.AllowsInfiniteX)
                width = DEFAULT_WIDTH;
            double height = availableSize.Height;
            if (double.IsPositiveInfinity(height) && !Template.AllowsInfiniteY)
                height = DEFAULT_HEIGHT;
            return new Size(width, height);
        }

        protected abstract void MeasureOverride(Size availableSize);

        protected abstract int RepeatXCount { get; }

        protected abstract int RepeatYCount { get; }

        protected abstract double GetGridWidth(GridColumn gridColumn, int repeatXIndex);

        protected abstract double GetGridHeight(GridRow gridRow, int repeatYIndex);

        private Size CalcViewportSize()
        {
            return new Size();
        }

        private Size CalcExtentSize()
        {
            return new Size();
        }

        public Size Arrange(Size finalSize)
        {
            return ViewportSize;
        }
    }
}

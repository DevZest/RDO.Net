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
                var scalarItems = Template.ScalarItems;
                for (int i = 0; i < scalarItems.Count; i++)
                {
                    var scalarItem = scalarItems[i];
                    var element = Elements[i];
                    scalarItem.Cleanup(element);
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

            var scalarItems = Template.ScalarItems;

            for (int i = 0; i < scalarItems.Count; i++)
            {
                var scalarItem = scalarItems[i];
                var element = scalarItem.Generate();
                element.SetDataView(_view);
                scalarItem.Initialize(element);
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

        private GridColumn[] _autoWidthColumns;
        private GridRow[] _autoHeightRows;
        private GridColumn[] _starWidthColumns;
        private GridRow[] _starHeightRows;
        private IList<AutoSizeItem> _autoSizeItems;

        private void GenerateAutoSizeItems(bool sizeToContentX, bool sizeToContentY, bool reset)
        {
            if (_autoSizeItems != null && !reset)
                return;

            var template = Template;
            _autoSizeItems = EmptyArray<AutoSizeItem>.Singleton;

            var scalarItems = template.ScalarItems;
            var listItems = template.ListItems;
            for (int i = 0; i < template.ScalarItemsCountBeforeList; i++)
                GenerateAutoSizeItem(scalarItems[i], sizeToContentX, sizeToContentY);

            for (int i = 0; i < listItems.Count; i++)
                GenerateAutoSizeItem(listItems[i], sizeToContentX, sizeToContentY);

            for (int i = template.ScalarItemsCountBeforeList; i < scalarItems.Count; i++)
                GenerateAutoSizeItem(scalarItems[i], sizeToContentX, sizeToContentY);

            _autoSizeItems.Sort((x, y) => Compare(x, y));
        }

        private static int Compare(AutoSizeItem x, AutoSizeItem y)
        {
            var order1 = x.TemplateItem.AutoSizeMeasureOrder;
            var order2 = y.TemplateItem.AutoSizeMeasureOrder;
            if (order1 > order2)
                return 1;
            else if (order1 < order2)
                return -1;
            else
                return 0;
        }

        private void GenerateAutoSizeItem(TemplateItem templateItem, bool sizeToContentX, bool sizeToContentY)
        {
            var autoSizeItem = TryGenerateAutoSizeItem(templateItem, sizeToContentX, sizeToContentY);
            if (autoSizeItem == null)
                return;

            if (_autoSizeItems == EmptyArray<AutoSizeItem>.Singleton)
                _autoSizeItems = new List<AutoSizeItem>();
            _autoSizeItems.Add(autoSizeItem);
        }

        private static AutoSizeItem TryGenerateAutoSizeItem(TemplateItem templateItem, bool sizeToContentX, bool sizeToContentY)
        {
            if (templateItem.AutoSizeMeasureOrder < 0)
                return null;

            var entry = AutoSizeEntry.Resolve(templateItem.GridRange, sizeToContentX, sizeToContentY);
            return entry.IsEmpty ? null : new AutoSizeItem(templateItem, entry.Columns, entry.Rows);
        }

        private static void InitMeasuredLength(double availableLength, GridTrack[] autoGridTracks, GridTrack[] starGridTracks)
        {
            double autoLengthTotal = 0;
            foreach (var autoGridTrack in autoGridTracks)
            {
                autoGridTrack.MeasuredLength = autoGridTrack.MinLength;
                autoLengthTotal += autoGridTrack.MeasuredLength;
            }

            CalcStarMeasuredLength(availableLength - autoLengthTotal, starGridTracks);
        }

        private static void CalcStarMeasuredLength(double availableLength, GridTrack[] starGridTracks)
        {
            if (starGridTracks.Length == 0)
                return;

            double starLengthTotal = 0;
            foreach (var starGridTrack in starGridTracks)
                starLengthTotal += starGridTrack.Length.Value;

            availableLength = Math.Max(0, availableLength);
            foreach (var starGridTrack in starGridTracks)
                starGridTrack.MeasuredLength = Math.Max(starGridTrack.MinLength, 
                    Math.Min(starGridTrack.MaxLength, availableLength / starLengthTotal * starGridTrack.Length.Value));
        }

        public Size Measure(Size availableSize)
        {
            bool sizeToContentX = double.IsPositiveInfinity(availableSize.Width);
            bool sizeToContentY = double.IsPositiveInfinity(availableSize.Height);

            bool gridColumnsReset = Template.GridColumns.Classify(sizeToContentX, ref _autoWidthColumns, ref _starWidthColumns);
            InitMeasuredLength(availableSize.Width, _autoWidthColumns, _starWidthColumns);
            bool gridRowsReset = Template.GridRows.Classify(sizeToContentY, ref _autoHeightRows, ref _starHeightRows);
            InitMeasuredLength(availableSize.Height, _autoHeightRows, _starHeightRows);
            GenerateAutoSizeItems(sizeToContentX, sizeToContentY, gridColumnsReset || gridRowsReset);

            MeasureAutoSizeItems();

            if (ScrollOwner != null)
            {
                ViewportSize = CalcViewportSize();
                ExtentSize = CalcExtentSize();
            }
            HorizontalOffsetDelta = 0;
            VerticalOffsetDelta = 0;
            _isInvalidated = false;
            return CalcDesiredSize();
        }

        protected abstract void MeasureAutoSizeItems();

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

        private Size CalcDesiredSize()
        {
            return new Size();
        }

        public Size Arrange(Size finalSize)
        {
            return ViewportSize;
        }
    }
}

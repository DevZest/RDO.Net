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

        private GridColumn[] _autoSizeColumns;
        private GridRow[] _autoSizeRows;
        private GridColumn[] _starSizeColumns;
        private GridRow[] _starSizeRows;
        private IList<AutoSizeItem> _autoSizeItems;

        private bool ClassifyGridColumns(bool sizeToContentX)
        {
            var autoSizeColumnsCount = Template.AutoSizeColumnsCount;
            if (sizeToContentX)
                autoSizeColumnsCount += Template.StarSizeColumnsCount;

            var starSizeColumnsCount = sizeToContentX ? 0 : Template.StarSizeColumnsCount;

            if (_autoSizeColumns != null && _autoSizeColumns.Length == autoSizeColumnsCount)
            {
                Debug.Assert(_starSizeColumns != null && _starSizeColumns.Length == starSizeColumnsCount);
                return false;
            }

            _autoSizeColumns = autoSizeColumnsCount == 0 ? EmptyArray<GridColumn>.Singleton : new GridColumn[autoSizeColumnsCount];
            _starSizeColumns = starSizeColumnsCount == 0 ? EmptyArray<GridColumn>.Singleton : new GridColumn[starSizeColumnsCount];
            ClassifyGridTracks(Template.GridColumns, sizeToContentX, _autoSizeColumns, _starSizeColumns);
            return true;
        }

        private bool ClassifyGridRows(bool sizeToContentY)
        {
            var autoSizeRowsCount = Template.AutoSizeRowsCount;
            if (sizeToContentY)
                autoSizeRowsCount += Template.StarSizeRowsCount;

            var starSizeRowsCount = sizeToContentY ? 0 : Template.StarSizeRowsCount;

            if (_autoSizeRows != null && _autoSizeRows.Length == autoSizeRowsCount)
            {
                Debug.Assert(_starSizeRows != null && _starSizeRows.Length == starSizeRowsCount);
                return false;
            }

            _autoSizeRows = autoSizeRowsCount == 0 ? EmptyArray<GridRow>.Singleton : new GridRow[autoSizeRowsCount];
            _starSizeRows = starSizeRowsCount == 0 ? EmptyArray<GridRow>.Singleton : new GridRow[starSizeRowsCount];
            ClassifyGridTracks(Template.GridRows, sizeToContentY, _autoSizeRows, _starSizeRows);
            return true;
        }

        private static void ClassifyGridTracks<T>(IReadOnlyList<T> tracks, bool sizeToContent, T[] autoSizeTracks, T[] starSizeTracks)
            where T : GridTrack
        {
            var indexAutoSize = 0;
            var indexStarSize = 0;
            foreach (var track in tracks)
            {
                var length = track.Length;
                if (length.IsAuto)
                    autoSizeTracks[indexAutoSize++] = track;
                else if (length.IsStar)
                {
                    if (sizeToContent)
                        autoSizeTracks[indexAutoSize++] = track;
                    else
                        starSizeTracks[indexStarSize++] = track;
                }
            }
        }

        private void PrepareAutoSizeItems(bool sizeToContentX, bool sizeToContentY, bool reset)
        {
            if (_autoSizeItems != null && !reset)
                return;

            var template = Template;
            _autoSizeItems = EmptyArray<AutoSizeItem>.Singleton;

            var scalarItems = template.ScalarItems;
            var listItems = template.ListItems;
            for (int i = 0; i < template.ScalarItemsCountBeforeList; i++)
                AddAutoSizeItem(scalarItems[i], sizeToContentX, sizeToContentY);

            for (int i = 0; i < listItems.Count; i++)
                AddAutoSizeItem(listItems[i], sizeToContentX, sizeToContentY);

            for (int i = template.ScalarItemsCountBeforeList; i < scalarItems.Count; i++)
                AddAutoSizeItem(scalarItems[i], sizeToContentX, sizeToContentY);

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

        private void AddAutoSizeItem(TemplateItem templateItem, bool sizeToContentX, bool sizeToContentY)
        {
            var autoSizeItem = GetAutoSizeItem(templateItem, sizeToContentX, sizeToContentY);
            if (autoSizeItem == null)
                return;

            if (_autoSizeItems == EmptyArray<AutoSizeItem>.Singleton)
                _autoSizeItems = new List<AutoSizeItem>();
            _autoSizeItems.Add(autoSizeItem);
        }

        private static AutoSizeItem GetAutoSizeItem(TemplateItem templateItem, bool sizeToContentX, bool sizeToContentY)
        {
            if (templateItem.AutoSizeMeasureOrder < 0)
                return null;

            var entry = GetAutoSizeEntry(templateItem.GridRange, sizeToContentX, sizeToContentY);
            return entry.IsEmpty ? null : new AutoSizeItem(templateItem, entry.Columns, entry.Rows);
        }

        private static AutoSizeEntry GetAutoSizeEntry(GridRange gridRange, bool sizeToContentX, bool sizeToContentY)
        {
            //  There is an issue with items contains both auto and star tracks.
            //  Intuitively, we expect that those items receive enough space to layout and that this space is perfectly divided into the auto / star tracks.
            //  The problem is that it is not possible to determine the size of star tracks until all auto track size determined,
            //  and that it is not possible determine missing space to include into the auto-sized tracks for those items as long as we don't know the size 
            //  of star-sized tracks.
            //  We are in a dead-end. There is basically two solutions: 
            //     1. Include all the missing size for those items into the auto tracks
            //     2. Include none of the missing size into the auto tracks and hope that the star tracks will be big enough to contain those items.
            //  Here we chose option (2), that is we ignore those elements during calculation of auto-sized tracks.
            //  The reason between this choice is that (1) will tend to increase excessively the size of auto-sized tracks (for nothing).
            //  Moreover, we consider items included both auto and star-size tracks are rare, and most of the time we want
            //  to be spread along several tracks rather than auto-sized.

            var columnSet = GridColumnSet.Empty;
            if (ContainsStarWidth(gridRange, sizeToContentX))
            {
                for (int x = gridRange.Left.Ordinal; x <= gridRange.Right.Ordinal; x++)
                {
                    var column = gridRange.Owner.GridColumns[x];
                    var width = column.Width;
                    if (width.IsAuto || (width.IsStar && sizeToContentX))
                        columnSet = columnSet.Merge(column);
                }
            }

            var rowSet = GridRowSet.Empty;
            if (ContainsStarHeight(gridRange, sizeToContentY))
            {
                for (int y = gridRange.Top.Ordinal; y <= gridRange.Bottom.Ordinal; y++)
                {
                    var row = gridRange.Owner.GridRows[y];
                    var height = row.Height;
                    if (height.IsAuto || (height.IsStar && sizeToContentY))
                        rowSet = rowSet.Merge(row);
                }
            }

            return new AutoSizeEntry(columnSet, rowSet);
        }

        private static bool ContainsStarWidth(GridRange gridRange, bool sizeToContentX)
        {
            if (sizeToContentX)
                return false;

            for (int x = gridRange.Left.Ordinal; x <= gridRange.Right.Ordinal; x++)
            {
                if (gridRange.Owner.GridColumns[x].Width.IsStar)
                    return true;
            }
            return false;
        }

        private static bool ContainsStarHeight(GridRange gridRange, bool sizeToContentY)
        {
            if (sizeToContentY)
                return false;

            for (int y = gridRange.Top.Ordinal; y <= gridRange.Bottom.Ordinal; y++)
            {
                if (gridRange.Owner.GridRows[y].Height.IsStar)
                    return true;
            }
            return false;
        }

        public Size Measure(Size availableSize)
        {
            bool sizeToContentX = double.IsPositiveInfinity(availableSize.Width);
            bool sizeToContentY = double.IsPositiveInfinity(availableSize.Height);

            bool gridColumnsReset = ClassifyGridColumns(sizeToContentX);
            bool gridRowsReset = ClassifyGridRows(sizeToContentY);
            PrepareAutoSizeItems(sizeToContentX, sizeToContentY, gridColumnsReset || gridRowsReset);

            MeasureOverride(availableSize);
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

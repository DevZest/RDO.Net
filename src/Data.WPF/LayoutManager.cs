using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows
{
    internal abstract partial class LayoutManager
    {
        internal static LayoutManager Create(DataPresenter presenter)
        {
            return presenter.Template.RepeatOrientation == RepeatOrientation.Z ? new LayoutZ(presenter) : RepeatLayout.Create(presenter);
        }

        protected LayoutManager(DataPresenter presenter)
        {
            Debug.Assert(presenter != null);
            _presenter = presenter;
            _realizedRows = new RealizedRowCollection(this);
        }

        private DataPresenter _presenter;

        private IReadOnlyList<RowPresenter> Rows
        {
            get { return _presenter; }
        }

        private RealizedRowCollection _realizedRows;

        public void InitRowViewConstructor(Func<RowView> rowViewConstructor)
        {
            _realizedRows.InitRowViewConstructor(rowViewConstructor);
        }

        public GridTemplate Template
        {
            get { return _presenter.Template; }
        }

        private int VirtualizingThreshold
        {
            get { return _presenter.VirtualizationThreshold; }
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
                    element.SetDataPresenter(null);
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
                element.SetDataPresenter(_presenter);
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

        public void OnRowRemoved(int index, RowPresenter row)
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

        private Size _availableSize;
        private GridColumn[] _autoWidthColumns;
        private GridRow[] _autoHeightRows;
        private GridColumn[] _starWidthColumns;
        private GridRow[] _starHeightRows;
        private IList<AutoSizeItem> _autoSizeItems;

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
            _availableSize = availableSize;
            InitGridTracks();
            InitMeasure();
            CalcStarSizeTracks();
            MeasureElements();
            CalcAccumulatedLengths(Template.GridColumns);
            CalcAccumulatedLengths(Template.GridRows);

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

        private void InitGridTracks()
        {
            bool sizeToContentX = double.IsPositiveInfinity(_availableSize.Width);
            bool sizeToContentY = double.IsPositiveInfinity(_availableSize.Height);

            bool gridColumnsReset = InitGridColumns(sizeToContentX);
            bool gridRowsReset = InitGridRows(sizeToContentY);
            GenerateAutoSizeItems(sizeToContentX, sizeToContentY, gridColumnsReset || gridRowsReset);
        }

        private bool InitGridColumns(bool sizeToContent)
        {
            var result = Template.GridColumns.Classify(sizeToContent, ref _autoWidthColumns, ref _starWidthColumns);
            InitMeasuredLength(_availableSize.Width, _autoWidthColumns, _starWidthColumns);
            return result;
        }

        private bool InitGridRows(bool sizeToContent)
        {
            var result = Template.GridRows.Classify(sizeToContent, ref _autoHeightRows, ref _starHeightRows);
            InitMeasuredLength(_availableSize.Height, _autoHeightRows, _starHeightRows);
            return result;
        }

        private void GenerateAutoSizeItems(bool sizeToContentX, bool sizeToContentY, bool reset)
        {
            if (_autoSizeItems != null && !reset)
                return;
            _autoSizeItems = AutoSizeItem.GenerateList(Template, sizeToContentX, sizeToContentY, reset);
        }

        private void CalcStarSizeTracks()
        {
            if (_starWidthColumns.Length > 0)
            {
                var usedSpace = Template.GridColumns.AbsoluteLengthTotal + _autoWidthColumns.Sum(x => x.MeasuredLength);
                CalcStarMeasuredLength(_availableSize.Width - usedSpace, _starWidthColumns);
            }

            if (_starHeightRows.Length > 0)
            {
                var usedSpace = Template.GridRows.AbsoluteLengthTotal + _autoHeightRows.Sum(x => x.MeasuredLength);
                CalcStarMeasuredLength(_availableSize.Height - usedSpace, _starHeightRows);
            }
        }

        /// <summary>Derived class must override this method to:
        /// 1. Realize row(s);
        /// 2. Update auto-sized MeasuredLength.
        /// </summary>
        protected abstract void InitMeasure();

        public virtual Orientation GrowOrientation
        {
            get { return Orientation.Vertical; }
        }

        private int FlowCount
        {
            get { return _presenter.FlowCount; }
            set { _presenter.FlowCount = value; }
        }

        private int GrowCount
        {
            get { return (_realizedRows.Count + FlowCount) / FlowCount - 1; }
        }

        protected abstract double GetMeasuredLength(GridTrack gridTrack, int repeatIndex);

        protected abstract void SetMeasureLength(GridTrack gridTrack, int repeatIndex, double value);

        protected virtual void CalcAccumulatedLengths(IReadOnlyList<GridTrack> gridTracks)
        {
            for (int i = 1; i < gridTracks.Count; i++)
                gridTracks[i].AccumulatedLength = gridTracks[i - 1].AccumulatedLength + gridTracks[i].MeasuredLength;
        }

        private RepeatPosition GetRepeatPosition(RowPresenter row)
        {
            Debug.Assert(row != null);
            Debug.Assert(_realizedRows.Count > 0);

            var offset = row.Index - _realizedRows.First.Index;
            Debug.Assert(offset >= 0 && offset < _realizedRows.Count);

            var flowIndex = offset % FlowCount;
            var growIndex = offset / FlowCount;
            Debug.Assert(growIndex < GrowCount);

            return GrowOrientation == Orientation.Vertical ? new RepeatPosition(flowIndex, growIndex) : new RepeatPosition(growIndex, flowIndex);
        }

        private void MeasureElements()
        {
            foreach (var element in Elements)
            {
                var rowView = element as RowView;
                if (rowView != null)
                    rowView.Measure(GetSize(rowView.Presenter));
                else
                    element.Measure(GetSize((ScalarItem)element.GetTemplateItem()));
            }
        }

        private double GetMeasuredLength(GridTrack start, GridTrack end, int repeatIndex = 0)
        {
            Debug.Assert(start != null && end != null);
            Debug.Assert(start.Owner == end.Owner && start.Orientation == end.Orientation);
            Debug.Assert(start.Ordinal <= end.Ordinal);

            double result = 0;
            var gridTracks = start.GridTracks;
            for (int i = start.Ordinal; i <= end.Ordinal; i++)
                result += GetMeasuredLength(gridTracks[i], repeatIndex);
            return result;
        }

        private Size GetSize(RowPresenter row)
        {
            var repeatPosition = GetRepeatPosition(row);
            var range = Template.RepeatRange;
            var width = GetMeasuredLength(range.Left, range.Right, repeatPosition.X);
            var height = GetMeasuredLength(range.Top, range.Bottom, repeatPosition.Y);
            return new Size(width, height);
        }

        private Point GetPoint(RowPresenter row)
        {
            throw new NotImplementedException();
        }

        private Rect GetRect(RowPresenter row)
        {
            return new Rect(GetPoint(row), GetSize(row));
        }

        private Size GetSize(ScalarItem scalarItem)
        {
            throw new NotImplementedException();
        }

        private Point GetPoint(ScalarItem scalarItem)
        {
            throw new NotImplementedException();
        }

        private Rect GetRect(ScalarItem scalarItem)
        {
            return new Rect(GetPoint(scalarItem), GetSize(scalarItem));
        }

        public Size GetSize(RowPresenter row, RepeatItem repeatItem)
        {
            throw new NotImplementedException();
        }

        public Point GetPoint(RowPresenter row, RepeatItem repeatItem)
        {
            throw new NotImplementedException();
        }

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
            foreach (var element in Elements)
            {
                var rowView = element as RowView;
                if (rowView != null)
                    rowView.Arrange(GetRect(rowView.Presenter));
                else
                    element.Arrange(GetRect((ScalarItem)element.GetTemplateItem()));
            }

            throw new NotImplementedException();
        }
    }
}

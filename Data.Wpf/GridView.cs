using DevZest.Data.Wpf.Resources;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class GridView
    {
        internal GridView()
        {
            GridRows = new GridDefinitionCollection<GridRow>();
            GridColumns = new GridDefinitionCollection<GridColumn>();
            ViewItems = new GridItemCollection(this);
        }

        public GridDefinitionCollection<GridRow> GridRows { get; private set; }
        public GridDefinitionCollection<GridColumn> GridColumns { get; private set; }
        public GridItemCollection ViewItems { get; private set; }

        internal DataSet DataSet { get; private set; }

        public IList<DataRow> DataRows
        {
            get { return DataSet.Rows; }
        }

        public Model Model
        {
            get { return DataSet.Model; }
        }

        private static GridLengthConverter s_gridLengthConverter = new GridLengthConverter();
        private static GridLength GetGridLength(string gridLength)
        {
            if (string.IsNullOrEmpty(gridLength))
                throw new ArgumentNullException(nameof(gridLength));

            return (GridLength)s_gridLengthConverter.ConvertFromInvariantString(gridLength);
        }

        private GridRange? _dataRowRange;
        public GridRange DataRowRange
        {
            get { return _dataRowRange.HasValue ? _dataRowRange.GetValueOrDefault() : GetGridRangeAll(); }
            internal set
            {
                VerifyDesignMode();
                if (!GetGridRangeAll().Contains(value) || !value.Contains(ViewItems.CalculatedDataRowRange))
                    throw new ArgumentOutOfRangeException(nameof(value));

                _dataRowRange = value;
            }
        }

        public int AddGridColumn(string width)
        {
            VerifyDesignMode();
            GridColumns.Add(new GridColumn(this, GridColumns.Count, GetGridLength(width)));
            return GridColumns.Count - 1;
        }

        public int AddGridRow(string height)
        {
            VerifyDesignMode();
            GridRows.Add(new GridRow(this, GridRows.Count, GetGridLength(height)));
            return GridRows.Count - 1;
        }

        public void AddViewItem(GridRange gridRange, GridItem viewItem)
        {
            VerifyDesignMode();
            VerifyGridRange(gridRange, nameof(gridRange));
            VerifyViewItem(viewItem, nameof(viewItem));

            ViewItems.Add(viewItem, gridRange);
        }

        private void VerifyViewItem(GridItem viewItem, string paramName)
        {
            if (viewItem == null)
                throw new ArgumentNullException(nameof(paramName));
            if (!viewItem.IsValidFor(DataSet.Model))
                throw new ArgumentException(Strings.DataSetView_InvalidViewItem(DataSet.Model), nameof(paramName));
        }

        private void VerifyGridRange(GridRange gridRange, string paramName)
        {
            if (!GetGridRangeAll().Contains(gridRange))
                throw new ArgumentOutOfRangeException(paramName);
        }

        private void VerifyGridColumn(int index, string paramName)
        {
            if (index < 0 || index >= GridColumns.Count)
                throw new ArgumentOutOfRangeException(paramName);
        }

        private void VerifyGridRow(int index, string paramName)
        {
            if (index < 0 || index >= GridRows.Count)
                throw new ArgumentOutOfRangeException(paramName);
        }

        private GridRange GetGridRangeAll()
        {
            if (GridColumns.Count == 0 || GridRows.Count == 0)
                return new GridRange();

            return new GridRange(GridColumns[0], GridRows[0], GridColumns[GridColumns.Count - 1], GridRows[GridRows.Count - 1]);
        }

        public GridRange this[int column, int row]
        {
            get
            {
                VerifyGridColumn(column, nameof(column));
                VerifyGridRow(row, nameof(row));
                return new GridRange(GridColumns[column], GridRows[row]);
            }
        }

        public GridRange this[int left, int top, int right, int bottom]
        {
            get
            {
                VerifyGridColumn(left, nameof(left));
                VerifyGridRow(top, nameof(top));
                VerifyGridColumn(right, nameof(right));
                VerifyGridRow(bottom, nameof(bottom));
                if (right < left)
                    throw new ArgumentOutOfRangeException(nameof(right));
                if (bottom < top)
                    throw new ArgumentOutOfRangeException(nameof(bottom));
                return new GridRange(GridColumns[left], GridRows[top], GridColumns[right], GridRows[bottom]);
            }
        }

        bool _designMode = true;
        public bool DesignMode
        {
            get { return _designMode; }
        }

        internal void BeginInit(DataSet dataSet)
        {
            DataSet = dataSet;
            GridRows.Clear();
            GridColumns.Clear();
            _designMode = true;
        }

        internal void EndInit()
        {
            _designMode = false;
        }

        protected void VerifyDesignMode()
        {
            if (!_designMode)
                throw Error.DataSetView_VerifyDesignMode();
        }

        private GridElementCollection _elements = new GridElementCollection();
        internal GridElementCollection Elements
        {
            get { return _elements; }
        }

        internal Size Measure(Size availableSize)
        {
            throw new NotImplementedException();
        }

        internal Size Arrange(Size finalSize)
        {
            throw new NotImplementedException();
        }

        internal Size ExtentSize { get; private set; }

        internal Point ViewportOffset { get; private set; }

        internal Size ViewportSize { get; private set; }

        public void SetHorizontalOffset(double offset)
        {
            ViewportOffset = new Point(offset, ViewportOffset.Y);
        }

        public void SetVerticalOffset(double offset)
        {
            ViewportOffset = new Point(ViewportOffset.X, offset);
        }
    }
}

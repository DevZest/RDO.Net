using DevZest.Data.Wpf.Resources;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DevZest.Data.Wpf
{
    internal class DataSetView : IScrollInfo
    {
        internal DataSetView()
        {
            GridRows = new GridDefinitionCollection<GridRow>();
            GridColumns = new GridDefinitionCollection<GridColumn>();
            Items = new ViewItemCollection(this);
        }

        public GridDefinitionCollection<GridRow> GridRows { get; private set; }
        public GridDefinitionCollection<GridColumn> GridColumns { get; private set; }
        public ViewItemCollection Items { get; private set; }

        public DataSet DataSet { get; private set; }

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
                if (!GetGridRangeAll().Contains(value) || !value.Contains(Items.CalculatedDataRowRange))
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

        public void AddViewItem(GridRange gridRange, ViewItem viewItem)
        {
            VerifyDesignMode();
            VerifyGridRange(gridRange, nameof(gridRange));
            VerifyViewItem(viewItem, nameof(viewItem));

            Items.Add(viewItem, gridRange);
        }

        private void VerifyViewItem(ViewItem viewItem, string paramName)
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

        public bool CanHorizontallyScroll
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool CanVerticallyScroll
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public double ExtentHeight
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double ExtentWidth
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double HorizontalOffset
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ScrollViewer ScrollOwner
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public double VerticalOffset
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double ViewportHeight
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double ViewportWidth
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void LineDown()
        {
            throw new NotImplementedException();
        }

        public void LineLeft()
        {
            throw new NotImplementedException();
        }

        public void LineRight()
        {
            throw new NotImplementedException();
        }

        public void LineUp()
        {
            throw new NotImplementedException();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }

        public void MouseWheelDown()
        {
            throw new NotImplementedException();
        }

        public void MouseWheelLeft()
        {
            throw new NotImplementedException();
        }

        public void MouseWheelRight()
        {
            throw new NotImplementedException();
        }

        public void MouseWheelUp()
        {
            throw new NotImplementedException();
        }

        public void PageDown()
        {
            throw new NotImplementedException();
        }

        public void PageLeft()
        {
            throw new NotImplementedException();
        }

        public void PageRight()
        {
            throw new NotImplementedException();
        }

        public void PageUp()
        {
            throw new NotImplementedException();
        }

        public void SetHorizontalOffset(double offset)
        {
            throw new NotImplementedException();
        }

        public void SetVerticalOffset(double offset)
        {
            throw new NotImplementedException();
        }
    }
}

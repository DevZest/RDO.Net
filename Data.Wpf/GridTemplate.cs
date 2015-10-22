using DevZest.Data.Primitives;
using DevZest.Data.Wpf.Resources;
using System;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class GridTemplate
    {
        internal GridTemplate()
        {
            GridRows = new GridDefinitionCollection<GridRow>();
            GridColumns = new GridDefinitionCollection<GridColumn>();
            ScalarItems = new GridItemCollection<ScalarGridItem>(this);
            SetItems = new GridItemCollection<SetGridItem>(this);
            ChildSetItems = new GridItemCollection<ChildSetGridItem>(this);
        }

        internal void BeginInit(Model model)
        {
            Model = model;
            GridRows.Clear();
            GridColumns.Clear();
            ScalarItems.Clear();
            SetItems.Clear();
            ChildSetItems.Clear();
            _isSealed = false;
        }

        internal void EndInit()
        {
            _isSealed = true;
        }


        bool _isSealed = false;
        public bool IsSealed
        {
            get { return _isSealed; }
        }
        private void VerifyIsSealed()
        {
            if (_isSealed)
                throw Error.GridTemplate_VerifyIsSealed();
        }


        public Model Model { get; private set; }

        private DataRowOrientation _orientation = DataRowOrientation.Y;
        public DataRowOrientation Orientation
        {
            get { return _orientation; }
            set
            {
                VerifyIsSealed();
                _orientation = value;
            }
        }

        public GridTemplate SetOrientation(DataRowOrientation value)
        {
            Orientation = value;
            return this;
        }

        private ScrollOption? _scrollOption;
        public ScrollOption ScrollOption
        {
            get { return _scrollOption.HasValue ? _scrollOption.GetValueOrDefault() : GetDefaultScrollOption(Orientation); }
            set
            {
                VerifyIsSealed();
                _scrollOption = value;
            }
        }

        private static ScrollOption GetDefaultScrollOption(DataRowOrientation orientation)
        {
            return orientation == DataRowOrientation.Z ? ScrollOption.None : ScrollOption.Virtualizing;
        }

        public GridTemplate SetScrollOption(ScrollOption value)
        {
            ScrollOption = value;
            return this;
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                VerifyIsSealed();
                _isReadOnly = value;
            }
        }

        public GridTemplate SetIsReadOnly(bool value)
        {
            IsReadOnly = value;
            return this;
        }

        private GridRange? _dataRowRange;
        public GridRange DataRowRange
        {
            get { return _dataRowRange.HasValue ? _dataRowRange.GetValueOrDefault() : GetGridRangeAll(); }
            set
            {
                VerifyIsSealed();
                if (!GetGridRangeAll().Contains(value) || !value.Contains(CalculatedDataRowRange))
                    throw new ArgumentOutOfRangeException(nameof(value));

                _dataRowRange = value;
            }
        }

        private GridRange CalculatedDataRowRange
        {
            get { return SetItems.Range.Union(ChildSetItems.Range); }
        }

        public GridTemplate SetDataRowRange(GridRange value)
        {
            DataRowRange = value;
            return this;
        }

        private int _frozenCount;
        public int FrozenCount
        {
            get { return _frozenCount; }
            set
            {
                VerifyIsSealed();
                if (value < 0)
                    throw new ArgumentOutOfRangeException();
                _frozenCount = value;
            }
        }

        public GridTemplate SetFrozenCount(int value)
        {
            FrozenCount = value;
            return this;
        }

        public GridDefinitionCollection<GridRow> GridRows { get; private set; }

        public GridDefinitionCollection<GridColumn> GridColumns { get; private set; }

        public GridItemCollection<ScalarGridItem> ScalarItems { get; private set; }

        public GridItemCollection<SetGridItem> SetItems { get; private set; }

        public GridItemCollection<ChildSetGridItem> ChildSetItems { get; private set; }

        private static GridLengthConverter s_gridLengthConverter = new GridLengthConverter();
        private static GridLength GetGridLength(string gridLength)
        {
            if (string.IsNullOrEmpty(gridLength))
                throw new ArgumentNullException(nameof(gridLength));

            return (GridLength)s_gridLengthConverter.ConvertFromInvariantString(gridLength);
        }

        public GridTemplate AddGridRows(params string[] heights)
        {
            if (heights != null)
                throw new ArgumentNullException(nameof(heights));

            foreach (var height in heights)
                AddGridRow(height);
            return this;
        }

        public GridTemplate AddGridColumns(params string[] widths)
        {
            if (widths != null)
                throw new ArgumentNullException(nameof(widths));

            foreach (var width in widths)
                AddGridColumn(width);
            return this;
        }

        private int AddGridColumn(string width)
        {
            VerifyIsSealed();
            GridColumns.Add(new GridColumn(this, GridColumns.Count, GetGridLength(width)));
            return GridColumns.Count - 1;
        }

        public GridTemplate AddGridColumn(string width, out int index)
        {
            index = AddGridColumn(width);
            return this;
        }

        private int AddGridRow(string height)
        {
            VerifyIsSealed();
            GridRows.Add(new GridRow(this, GridRows.Count, GetGridLength(height)));
            return GridRows.Count - 1;
        }

        public GridTemplate AddGridRow(string height, out int index)
        {
            index = AddGridRow(height);
            return this;
        }

        public GridTemplate AddItem(GridRange gridRange, ScalarGridItem gridItem)
        {
            VerifyAddItem(gridRange, gridItem);
            ScalarItems.Add(gridItem, gridRange);
            return this;
        }

        public GridTemplate AddItem(GridRange gridRange, SetGridItem gridItem)
        {
            VerifyAddItem(gridRange, gridItem);
            SetItems.Add(gridItem, gridRange);
            return this;
        }

        public GridTemplate AddItem(GridRange gridRange, ChildSetGridItem gridItem)
        {
            VerifyAddItem(gridRange, gridItem);
            ChildSetItems.Add(gridItem, gridRange);
            return this;
        }

        private void VerifyAddItem(GridRange gridRange, GridItem gridItem)
        {
            VerifyIsSealed();
            if (!GetGridRangeAll().Contains(gridRange))
                throw new ArgumentOutOfRangeException(nameof(gridRange));
            if (gridItem == null)
                throw new ArgumentNullException(nameof(gridItem));
            if (gridItem.Owner != null || (gridItem.ParentModel != null && gridItem.ParentModel != Model))
                throw new ArgumentException(Strings.GridTemplate_InvalidGridItem, nameof(gridItem));
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

        internal void DefaultInitialize()
        {
            var columns = Model.GetColumns();

            this.AddGridColumns(columns.Select(x => "Auto").ToArray())
                .AddGridRows("Auto", "Auto")
                .SetDataRowRange(this[0, 1, columns.Count - 1, 1]);

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                //this.AddScalarItem(this[i, 0], column)
                //    .AddSetItem(this[i, 1], column.TextBlock());
            }
        }
    }
}

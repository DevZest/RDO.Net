using DevZest.Data.Primitives;
using DevZest.Data.Wpf.Resources;
using System;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class GridView
    {
        internal GridView()
        {
            GridRows = new GridDefinitionCollection<GridRow>();
            GridColumns = new GridDefinitionCollection<GridColumn>();
            GridItems = new GridItemCollection(this);
        }

        bool _isSealed = false;
        public bool IsSealed
        {
            get { return _isSealed; }
        }
        private void VerifyIsSealed()
        {
            if (_isSealed)
                throw Error.GridView_VerifyIsSealed();
        }


        internal void BeginInit(Model model)
        {
            Model = model;
            GridRows.Clear();
            GridColumns.Clear();
            _isSealed = false;
        }

        internal void EndInit()
        {
            _isSealed = true;
        }

        public Model Model { get; private set; }

        private GridViewOrientation _orientation = GridViewOrientation.Y;
        public GridViewOrientation Orientation
        {
            get { return _orientation; }
            set
            {
                VerifyIsSealed();
                _orientation = value;
            }
        }

        public GridView SetOrientation(GridViewOrientation value)
        {
            Orientation = value;
            return this;
        }

        private GridRange? _dataRowRange;
        public GridRange DataRowRange
        {
            get { return _dataRowRange.HasValue ? _dataRowRange.GetValueOrDefault() : GetGridRangeAll(); }
            set
            {
                VerifyIsSealed();
                if (!GetGridRangeAll().Contains(value) || !value.Contains(GridItems.CalculatedDataRowRange))
                    throw new ArgumentOutOfRangeException(nameof(value));

                _dataRowRange = value;
            }
        }

        public GridView SetDataRowRange(GridRange value)
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

        public GridView SetFrozenCount(int value)
        {
            FrozenCount = value;
            return this;
        }

        public GridDefinitionCollection<GridRow> GridRows { get; private set; }
        public GridDefinitionCollection<GridColumn> GridColumns { get; private set; }
        public GridItemCollection GridItems { get; private set; }

        private static GridLengthConverter s_gridLengthConverter = new GridLengthConverter();
        private static GridLength GetGridLength(string gridLength)
        {
            if (string.IsNullOrEmpty(gridLength))
                throw new ArgumentNullException(nameof(gridLength));

            return (GridLength)s_gridLengthConverter.ConvertFromInvariantString(gridLength);
        }

        private int AddGridColumn(string width)
        {
            VerifyIsSealed();
            GridColumns.Add(new GridColumn(this, GridColumns.Count, GetGridLength(width)));
            return GridColumns.Count - 1;
        }

        public GridView AddGridColumn(string width, out int index)
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

        public GridView AddGridRow(string height, out int index)
        {
            index = AddGridRow(height);
            return this;
        }

        private void AddGridItem(GridRange gridRange, GridItem gridItem)
        {
            VerifyIsSealed();
            VerifyGridRange(gridRange, nameof(gridRange));
            VerifyGridItem(gridItem, nameof(gridItem));

            GridItems.Add(gridItem, gridRange);
        }

        private void VerifyGridItem(GridItem gridItem, string paramName)
        {
            if (gridItem == null)
                throw new ArgumentNullException(nameof(paramName));
            if (!gridItem.IsValidFor(Model))
                throw new ArgumentException(Strings.GridView_InvalidGridItemForModel(Model), nameof(paramName));
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

        internal void DefaultInitialize()
        {
            var columns = Model.GetColumns();

            this.AddGridColumns(columns.Select(x => "Auto").ToArray())
                .AddGridRows("Auto", "Auto")
                .SetDataRowRange(this[0, 1, columns.Count - 1, 1]);

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                this.AddColumnHeader(this[i, 0], column)
                    .AddColumnValue(this[i, 1], column.TextBlock());
            }
        }

        public GridView AddGridRows(params string[] heights)
        {
            if (heights != null)
                throw new ArgumentNullException(nameof(heights));

            foreach (var height in heights)
                AddGridRow(height);
            return this;
        }

        public GridView AddGridColumns(params string[] widths)
        {
            if (widths != null)
                throw new ArgumentNullException(nameof(widths));

            foreach (var width in widths)
                AddGridColumn(width);
            return this;
        }

        public GridView AddChildSet<T>(GridRange gridRange, ChildSetGridItem<T> gridItem)
            where T : DataSetControl, new()
        {
            AddGridItem(gridRange, gridItem);
            return this;
        }

        public GridView AddColumnValue<T>(GridRange gridRange, ColumnValueGridItem<T> gridItem)
            where T : UIElement, new()
        {
            AddGridItem(gridRange, gridItem);
            return this;
        }

        public GridView AddHeaderSelector(GridRange gridRange, Action<DataSetSelector> initializer = null)
        {
            AddGridItem(gridRange, new DataSetSelectorGridItem<DataSetSelector>(Model, initializer));
            return this;
        }

        public GridView AddRowSelector(GridRange gridRange, Action<DataRowSelector> initializer = null)
        {
            AddGridItem(gridRange, new DataRowSelectorGridItem<DataRowSelector>(Model, initializer));
            return this;
        }

        public GridView AddColumnHeader(GridRange gridRange, Column column, Action<ColumnHeader> initializer = null)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            AddGridItem(gridRange, new ColumnHeaderGridItem<ColumnHeader>(column, initializer));
            return this;
        }
    }
}

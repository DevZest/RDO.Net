using DevZest.Data.Primitives;
using DevZest.Data.Wpf.Resources;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public class DataSetControl : Control
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(DataSetControl.Orientation), typeof(LayoutOrientation), typeof(DataSetControl),
            new FrameworkPropertyMetadata(Wpf.LayoutOrientation.Y));
        public static readonly DependencyProperty ScrollableProperty = DependencyProperty.Register(nameof(Scrollable), typeof(bool), typeof(DataSetControl),
            new FrameworkPropertyMetadata(BooleanBoxes.True));
        public static readonly DependencyProperty FrozenGridCountProperty = DependencyProperty.Register(nameof(FrozenGridCount), typeof(int), typeof(DataSetControl),
            new FrameworkPropertyMetadata(0));

        public DataSetControl()
        {
            GridRows = new GridDefinitionCollection<GridRow>();
            GridColumns = new GridDefinitionCollection<GridColumn>();
            ViewManagers = new ViewManagerCollection(this);
        }

        public LayoutOrientation Orientation
        {
            get { return (LayoutOrientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public bool Scrollable
        {
            get { return (bool)GetValue(ScrollableProperty); }
            set { SetValue(ScrollableProperty, BooleanBoxes.Box(value)); }
        }

        public int FrozenGridCount
        {
            get { return (int)GetValue(FrozenGridCountProperty); }
            set { SetValue(FrozenGridCountProperty, value); }
        }

        private GridRange? _rowsPanelRange;
        public GridRange RowsPanelRange
        {
            get { return _rowsPanelRange.HasValue ? _rowsPanelRange.GetValueOrDefault() : GetGridRangeAll(); }
            internal set
            {
                VerifyDesignMode();
                if (!GetGridRangeAll().Contains(value) || !value.Contains(ViewManagers.CalculatedRowsPanelRange))
                    throw new ArgumentOutOfRangeException(nameof(value));

                _rowsPanelRange = value;
            }
        }

        internal DataSet DataSet { get; set; }
        public GridDefinitionCollection<GridRow> GridRows { get; private set; }
        public GridDefinitionCollection<GridColumn> GridColumns { get; private set; }
        internal ViewManagerCollection ViewManagers { get; private set; }

        internal Model Model
        {
            get { return DataSet.Model; }
        }

        private static DataGridLengthConverter s_gridLengthConverter = new DataGridLengthConverter();
        private static DataGridLength GetGridLength(string gridLength)
        {
            if (string.IsNullOrEmpty(gridLength))
                throw new ArgumentNullException(nameof(gridLength));

            return (DataGridLength)s_gridLengthConverter.ConvertFromInvariantString(gridLength);
        }

        internal int InitGridColumn(string width)
        {
            VerifyDesignMode();
            GridColumns.Add(new GridColumn(this, GridColumns.Count, GetGridLength(width)));
            return GridColumns.Count - 1;
        }

        internal int InitGridRow(string height)
        {
            VerifyDesignMode();
            GridRows.Add(new GridRow(this, GridRows.Count, GetGridLength(height)));
            return GridRows.Count - 1;
        }

        public void InitView(GridRange gridRange, ViewManager manager)
        {
            VerifyDesignMode();
            VerifyGridRange(gridRange, nameof(gridRange));
            VerifyViewManager(manager, nameof(manager));

            ViewManagers.Add(manager, gridRange);
        }

        private void VerifyViewManager(ViewManager manager, string paramName)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(paramName));
            if (!manager.IsValidFor(DataSet.Model))
                throw new ArgumentException(Strings.DataSetControl_InvalidViewManager(DataSet.Model), nameof(paramName));
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

        internal void BeginInitialization(DataSet dataSet)
        {
            DataSet = dataSet;
            GridRows.Clear();
            GridColumns.Clear();
            _designMode = true;
        }

        internal void EndInitialization()
        {
            _designMode = false;
        }

        protected void VerifyDesignMode()
        {
            if (!_designMode)
                throw Error.DataSetControl_VerifyDesignMode();
        }

        internal void DefaultInitialize()
        {
            var model = DataSet.Model;
            var columns = model.GetColumns();

            this.GridColumns(columns.Select(x => "Auto").ToArray())
                .GridRows("Auto", "Auto")
                .RowsPanel(this[0, 1, columns.Count, 1]);

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                this.ColumnHeader(this[i, 0], column)
                    .ColumnValue(this[i, 1], column.TextBlock());
            }
        }
    }
}

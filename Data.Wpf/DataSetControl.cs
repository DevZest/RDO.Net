using DevZest.Data.Primitives;
using DevZest.Data.Wpf.Resources;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public class DataSetControl : Control
    {
        public static readonly DependencyProperty RowOrientationProperty = DependencyProperty.Register(nameof(RowOrientation), typeof(RowOrientation), typeof(DataSetControl),
            new FrameworkPropertyMetadata(RowOrientation.Y));
        public static readonly DependencyProperty ScrollableProperty = DependencyProperty.Register(nameof(Scrollable), typeof(bool), typeof(DataSetControl),
            new FrameworkPropertyMetadata(true));

        public DataSetControl()
        {
            GridRows = new GridDefinitionCollection<GridRow>();
            GridColumns = new GridDefinitionCollection<GridColumn>();
            ViewGenerators = new ViewGeneratorCollection(this);
        }

        public RowOrientation RowOrientation
        {
            get { return (RowOrientation)GetValue(RowOrientationProperty); }
            set { SetValue(RowOrientationProperty, value); }
        }

        public bool Scrollable
        {
            get { return (bool)GetValue(ScrollableProperty); }
            set { SetValue(ScrollableProperty, value); }
        }

        internal DataSet DataSet { get; set; }
        public GridDefinitionCollection<GridRow> GridRows { get; private set; }
        public GridDefinitionCollection<GridColumn> GridColumns { get; private set; }
        internal ViewGeneratorCollection ViewGenerators { get; private set; }


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
            VerifyAreGridsInherited();
            GridColumns.Add(new GridColumn(this, GridColumns.Count, GetGridLength(width)));
            return GridColumns.Count - 1;
        }

        internal int InitGridRow(string height)
        {
            VerifyDesignMode();
            VerifyAreGridsInherited();
            GridRows.Add(new GridRow(this, GridRows.Count, GetGridLength(height)));
            return GridRows.Count - 1;
        }

        private bool _areGridsInherited;
        private ViewGenerator _panelGenerator;
        internal void InitGridsInherited()
        {
            VerifyDesignMode();
            if (GridRows.Count > 0 || GridColumns.Count > 0)
                throw Error.DataSetControl_InheritGrids();

            if (_panelGenerator == null)
                throw Error.DataSetControl_NullPanelGenerator();

            var gridRange = _panelGenerator.GridRange;
            var sourceControl = gridRange.DataSetControl;
            for (int i = gridRange.Left.Ordinal; i <= gridRange.Right.Ordinal; i++)
                GridColumns.Add(sourceControl.GridColumns[i]);
            for (int i = gridRange.Top.Ordinal; i <= gridRange.Bottom.Ordinal; i++)
                GridRows.Add(sourceControl.GridColumns[i]);

            _areGridsInherited = true;
        }

        private void VerifyAreGridsInherited()
        {
            if (_areGridsInherited)
                throw Error.DataSetControl_VerifyAreGridsInherited();
        }

        internal void InitHeaderSelector<T>(GridRange gridRange, HeaderSelectorGenerator<T> generator = null)
            where T : HeaderSelector, new()
        {
            VerifyDesignMode();
            VerifyGridRange(gridRange, nameof(GridRange));
            throw new NotImplementedException();
        }

        internal void InitRowSelector<T>(GridRange gridRange, RowSelectorGenerator<T> generator = null)
            where T : RowSelector, new()
        {
            VerifyDesignMode();
            VerifyGridRange(gridRange, nameof(GridRange));
            if (generator == null)
                throw new NotImplementedException();
            else
                VerifyViewGenerator(generator, nameof(generator));
            ViewGenerators.Add(generator, gridRange);
        }

        internal void InitColumnHeader<T>(GridRange gridRange, ColumnHeaderGenerator<T> generator)
            where T : ColumnHeader, new()
        {
            VerifyDesignMode();
            VerifyGridRange(gridRange, nameof(GridRange));
            VerifyViewGenerator(generator, nameof(generator));
            ViewGenerators.Add(generator, gridRange);
        }

        internal void InitColumnValue<T>(GridRange gridRange, ColumnValueGenerator<T> generator)
            where T : UIElement, new()
        {
            VerifyDesignMode();
            VerifyGridRange(gridRange, nameof(GridRange));
            VerifyViewGenerator(generator, nameof(generator));
            ViewGenerators.Add(generator, gridRange);
        }

        internal void InitPanel<T>(GridRange gridRange, PanelGenerator<T> generator)
            where T : DataSetControl, new()
        {
            VerifyDesignMode();
            VerifyGridRange(gridRange, nameof(gridRange));
            VerifyGenerator(generator, nameof(generator));

            ViewGenerators.Add(generator, gridRange);
        }

        private void VerifyGenerator(ViewGenerator generator, string paramName)
        {
            if (generator == null)
                throw new ArgumentNullException(paramName);
            if (!generator.IsValidFor(DataSet.Model))
                throw new ArgumentException(Strings.DataSetControl_InvalidGenerator(DataSet.Model), nameof(paramName));
        }

        private void VerifyGridRange(GridRange gridRange, string paramName)
        {
            if (!GetGridRangeAll().Contains(gridRange))
                throw new ArgumentOutOfRangeException(paramName);
        }

        private void VerifyViewGenerator(ViewGenerator generator, string paramName)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(paramName));
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

        internal void BeginInitialization(ViewGenerator panelGenerator, DataSet dataSet)
        {
            _panelGenerator = panelGenerator;
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
            RowOrientation = RowOrientation.Z;
            InitGridRow("Auto");
            InitGridRow("Auto");

            var model = DataSet.Model;
            var columns = model.GetColumns();
            foreach (var column in columns)
            {
                int columnIndex = InitGridColumn("Auto");
                //InitColumnHeader(this[columnIndex, 0], column);
            }

            InitPanel(this[0, 1, columns.Count - 1, 1], model.Panel((DataSetControl x, Model m) =>
            {
                x.RowOrientationY().InitGridsInherited();
                //for (int i = 0; i < columns.Count; i++)
                //    x.InitColumnValue(x[i, 0], columns[i]);
            }));
        }
    }
}

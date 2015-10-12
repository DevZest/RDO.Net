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
        private DataGridLength GetGridLength(string gridLength)
        {
            if (string.IsNullOrEmpty(gridLength))
                throw new ArgumentNullException(nameof(gridLength));

            return (DataGridLength)s_gridLengthConverter.ConvertFromInvariantString(gridLength);
        }

        public DataSetControl GridColumn(string width)
        {
            VerifyAreGridsInherited();
            GridColumns.Add(new GridColumn(this, GridColumns.Count, GetGridLength(width)));
            return this;
        }

        public DataSetControl GridColumn(string width, out GridColumn gridColumn)
        {
            VerifyAreGridsInherited();
            GridColumns.Add(gridColumn = new GridColumn(this, GridColumns.Count, GetGridLength(width)));
            return this;
        }

        public DataSetControl GridRow(string height)
        {
            VerifyAreGridsInherited();
            GridRows.Add(new GridRow(this, GridRows.Count, GetGridLength(height)));
            return this;
        }

        public DataSetControl GridRow(string height, out GridRow gridRow)
        {
            VerifyAreGridsInherited();
            GridRows.Add(gridRow = new GridRow(this, GridRows.Count, GetGridLength(height)));
            return this;
        }

        private bool _areGridsInherited;
        public DataSetControl InheritGrids()
        {
            if (GridRows.Count > 0 || GridColumns.Count > 0)
                throw Error.DataSetControl_InheritGrids();

            throw new NotImplementedException();

            //_areGridsInherited = true;
            //return this;
        }

        private void VerifyAreGridsInherited()
        {
            if (_areGridsInherited)
                throw Error.DataSetControl_VerifyAreGridsInherited();
        }

        public DataSetControl HeaderSelector(GridRange gridRange, HeaderSelectorGenerator generator = null)
        {
            throw new NotImplementedException();
        }

        public DataSetControl RowSelector(GridRange gridRange, RowSelectorGenerator generator = null)
        {
            throw new NotImplementedException();
        }

        public DataSetControl ColumnHeader(GridRange gridRange, Column column)
        {
            throw new NotImplementedException();
        }

        public DataSetControl ColumnHeader(GridRange gridRange, ColumnHeaderGenerator generator)
        {
            throw new NotImplementedException();
        }

        public DataSetControl ColumnValue(GridRange gridRange, Column column)
        {
            throw new NotImplementedException();
        }

        public DataSetControl ColumnValue(GridRange gridRange, ColumnValueGenerator generator)
        {
            throw new NotImplementedException();
        }

        public DataSetControl Panel(GridRange gridRange, PanelGenerator generator)
        {
            throw new NotImplementedException();
        }

        private void VerifyGridDefinition(GridDefinition gridDefinition, string paramName)
        {
            if (gridDefinition == null)
                throw new ArgumentNullException(paramName);
            if (gridDefinition.DataSetControl != this)
                throw new ArgumentException(Strings.DataSetControl_InvalidGridDefinition, paramName);
        }

        public GridRange this[GridColumn column, GridRow row]
        {
            get
            {
                VerifyGridDefinition(column, nameof(column));
                VerifyGridDefinition(row, nameof(row));
                return new GridRange(column, row);
            }
        }

        public GridRange this[GridColumn left, GridRow top, GridColumn right, GridRow bottom]
        {
            get
            {
                VerifyGridDefinition(left, nameof(left));
                VerifyGridDefinition(top, nameof(top));
                VerifyGridDefinition(right, nameof(right));
                VerifyGridDefinition(bottom, nameof(bottom));
                return new GridRange(left, top, right, bottom);
            }
        }


        internal void Initialize(DataSet dataSet)
        {
            DataSet = dataSet;
            GridRows.Clear();
            GridColumns.Clear();
        }

        internal void DefaultInitialize()
        {
            RowOrientation = RowOrientation.Z;
            GridRow row0, row1;
            GridRow("Auto", out row0);
            GridRow("Auto", out row1);

            var model = DataSet.Model;
            var columns = model.GetColumns();
            foreach (var column in columns)
            {
                GridColumn gridColumn;
                GridColumn("Auto", out gridColumn);
                ColumnHeader(this[gridColumn, row0], column);
            }

            Panel(this[GridColumns[0], row1, GridColumns[columns.Count - 1], row1], model.Panel((DataSetControl x, Model m) =>
            {
                x.RowOrientationY().InheritGrids();
                for (int i = 0; i < columns.Count; i++)
                    x.ColumnValue(x[GridColumns[i], row1], columns[i]);
            }));
        }
    }
}

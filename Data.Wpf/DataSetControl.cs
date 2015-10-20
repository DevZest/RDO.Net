using DevZest.Data.Primitives;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public class DataSetControl : Control
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(DataSetControl.Orientation), typeof(DataRowOrientation), typeof(DataSetControl),
            new FrameworkPropertyMetadata(Wpf.DataRowOrientation.Y));
        public static readonly DependencyProperty ScrollableProperty = DependencyProperty.Register(nameof(Scrollable), typeof(bool), typeof(DataSetControl),
            new FrameworkPropertyMetadata(BooleanBoxes.True));
        public static readonly DependencyProperty FrozenGridCountProperty = DependencyProperty.Register(nameof(FrozenGridCount), typeof(int), typeof(DataSetControl),
            new FrameworkPropertyMetadata(0));

        public DataSetControl()
        {
            View = new DataSetView();
        }

        public DataRowOrientation Orientation
        {
            get { return (DataRowOrientation)GetValue(OrientationProperty); }
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

        internal DataSetView View { get; private set; }

        public Model Model
        {
            get { return View.Model; }
        }

        public GridDefinitionCollection<GridRow> GridRows
        {
            get { return View.GridRows; }
        }

        public GridDefinitionCollection<GridColumn> GridColumns
        {
            get { return View.GridColumns; }
        }

        public ViewItemCollection ViewItems
        {
            get { return View.ViewItems; }
        }

        public GridRange this[int column, int row]
        {
            get { return View[column, row]; }
        }

        public GridRange this[int left, int top, int right, int bottom]
        {
            get { return View[left, top, right, bottom]; }
        }

        internal void DefaultInitialize()
        {
            var columns = Model.GetColumns();

            this.GridColumns(columns.Select(x => "Auto").ToArray())
                .GridRows("Auto", "Auto")
                .DataRowRange(this[0, 1, columns.Count - 1, 1]);

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                this.ColumnHeader(this[i, 0], column)
                    .ColumnValue(this[i, 1], column.TextBlock());
            }
        }
    }
}

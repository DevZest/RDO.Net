using DevZest.Data.Primitives;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public class DataSetControl : Control
    {
        public static readonly DependencyProperty ScrollableProperty = DependencyProperty.Register(nameof(Scrollable), typeof(bool), typeof(DataSetControl),
            new FrameworkPropertyMetadata(BooleanBoxes.True));
        public static readonly DependencyProperty FrozenGridCountProperty = DependencyProperty.Register(nameof(FrozenGridCount), typeof(int), typeof(DataSetControl),
            new FrameworkPropertyMetadata(0));

        public DataSetControl()
        {
            GridView = new GridView();
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

        private GridView _gridView;
        public GridView GridView
        {
            get
            {
                if (_gridView == null)
                    _gridView = new GridView();
                return _gridView;
            }
            internal set { _gridView = value; }
        }

        public Model Model
        {
            get { return GridView.Model; }
        }

        public void Initialize<TModel>(DataSet<TModel> dataSet, Action<GridView, TModel> gridViewInitializer = null)
            where TModel : Model, new()
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            var gridView = GridView;
            gridView.BeginInit(dataSet.Model);
            if (gridViewInitializer != null)
                gridViewInitializer(gridView, dataSet._);
            else
                gridView.DefaultInitialize();
            gridView.EndInit();
        }
    }
}

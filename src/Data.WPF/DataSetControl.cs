using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class DataSetControl : Control
    {
        private static readonly DependencyPropertyKey DataSetManagerPropertyKey = DependencyProperty.RegisterReadOnly(nameof(DataSetManager),
            typeof(DataSetManager), typeof(DataSetControl), new FrameworkPropertyMetadata(null, OnManagerChanged));

        public static readonly DependencyProperty DataSetManagerProperty = DataSetManagerPropertyKey.DependencyProperty;

        public static readonly DependencyProperty ScrollableProperty = DependencyProperty.Register(nameof(Scrollable),
            typeof(bool), typeof(DataSetControl), new FrameworkPropertyMetadata(BooleanBoxes.True));

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarVisibilityProperty),
            typeof(ScrollBarVisibility), typeof(DataSetControl), new PropertyMetadata(ScrollBarVisibility.Auto));

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarVisibility),
            typeof(ScrollBarVisibility), typeof(DataSetControl), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));

        public static readonly DependencyProperty ScrollLineHeightProperty = DependencyProperty.Register(nameof(ScrollLineHeight),
            typeof(double), typeof(DataSetControl), new FrameworkPropertyMetadata(20.0d));

        public static readonly DependencyProperty ScrollLineWidthProperty = DependencyProperty.Register(nameof(ScrollLineWidth),
            typeof(double), typeof(DataSetControl), new FrameworkPropertyMetadata(20.0d));

        private static void OnManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (DataSetManager)e.OldValue;
            var newValue = (DataSetManager)e.NewValue;
            ((DataSetControl)d).OnManagerChanged(oldValue, newValue);
        }

        private void OnManagerChanged(DataSetManager oldValue, DataSetManager newValue)
        {
            if (oldValue != null)
                oldValue.DataSetControl = null;

            Debug.Assert(newValue != null);
            newValue.DataSetControl = this;
        }

        public DataSetManager DataSetManager
        {
            get { return (DataSetManager)GetValue(DataSetManagerProperty); }
            internal set { SetValue(DataSetManagerPropertyKey, value); }
        }

        public bool Scrollable
        {
            get { return (bool)GetValue(ScrollableProperty); }
            set { SetValue(ScrollableProperty, value); }
        }

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
            set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        public double ScrollLineHeight
        {
            get { return (double)GetValue(ScrollLineHeightProperty); }
            set { SetValue(ScrollLineHeightProperty, value); }
        }

        public double ScrollLineWidth
        {
            get { return (double)GetValue(ScrollLineWidthProperty); }
            set { SetValue(ScrollLineWidthProperty, value); }
        }

        public void Initialize<TModel>(DataSet<TModel> dataSet, Action<GridTemplate, TModel> templateInitializer = null)
            where TModel : Model, new()
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            var gridTemplate = new GridTemplate(dataSet.Model);
            if (templateInitializer != null)
                templateInitializer(gridTemplate, dataSet._);
            else
                gridTemplate.DefaultInitialize();
            gridTemplate.Seal();
            DataSetManager = new DataSetManager(null, gridTemplate);
        }
    }
}

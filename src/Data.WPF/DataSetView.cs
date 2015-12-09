using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public class DataSetView : Control
    {
        private static readonly DependencyPropertyKey ManagerPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Manager),
            typeof(DataSetManager), typeof(DataSetView), new FrameworkPropertyMetadata(null, OnManagerChanged));

        private static void OnManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (DataSetManager)e.OldValue;
            var newValue = (DataSetManager)e.NewValue;
            ((DataSetView)d).OnManagerChanged(oldValue, newValue);
        }

        private void OnManagerChanged(DataSetManager oldValue, DataSetManager newValue)
        {
            if (oldValue != null)
                oldValue.DataSetView = null;

            Debug.Assert(newValue != null);
            newValue.DataSetView = this;
            ScrollMode = newValue.Template.ScrollMode;
        }

        public static readonly DependencyProperty ManagerProperty = ManagerPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ScrollModePropertyKey = DependencyProperty.RegisterReadOnly(nameof(ScrollMode),
            typeof(ScrollMode), typeof(DataSetView), new FrameworkPropertyMetadata(ScrollMode.None));

        public static readonly DependencyProperty ScrollModeProperty = ScrollModePropertyKey.DependencyProperty;

        public static readonly DependencyProperty ScrollLineHeightProperty = DependencyProperty.Register(nameof(ScrollLineHeight),
            typeof(double), typeof(DataSetView), new FrameworkPropertyMetadata(20.0d));

        public static readonly DependencyProperty ScrollLineWidthProperty = DependencyProperty.Register(nameof(ScrollLineWidth),
            typeof(double), typeof(DataSetView), new FrameworkPropertyMetadata(20.0d));

        public DataSetManager Manager
        {
            get { return (DataSetManager)GetValue(ManagerProperty); }
            internal set { SetValue(ManagerPropertyKey, value); }
        }

        public ScrollMode ScrollMode
        {
            get { return (ScrollMode)GetValue(ScrollModeProperty); }
            private set { SetValue(ScrollModePropertyKey, value); }
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
            Manager = new DataSetManager(null, gridTemplate);
        }
    }
}

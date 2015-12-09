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
        private static readonly DependencyPropertyKey ManagerPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Manager),
            typeof(DataSetManager), typeof(DataSetControl), new FrameworkPropertyMetadata(null, OnManagerChanged));

        private static void OnManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (DataSetManager)e.NewValue;
            Debug.Assert(newValue != null);
            ((DataSetControl)d).ScrollMode = newValue.Template.ScrollMode;
        }

        public static readonly DependencyProperty ManagerProperty = ManagerPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ScrollModePropertyKey = DependencyProperty.RegisterReadOnly(nameof(ScrollMode),
            typeof(ScrollMode), typeof(DataSetControl), new FrameworkPropertyMetadata(ScrollMode.None));

        public static readonly DependencyProperty ScrollModeProperty = ScrollModePropertyKey.DependencyProperty;

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

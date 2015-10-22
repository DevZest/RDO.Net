using DevZest.Data.Primitives;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public class DataSetGrid : Control
    {
        public static readonly DependencyProperty ScrollOptionProperty = DependencyProperty.Register(nameof(ScrollOption), typeof(ScrollOption), typeof(DataSetGrid),
            new FrameworkPropertyMetadata(BooleanBoxes.True));

        public DataSetGrid()
        {
        }

        public ScrollOption ScrollOption
        {
            get { return (ScrollOption)GetValue(ScrollOptionProperty); }
            set { SetValue(ScrollOptionProperty, value); }
        }

        public DataSetView View { get; private set; }

        public void Initialize<TModel>(DataSet<TModel> dataSet, Action<GridTemplate, TModel> templateInitializer = null)
            where TModel : Model, new()
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            var gridTemplate = new GridTemplate();
            gridTemplate.BeginInit(dataSet.Model);
            if (templateInitializer != null)
                templateInitializer(gridTemplate, dataSet._);
            else
                gridTemplate.DefaultInitialize();
            gridTemplate.EndInit();
            View = new DataSetView(null, gridTemplate);
        }
    }
}

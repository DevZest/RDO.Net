using System;

namespace DevZest.Data.Windows
{
    public static class DataSetViewFactory
    {
        public static ChildGridItem<DataSetView> DataSetView<T>(this T childModel, Action<DataSetPresenterConfig, T> configAction, Func<DataSetView> constructor = null)
            where T : Model
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (configAction == null)
                throw new ArgumentNullException(nameof(configAction));

            //var template = new GridTemplate(childModel);
            //configAction(template, childModel);
            //var result = new ChildGridItem<DataSetView>(template);
            //if (initializer != null)
            //    result.Initializer = initializer;
            //return result;
            throw new NotImplementedException();
        }
    }
}

using System;

namespace DevZest.Data.Windows
{
    internal static class Util
    {
        internal static DataSetPresenter CreateDataSetPresenter<T>(DataSet<T> dataSet, Action<GridTemplate> templateInitializer = null)
            where T : Model, new()
        {
            var template = new GridTemplate(dataSet.Model);
            if (templateInitializer != null)
                templateInitializer(template);

            return new DataSetPresenter(null, template);
        }
    }
}

using System;

namespace DevZest.Data.Windows
{
    public static class DataSetViewFactory
    {
        public static ChildGridItem<DataSetView> DataSetView<T>(this T childModel, Action<GridTemplate, T> templateInitializer, Action<DataSetView> initializer = null)
            where T : Model
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (templateInitializer == null)
                throw new ArgumentNullException(nameof(templateInitializer));

            var template = new GridTemplate(childModel);
            templateInitializer(template, childModel);
            template.Seal();
            var result = new ChildGridItem<DataSetView>(template);
            if (initializer != null)
                result.Initializer = initializer;
            return result;
        }
    }
}

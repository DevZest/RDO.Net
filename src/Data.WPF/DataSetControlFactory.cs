using System;

namespace DevZest.Data.Windows
{
    public static class DataSetControlFactory
    {
        public static ChildGridItem<DataSetControl> DataSetControl<T>(this T childModel, Action<GridTemplate, T> templateInitializer, Action<DataSetControl> initializer = null)
            where T : Model
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (templateInitializer == null)
                throw new ArgumentNullException(nameof(templateInitializer));

            var template = new GridTemplate(childModel);
            templateInitializer(template, childModel);
            template.Seal();
            var result = new ChildGridItem<DataSetControl>(template);
            if (initializer != null)
                result.Initializer = initializer;
            return result;
        }
    }
}

using System;

namespace DevZest.Data.Windows
{
    public static class DataSetGridFactory
    {
        public static ChildGridItem<T> DataSetGrid<TModel, T>(this TModel childModel, Action<GridTemplate, TModel> templateInitializer, Action<T> initializer = null)
            where TModel : Model
            where T : DataSetGrid, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (templateInitializer == null)
                throw new ArgumentNullException(nameof(templateInitializer));

            var template = new GridTemplate(childModel);
            templateInitializer(template, childModel);
            template.Seal();
            return new ChildGridItem<T>(template, initializer);
        }
    }
}

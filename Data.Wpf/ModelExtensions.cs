using System;

namespace DevZest.Data.Wpf
{
    public static class ModelExtensions
    {
        public static PanelGenerator<TDataSetControl> Panel<TModel, TDataSetControl>(this TModel model, Action<TDataSetControl, TModel> initializer)
            where TModel : Model
            where TDataSetControl : DataSetControl, new()
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));

            return new PanelGenerator<TDataSetControl>(model, x => initializer(x, model));
        }
    }
}

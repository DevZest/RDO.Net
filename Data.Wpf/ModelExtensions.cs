using System;
using System.Diagnostics;

namespace DevZest.Data.Wpf
{
    public static class ModelExtensions
    {
        public static PanelManager<TDataSetControl> Panel<TModel, TDataSetControl>(this TModel model, Action<TDataSetControl, TModel> initializer)
            where TModel : Model
            where TDataSetControl : DataSetControl, new()
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));

            return new PanelManager<TDataSetControl>(model, x => initializer(x, model));
        }
    }
}

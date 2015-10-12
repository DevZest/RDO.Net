using System;

namespace DevZest.Data.Wpf
{
    public static class ModelExtensions
    {
        public static PanelGenerator Panel<TModel, TDataSetControl>(this TModel model, Action<TDataSetControl, TModel> initializer)
            where TModel : Model
            where TDataSetControl : DataSetControl, new()
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));

            Func<DataSetControl> creator = () => new TDataSetControl();
            Action<DataSetControl> initializeAction = x => initializer((TDataSetControl)x, model);
            return new PanelGenerator(model, creator, initializeAction);
        }
    }
}

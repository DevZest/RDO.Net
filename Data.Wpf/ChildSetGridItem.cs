using DevZest.Data.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class ChildSetGridItem<T> : ModelGridItem<T>
        where T : DataSetControl, new()
    {
        internal ChildSetGridItem(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }

        internal override void InitUIElementOverride(T dataSetControl)
        {
            dataSetControl.GridView.BeginInit(Model);
            base.InitUIElementOverride(dataSetControl);
            dataSetControl.GridView.EndInit();
        }

        internal sealed override bool IsValidFor(Model model)
        {
            return Model.GetParentModel() == model;
        }
    }
}

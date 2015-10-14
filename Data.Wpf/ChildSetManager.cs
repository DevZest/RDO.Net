using DevZest.Data.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class ChildSetManager<T> : ModelViewManager<T>
        where T : DataSetControl, new()
    {
        internal ChildSetManager(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }

        internal override void InitUIElementOverride(T dataSetControl)
        {
            dataSetControl.BeginInitialization(GetDataSet(dataSetControl));
            base.InitUIElementOverride(dataSetControl);
            dataSetControl.EndInitialization();
        }

        internal sealed override bool IsValidFor(Model model)
        {
            return Model.GetParentModel() == model;
        }

        private DataSet GetDataSet(UIElement uiElement)
        {
            var dataRowControl = uiElement.GetParent<DataRowControl>();
            return DataSet.Get(dataRowControl, Model);
        }

        internal sealed override ViewManagerKind Kind
        {
            get { return ViewManagerKind.ChildSet; }
        }
    }
}

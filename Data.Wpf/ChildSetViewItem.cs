using DevZest.Data.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class ChildSetViewItem<T> : ModelViewItem<T>
        where T : DataSetControl, new()
    {
        internal ChildSetViewItem(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }

        internal override void InitUIElementOverride(T dataSetControl)
        {
            dataSetControl.View.BeginInitialization(GetDataSet(dataSetControl));
            base.InitUIElementOverride(dataSetControl);
            dataSetControl.View.EndInitialization();
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

        internal sealed override ViewItemKind Kind
        {
            get { return ViewItemKind.ChildSet; }
        }
    }
}

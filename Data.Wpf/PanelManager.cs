using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class PanelManager<T> : ModelViewManager<T>
        where T : DataSetControl, new()
    {
        internal PanelManager(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }

        internal override void InitUIElementOverride(T dataSetControl)
        {
            dataSetControl.BeginInitialization(this, GetDataSet(dataSetControl));
            base.InitUIElementOverride(dataSetControl);
            dataSetControl.EndInitialization();
        }

        private DataSet GetDataSet(UIElement uiElement)
        {
            var dataRowControl = uiElement.GetParent<DataRowControl>();
            return DataSet.Get(dataRowControl, Model);
        }
    }
}

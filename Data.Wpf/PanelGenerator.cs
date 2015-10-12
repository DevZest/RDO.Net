using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class PanelGenerator<T> : ModelViewGenerator<T>
        where T : DataSetControl, new()
    {
        internal PanelGenerator(Model model, Action<T> initializer)
            : base(model, initializer)
        {
        }

        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.Panel; }
        }

        internal override void InitializeUIElementOverride(T uiElement)
        {
            uiElement.BeginInitialization(this, GetDataSet(uiElement));
            base.InitializeUIElementOverride(uiElement);
            uiElement.EndInitialization();
        }

        private DataSet GetDataSet(UIElement uiElement)
        {
            var dataRowControl = uiElement.GetParent<DataRowControl>();
            return DataSet.Get(dataRowControl, Model);
        }
    }
}

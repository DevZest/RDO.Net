using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class PanelGenerator : ModelViewGenerator<DataSetControl>
    {
        internal PanelGenerator(Model model, Func<DataSetControl> creator, Action<DataSetControl> initializer)
            : base(model, creator, initializer)
        {
        }

        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.Panel; }
        }

        internal override void InitializeUIElementOverride(DataSetControl uiElement)
        {
            uiElement.Initialize(this, GetDataSet(uiElement));
            base.InitializeUIElementOverride(uiElement);
        }

        private DataSet GetDataSet(UIElement uiElement)
        {
            var dataRowControl = uiElement.GetParent<DataRowControl>();
            return DataSet.Get(dataRowControl, Model);
        }
    }
}

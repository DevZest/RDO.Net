using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Wpf
{
    public sealed class PanelGenerator : ViewGenerator
    {
        internal PanelGenerator(Model model, Func<DataSetControl> creator, Action<DataSetControl> initializer)
        {
            Debug.Assert(model != null);
            Debug.Assert(creator != null);
            Debug.Assert(initializer != null);

            _creator = creator;
            _initializer = initializer;
        }

        public sealed override ViewGeneratorKind Kind
        {
            get { return ViewGeneratorKind.Panel; }
        }

        Model _model;
        Func<DataSetControl> _creator;
        Action<DataSetControl> _initializer;

        internal override UIElement CreateUIElement()
        {
            return _creator();
        }

        internal override void Initialize(UIElement uiElement)
        {
            var dataSetControl = (DataSetControl)uiElement;
            dataSetControl.DataSet = GetDataSet(uiElement);

            if (_initializer != null)
                _initializer(dataSetControl);
        }

        private DataSet GetDataSet(UIElement uiElement)
        {
            var dataRowControl = uiElement.GetParent<DataRowControl>();
            return DataSet.Get(dataRowControl, _model);
        }
    }
}

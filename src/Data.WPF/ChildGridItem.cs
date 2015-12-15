using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class ChildGridItem : GridItem
    {
        internal ChildGridItem(GridTemplate template, Action<DataSetControl> initializer)
                : base(template.Model.GetParentModel())
        {
            Template = template;
            Initializer = initializer;
        }

        Action<DataSetControl> Initializer;

        internal override UIElement Create()
        {
            return new DataSetControl();
        }

        public GridTemplate Template { get; private set; }

        internal override void OnMounted(UIElement uiElement)
        {
            var dataSetControl = (DataSetControl)uiElement;
            var parentDataRowManager = dataSetControl.GetDataRowManager();
            dataSetControl.DataSetManager = parentDataRowManager.Children[Template.ChildOrdinal];
            Debug.Assert(dataSetControl.DataSetManager.Template == Template);
        }

        internal override void OnUnmounting(UIElement uiElement)
        {
            var dataSetControl = (DataSetControl)uiElement;
            dataSetControl.DataSetManager = null;
        }
    }
}

using DevZest.Data.Primitives;
using System;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    internal sealed class ChildGridItem : GridItem<DataSetControl>
    {
        internal ChildGridItem(GridTemplate template, Action<DataSetControl> initializer)
                : base(template.Model.GetParentModel())
        {
            Template = template;
            Initializer = initializer;
        }

        public GridTemplate Template { get; private set; }

        protected override void OnMounted(DataSetControl dataSetControl)
        {
            var parentDataRowManager = dataSetControl.GetDataRowManager();
            dataSetControl.Manager = parentDataRowManager.Children[Template.ChildOrdinal];
            Debug.Assert(dataSetControl.Manager.Template == Template);
        }

        protected override void OnUnmounting(DataSetControl dataSetControl)
        {
            dataSetControl.Manager = null;
        }
    }
}

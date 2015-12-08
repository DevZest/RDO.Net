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
            var parentDataRowView = dataSetControl.GetDataRowView();
            dataSetControl.View = parentDataRowView.ChildDataSetViews[Template.ChildOrdinal];
            Debug.Assert(dataSetControl.View.Template == Template);
        }

        protected override void OnUnmounting(DataSetControl dataSetControl)
        {
            dataSetControl.View = null;
        }
    }
}

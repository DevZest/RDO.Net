using DevZest.Data.Primitives;
using System;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    internal sealed class ChildGridItem : GridItem<DataSetGrid>
    {
        internal ChildGridItem(GridTemplate template, Action<DataSetGrid> initializer)
                : base(template.Model.GetParentModel())
        {
            Template = template;
            Initializer = initializer;
        }

        public GridTemplate Template { get; private set; }

        protected override void OnMounted(DataSetGrid dataSetGrid)
        {
            var parentDataRowView = dataSetGrid.GetDataRowView();
            dataSetGrid.View = parentDataRowView.ChildDataSetViews[Template.ChildOrdinal];
            Debug.Assert(dataSetGrid.View.Template == Template);
        }

        protected override void OnUnmounting(DataSetGrid dataSetGrid)
        {
            dataSetGrid.View = null;
        }
    }
}

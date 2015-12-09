using DevZest.Data.Primitives;
using System;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    internal sealed class ChildGridItem : GridItem<DataSetView>
    {
        internal ChildGridItem(GridTemplate template, Action<DataSetView> initializer)
                : base(template.Model.GetParentModel())
        {
            Template = template;
            Initializer = initializer;
        }

        public GridTemplate Template { get; private set; }

        protected override void OnMounted(DataSetView dataSetView)
        {
            var parentDataRowManager = dataSetView.GetDataRowManager();
            dataSetView.Manager = parentDataRowManager.Children[Template.ChildOrdinal];
            Debug.Assert(dataSetView.Manager.Template == Template);
        }

        protected override void OnUnmounting(DataSetView dataSetView)
        {
            dataSetView.Manager = null;
        }
    }
}

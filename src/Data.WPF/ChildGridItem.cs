using DevZest.Data.Primitives;
using System;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    internal sealed class ChildGridItem<T> : GridItem<T>
        where T : DataSetGrid, new()
    {
        internal ChildGridItem(GridTemplate template, Action<T> initializer)
                : base(template.Model.GetParentModel())
        {
            Template = template;
            Initializer = initializer;
        }

        public GridTemplate Template { get; private set; }

        protected override void OnMounted(T uiElement)
        {
            uiElement.View = uiElement.GetDataRowView()[Template.ChildOrdinal];
            Debug.Assert(uiElement.View.Template == Template);
        }

        protected override void UpdateSource(T element)
        {
        }

        protected override void UpdateTarget(T uiElement)
        {
        }

        protected override void UpdateValidation(T uiElement)
        {
        }

        protected override void OnUnmounting(T uiElement)
        {
            uiElement.View = null;
        }
    }
}

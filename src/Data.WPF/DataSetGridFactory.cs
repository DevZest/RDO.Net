using System;
using DevZest.Data.Primitives;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    public static class DataSetGridFactory
    {
        private sealed class ChildDataSetGridItem<T> : SetGridItem<T>
            where T : DataSetGrid, new()
        {
            public ChildDataSetGridItem(GridTemplate template, Action<T> initializer)
                : base(template.Model.GetParentModel(), initializer)
            {
                _template = template;
            }

            private GridTemplate _template;
            internal override GridTemplate Template
            {
                get { return _template; }
            }

            protected override void Initialize(DataRowView dataRowView, T uiElement)
            {
                uiElement.View = dataRowView[_template.ChildOrdinal];
                Debug.Assert(uiElement.View.Template == Template);
            }

            protected override void Refresh(DataRowView dataRowView, T uiElement)
            {
                Debug.Fail("DataSetGrid should not be refreshed when DataRowView changed.");
            }

            protected override void Cleanup(DataRowView dataRowView, T uiElement)
            {
                uiElement.View = null;
            }
        }

        public static SetGridItem ChildDataSetGrid<TModel, T>(this TModel childModel, Action<GridTemplate, TModel> templateInitializer, Action<T> initializer = null)
            where TModel : Model
            where T : DataSetGrid, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (templateInitializer == null)
                throw new ArgumentNullException(nameof(templateInitializer));

            var template = new GridTemplate(childModel);
            templateInitializer(template, childModel);
            template.Seal();
            return new ChildDataSetGridItem<T>(template, initializer);
        }
    }
}

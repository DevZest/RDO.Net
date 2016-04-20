using System;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class LayoutManagerTestsBase : RowManagerTestsBase
    {
        internal static LayoutManager CreateLayoutManager<T>(DataSet<T> dataSet, Action<TemplateBuilder, T> buildTemplateAction)
            where T : Model, new()
        {
            var template = new Template();
            using (var templateBuilder = new TemplateBuilder(template, dataSet.Model))
            {
                buildTemplateAction(templateBuilder, dataSet._);
                templateBuilder.BlockView((BlockView blockView) => blockView.InitializeElements(null))
                    .RowView((RowView rowView) => rowView.RowPresenter.Initialize(null));
            }
            var result = LayoutManager.Create(template, dataSet);
            result.InitializeElements(null);
            return result;
        }

    }
}

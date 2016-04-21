using Microsoft.VisualStudio.TestTools.UnitTesting;
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
                templateBuilder.BlockView((BlockView blockView) => blockView.CreateVisualTree());
            }
            var result = LayoutManager.Create(template, dataSet);
            result.InitializeElements(null);
            return result;
        }

        internal static void VerifyBlockViewRect(LayoutManager layoutManager, int blockViewIndex, Rect expectedRect)
        {
            var blockView = layoutManager.BlockViews[blockViewIndex];
            var rect = layoutManager.GetArrangeRect(blockView);
            Assert.AreEqual(expectedRect, rect);
        }

        internal static void VerifyRowRect(LayoutManager layoutManager, int blockViewIndex, int blockDimension, Rect expectedRect)
        {
            var blockView = layoutManager.BlockViews[blockViewIndex];
            var rect = layoutManager.GetArrangeRect(blockView, blockDimension);
            Assert.AreEqual(expectedRect, rect);
        }

        internal static void VerifyRowItemRect(LayoutManager layoutManager, RowPresenter row, int rowItemIndex, Rect expectedRect)
        {
            var blockView = layoutManager.BlockViews[row];
            var rowItems = row.RowItems;
            var rect = layoutManager.GetArrangeRect(blockView, rowItems[rowItemIndex]);
            Assert.AreEqual(expectedRect, rect);
        }
    }
}

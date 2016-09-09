using DevZest.Data.Windows.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class LayoutManagerTestsBase
    {
        internal static LayoutManager CreateLayoutManager<T>(DataSet<T> dataSet, Action<TemplateBuilder, T> buildTemplateAction)
            where T : Model, new()
        {
            var template = new Template();
            using (var templateBuilder = new TemplateBuilder(template, dataSet.Model))
            {
                buildTemplateAction(templateBuilder, dataSet._);
                templateBuilder.RowView<AutoInitRowView>()
                    .BlockView<AutoInitBlockView>();
            }
            var result = LayoutManager.Create(template, dataSet);
            result.InitializeElements(null);
            return result;
        }

        internal static void VerifyBlockViewRect(LayoutManager layoutManager, int blockViewIndex, Rect expectedRect)
        {
            var blockView = layoutManager.BlockViews[blockViewIndex];
            var rect = layoutManager.GetBlockRect(blockView);
            Assert.AreEqual(expectedRect, rect);
        }

        internal static void VerifyRowRect(LayoutManager layoutManager, int blockViewIndex, int blockDimension, Rect expectedRect)
        {
            var blockView = layoutManager.BlockViews[blockViewIndex];
            var rect = layoutManager.GetRowRect(blockView, blockDimension);
            Assert.AreEqual(expectedRect, rect);
        }

        internal static void VerifyRowItemRect(LayoutManager layoutManager, RowPresenter row, int rowItemIndex, Rect expectedRect)
        {
            var rowItems = row.RowItems;
            var rect = layoutManager.GetRowItemRect(row, rowItems[rowItemIndex]);
            Assert.AreEqual(expectedRect, rect);
        }
    }
}

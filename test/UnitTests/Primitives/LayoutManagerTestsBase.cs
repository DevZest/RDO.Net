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
            var blockView = blockViewIndex == -1 ? layoutManager.CurrentBlockView : layoutManager.BlockViewList[blockViewIndex];
            var rect = layoutManager.GetBlockRect(blockView);
            Assert.AreEqual(expectedRect, rect);
        }

        internal static void VerifyRowRect(LayoutManager layoutManager, int blockViewIndex, int blockDimension, Rect expectedRect)
        {
            var blockView = blockViewIndex == -1 ? layoutManager.CurrentBlockView : layoutManager.BlockViewList[blockViewIndex];
            var rect = layoutManager.GetRowRect(blockView, blockDimension);
            Assert.AreEqual(expectedRect, rect);
        }

        internal static void VerifyRowItemRect(LayoutManager layoutManager, RowPresenter row, int rowItemIndex, Rect expectedRect)
        {
            VerifyRowItemRect(layoutManager, row.View, rowItemIndex, expectedRect);
        }

        private static void VerifyRowItemRect(LayoutManager layoutManager, RowView rowView, int rowItemIndex, Rect expectedRect)
        {
            var rowItems = rowView.RowItems;
            var rect = layoutManager.GetRowItemRect(rowView, rowItems[rowItemIndex]);
            Assert.AreEqual(expectedRect, rect);
        }
    }
}

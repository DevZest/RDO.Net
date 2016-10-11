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
            var rect = layoutManager.GetRect(blockView);
            Assert.AreEqual(expectedRect, rect);
        }

        internal static void VerifyRowRect(LayoutManager layoutManager, int blockViewIndex, int blockDimension, Rect expectedRect)
        {
            var blockView = blockViewIndex == -1 ? layoutManager.CurrentBlockView : layoutManager.BlockViewList[blockViewIndex];
            var rect = layoutManager.GetRect(blockView, blockDimension);
            Assert.AreEqual(expectedRect, rect);
        }

        internal static void VerifyRowBindingRect(LayoutManager layoutManager, RowPresenter row, int rowBindingIndex, Rect expectedRect)
        {
            VerifyRowBindingRect(layoutManager, row.View, rowBindingIndex, expectedRect);
        }

        private static void VerifyRowBindingRect(LayoutManager layoutManager, RowView rowView, int rowBindingIndex, Rect expectedRect)
        {
            var rowBindings = rowView.RowBindings;
            var rect = layoutManager.GetRect(rowView, rowBindings[rowBindingIndex]);
            Assert.AreEqual(expectedRect, rect);
        }
    }
}

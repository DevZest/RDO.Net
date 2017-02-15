using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class LayoutManagerTestsBase
    {
        internal static void VerifyContainerViewRect(LayoutManager layoutManager, int blockViewIndex, Rect expectedRect)
        {
            var blockView = blockViewIndex == -1 ? layoutManager.CurrentContainerView : layoutManager.ContainerViewList[blockViewIndex];
            var rect = layoutManager.GetRect(blockView);
            Assert.AreEqual(expectedRect, rect);
        }

        internal static void VerifyRowRect(LayoutManager layoutManager, int blockViewIndex, int blockDimension, Rect expectedRect)
        {
            var blockView = (BlockView)(blockViewIndex == -1 ? layoutManager.CurrentContainerView : layoutManager.ContainerViewList[blockViewIndex]);
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

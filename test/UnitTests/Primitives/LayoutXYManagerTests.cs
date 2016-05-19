using DevZest.Data.Windows.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class LayoutXYManagerTests : LayoutManagerTestsBase
    {
        [TestMethod]
        public void LayoutXYManager_fixed_rows_only()
        {
            var dataSet = MockProductCategories(9, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("100")
                    .GridRows("20")
                    .RowRange(0, 0, 0, 0)
                    .Layout(Orientation.Vertical);
            });

            {
                var measuredSize = layoutManager.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
                Assert.AreEqual(8, layoutManager.BlockViews.Last.Ordinal);
                Assert.AreEqual(new Size(100, 180), measuredSize);
                Assert.AreEqual(100, layoutManager.ExtentX);
                Assert.AreEqual(180, layoutManager.ExtentY);
                Assert.AreEqual(100, layoutManager.ViewportX);
                Assert.AreEqual(180, layoutManager.ViewportY);
                Assert.AreEqual(0, layoutManager.ScrollOffsetX);
                Assert.AreEqual(0, layoutManager.ScrollOffsetY);
            }

            {
                var measuredSize = layoutManager.Measure(new Size(50, 30));
                Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
                Assert.AreEqual(1, layoutManager.BlockViews.Last.Ordinal);
                Assert.AreEqual(new Size(50, 30), measuredSize);
                Assert.AreEqual(100, layoutManager.ExtentX);
                Assert.AreEqual(180, layoutManager.ExtentY);
                Assert.AreEqual(50, layoutManager.ViewportX);
                Assert.AreEqual(30, layoutManager.ViewportY);
                Assert.AreEqual(0, layoutManager.ScrollOffsetX);
                Assert.AreEqual(0, layoutManager.ScrollOffsetY);
            }

            {
                layoutManager.ScrollOffsetX = 20;
                layoutManager.ScrollOffsetY = 30;
                var measuredSize = layoutManager.Measure(new Size(50, 30));
                Assert.AreEqual(1, layoutManager.BlockViews.First.Ordinal);
                Assert.AreEqual(2, layoutManager.BlockViews.Last.Ordinal);
                Assert.AreEqual(new Size(50, 30), measuredSize);
                Assert.AreEqual(100, layoutManager.ExtentX);
                Assert.AreEqual(180, layoutManager.ExtentY);
                Assert.AreEqual(50, layoutManager.ViewportX);
                Assert.AreEqual(30, layoutManager.ViewportY);
                Assert.AreEqual(20, layoutManager.ScrollOffsetX);
                Assert.AreEqual(30, layoutManager.ScrollOffsetY);
            }

            {
                layoutManager.ScrollOffsetY = 0;
                var measuredSize = layoutManager.Measure(new Size(50, 30));
                Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
                Assert.AreEqual(1, layoutManager.BlockViews.Last.Ordinal);
                Assert.AreEqual(new Size(50, 30), measuredSize);
                Assert.AreEqual(100, layoutManager.ExtentX);
                Assert.AreEqual(180, layoutManager.ExtentY);
                Assert.AreEqual(50, layoutManager.ViewportX);
                Assert.AreEqual(30, layoutManager.ViewportY);
                Assert.AreEqual(20, layoutManager.ScrollOffsetX);
                Assert.AreEqual(0, layoutManager.ScrollOffsetY);
            }
        }

        [TestMethod]
        public void LayoutXYManager_RowItem()
        {
            var dataSet = MockProductCategories(9, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("100")
                    .GridRows("Auto")
                    .Layout(Orientation.Vertical)
                    .RowItem().Bind((r, e) => { e.DesiredHeight = 10 * (r.Ordinal + 1); }).At(0, 0);
            });

            var measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(3, layoutManager.BlockViews.Last.Ordinal);

            var rowItem = layoutManager.Template.RowItemGroups[0][0];
            BlockView block;
                
            block = layoutManager.BlockViews[0];
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[1];
            Assert.AreEqual(new Rect(0, 10, 100, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[2];
            Assert.AreEqual(new Rect(0, 30, 100, 30), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[3];
            Assert.AreEqual(new Rect(0, 60, 100, 40), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowItemRect(block, rowItem));

            // Do another measure to detect bugs related to recycled elements
            measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(3, layoutManager.BlockViews.Last.Ordinal);

            rowItem = layoutManager.Template.RowItemGroups[0][0];

            block = layoutManager.BlockViews[0];
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 10), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[1];
            Assert.AreEqual(new Rect(0, 10, 100, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[2];
            Assert.AreEqual(new Rect(0, 30, 100, 30), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 30), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[3];
            Assert.AreEqual(new Rect(0, 60, 100, 40), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowItemRect(block, rowItem));
        }

        [TestMethod]
        public void LayoutXYManager_RowItem_multidimensional()
        {
            var dataSet = MockProductCategories(9, false);
            var layoutManager = (LayoutXYManager)CreateLayoutManager(dataSet, (builder, _) =>
            {
                builder.GridColumns("100")
                    .GridRows("Auto")
                    .Layout(Orientation.Vertical, 2)
                    .RowItem().Bind((r, e) => { e.DesiredHeight = 10 * (r.Ordinal + 1); }).At(0, 0);
            });

            var measuredSize = layoutManager.Measure(new Size(100, 100));
            Assert.AreEqual(0, layoutManager.BlockViews.First.Ordinal);
            Assert.AreEqual(2, layoutManager.BlockViews.Last.Ordinal);

            var rowItem = layoutManager.Template.RowItemGroups[0][0];
            BlockView block;

            block = layoutManager.BlockViews[0];
            Assert.AreEqual(new Rect(0, 0, 200, 20), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(100, 0, 100, 20), layoutManager.GetRowRect(block, 1));
            Assert.AreEqual(new Rect(0, 0, 100, 20), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[1];
            Assert.AreEqual(new Rect(0, 20, 200, 40), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(100, 0, 100, 40), layoutManager.GetRowRect(block, 1));
            Assert.AreEqual(new Rect(0, 0, 100, 40), layoutManager.GetRowItemRect(block, rowItem));

            block = layoutManager.BlockViews[2];
            Assert.AreEqual(new Rect(0, 60, 200, 60), layoutManager.GetBlockRect(block));
            Assert.AreEqual(new Rect(0, 0, 100, 60), layoutManager.GetRowRect(block, 0));
            Assert.AreEqual(new Rect(100, 0, 100, 60), layoutManager.GetRowRect(block, 1));
            Assert.AreEqual(new Rect(0, 0, 100, 60), layoutManager.GetRowItemRect(block, rowItem));
        }
    }
}
